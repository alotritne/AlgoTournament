using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace algotournament.Services
{
    public class JudgeBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JudgeBackgroundService> _logger;
        private readonly string _tempBasePath;

        public JudgeBackgroundService(IServiceProvider serviceProvider, ILogger<JudgeBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _tempBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "Submissions");
            
            // Ensure temp directory exists
            Directory.CreateDirectory(_tempBasePath);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Judge Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNextJobAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing judge job");
                }

                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Judge Background Service stopped");
        }

        private async Task ProcessNextJobAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var job = await context.JudgeQueues
                .Include(jq => jq.Submission)
                    .ThenInclude(s => s.Problem)
                .Where(jq => jq.Status == JudgeQueueStatus.Pending)
                .OrderBy(jq => jq.Priority)
                .ThenBy(jq => jq.QueuedAt)
                .FirstOrDefaultAsync(stoppingToken);

            if (job == null)
            {
                return;
            }

            job.Status = JudgeQueueStatus.Processing;
            job.StartedAt = DateTime.UtcNow;
            job.AssignedWorker = Environment.MachineName;
            await context.SaveChangesAsync(stoppingToken);

            try
            {
                await JudgeSubmissionAsync(job.Submission, context, stoppingToken);
                
                job.Status = JudgeQueueStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(stoppingToken);

                var submissionId = job.Submission.Id;
                var duelMatchId = await context.Submissions
                    .Where(s => s.Id == submissionId)
                    .Select(s => s.DuelMatchId)
                    .FirstOrDefaultAsync(stoppingToken);

                if (duelMatchId.HasValue)
                {
                    var duelNotifier = scope.ServiceProvider.GetRequiredService<algotournament.Services.Abstractions.IDuelJudgeNotifier>();
                    await duelNotifier.NotifySubmissionJudgedAsync(submissionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error judging submission {SubmissionId}", job.Submission.Id);
                
                // Update submission status to CompilationError if it's still Compiling
                var submission = await context.Submissions.FindAsync(job.Submission.Id);
                if (submission != null && submission.Status == SubmissionStatus.Compiling)
                {
                    submission.Status = SubmissionStatus.CompilationError;
                    submission.CompileError = ex.Message;
                    submission.JudgedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync(stoppingToken);
                }

                if (submission?.DuelMatchId != null)
                {
                    var duelNotifier = scope.ServiceProvider.GetRequiredService<algotournament.Services.Abstractions.IDuelJudgeNotifier>();
                    await duelNotifier.NotifySubmissionJudgedAsync(submission.Id);
                }
                
                job.Status = JudgeQueueStatus.Failed;
                job.ErrorMessage = ex.Message;
                job.CompletedAt = DateTime.UtcNow;
                job.RetryCount++;
                
                await context.SaveChangesAsync(stoppingToken);
            }
            finally
            {
                CleanupTempDirectory(job.Submission.Id);
            }
        }

        private async Task JudgeSubmissionAsync(Submission submission, ApplicationDbContext context, CancellationToken stoppingToken)
        {
            var problem = submission.Problem;
            var contest = submission.Contest;
            var scoringMode = contest?.ScoringMode ?? ScoringMode.ACM; // Default to ACM if no contest
            
            var testCases = await context.TestCases
                .Where(tc => tc.ProblemId == problem.Id)
                .OrderBy(tc => tc.Order)
                .ToListAsync(stoppingToken);

            if (!testCases.Any())
            {
                await UpdateSubmissionStatusAsync(context, submission.Id, SubmissionStatus.WrongAnswer, 0, 0, null, "No test cases found");
                return;
            }

            // Update status to Compiling
            await UpdateSubmissionStatusAsync(context, submission.Id, SubmissionStatus.Compiling);

            var submissionDir = Path.Combine(_tempBasePath, submission.Id.ToString());
            Directory.CreateDirectory(submissionDir);

            var sourceFile = Path.Combine(submissionDir, "main.cpp");
            var executableFile = Path.Combine(submissionDir, "main.exe");

            // Write source code
            await File.WriteAllTextAsync(sourceFile, submission.SourceCode, stoppingToken);

            // Compile
            var compileResult = await CompileCodeAsync(sourceFile, executableFile, submission.Language);

            if (!compileResult.Success)
            {
                await UpdateSubmissionStatusAsync(context, submission.Id, SubmissionStatus.CompilationError, 0, 0, compileResult.ErrorMessage);
                return;
            }

            // Update status to Running
            await UpdateSubmissionStatusAsync(context, submission.Id, SubmissionStatus.Running);

            var totalScore = 0;
            var maxScore = testCases.Sum(tc => tc.Points);
            var allPassed = true;

            foreach (var testCase in testCases)
            {
                var result = await RunTestCaseAsync(executableFile, testCase, problem.TimeLimitMs, stoppingToken);

                await SaveSubmissionResultAsync(context, submission.Id, testCase.Id, result.Status, result.ExecutionTimeMs, result.MemoryUsedKB, result.Output, result.ErrorMessage);

                if (result.Status == SubmissionStatus.Accepted)
                {
                    totalScore += testCase.Points;
                }
                else
                {
                    allPassed = false;
                    
                    // For ACM mode, one wrong answer means 0 points
                    if (scoringMode == ScoringMode.ACM)
                    {
                        totalScore = 0;
                        break;
                    }
                }
            }

            // Calculate final score
            var finalScore = maxScore > 0 ? (int)((double)totalScore / maxScore * 100) : 0;

            var finalStatus = allPassed ? SubmissionStatus.Accepted : SubmissionStatus.WrongAnswer;
            var executionTime = await context.SubmissionResults
                .Where(sr => sr.SubmissionId == submission.Id)
                .MaxAsync(sr => sr.ExecutionTimeMs, stoppingToken);

            await UpdateSubmissionStatusAsync(context, submission.Id, finalStatus, finalScore, executionTime);

            // Update user statistics if accepted
            _logger.LogInformation("Submission {SubmissionId} final status: {FinalStatus}", submission.Id, finalStatus);
            if (finalStatus == SubmissionStatus.Accepted)
            {
                _logger.LogInformation("Calling UpdateUserStatisticsAsync for user {UserId}, problem {ProblemId}", submission.UserId, submission.ProblemId);
                await UpdateUserStatisticsAsync(context, submission.UserId, submission.ProblemId, stoppingToken);
            }
        }

        private async Task<(bool Success, string? ErrorMessage)> CompileCodeAsync(string sourceFile, string executableFile, ProgrammingLanguage language)
        {
            string compiler, arguments;
            
            // Check if g++ is available (Linux/Mac) or use cl/MSVC (Windows)
            if (OperatingSystem.IsWindows())
            {
                // Try to use cl.exe (MSVC) or g++ if available
                compiler = "cl.exe";
                var msvcStdFlag = language == ProgrammingLanguage.Cpp17 ? "/std:c++17" : "/std:c++20";
                arguments = $"{msvcStdFlag} /O2 /Fe:\"{executableFile}\" \"{sourceFile}\"";
                
                // Check if cl.exe is available
                try
                {
                    var testProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "where",
                        Arguments = "cl.exe",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    
                    if (testProcess != null)
                    {
                        await testProcess.WaitForExitAsync();
                        if (testProcess.ExitCode != 0)
                        {
                            // cl.exe not found, try g++
                            compiler = "g++";
                            var gccStdFlag = language == ProgrammingLanguage.Cpp17 ? "-std=c++17" : "-std=c++20";
                            arguments = $"{gccStdFlag} -O2 \"{sourceFile}\" -o \"{executableFile}\"";
                        }
                    }
                }
                catch
                {
                    // If where command fails, try g++ directly
                    compiler = "g++";
                    var gccStdFlag = language == ProgrammingLanguage.Cpp17 ? "-std=c++17" : "-std=c++20";
                    arguments = $"{gccStdFlag} -O2 \"{sourceFile}\" -o \"{executableFile}\"";
                }
            }
            else
            {
                // Linux/Mac - use g++
                compiler = "g++";
                var gccStdFlag = language == ProgrammingLanguage.Cpp17 ? "-std=c++17" : "-std=c++20";
                arguments = $"{gccStdFlag} -O2 \"{sourceFile}\" -o \"{executableFile}\"";
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = compiler,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return (false, $"Failed to start compiler '{compiler}'. Please ensure {compiler} is installed and available in PATH.");
            }

            var output = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                return (false, output);
            }

            return (true, null);
        }

        private async Task<(SubmissionStatus Status, int ExecutionTimeMs, int MemoryUsedKB, string? Output, string? ErrorMessage)> RunTestCaseAsync(
            string executableFile, 
            TestCase testCase, 
            int timeLimitMs, 
            CancellationToken stoppingToken)
        {
            var inputFile = Path.Combine(Path.GetTempPath(), $"input_{Guid.NewGuid()}.txt");
            var outputFile = Path.Combine(Path.GetTempPath(), $"output_{Guid.NewGuid()}.txt");

            try
            {
                // Write input file
                await File.WriteAllTextAsync(inputFile, testCase.Input, stoppingToken);

                var processInfo = new ProcessStartInfo
                {
                    FileName = executableFile,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    return (SubmissionStatus.RuntimeError, 0, 0, null, "Failed to start process");
                }

                // Write input to process
                await process.StandardInput.WriteAsync(testCase.Input);
                process.StandardInput.Close();

                // Read output
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                // Start timing
                var stopwatch = Stopwatch.StartNew();

                // Wait for process to complete or timeout
                var timeoutTask = Task.Delay(timeLimitMs + 100, stoppingToken);
                var exitTask = process.WaitForExitAsync(stoppingToken);

                var completedTask = await Task.WhenAny(timeoutTask, exitTask);

                stopwatch.Stop();

                if (completedTask == timeoutTask)
                {
                    process.Kill();
                    return (SubmissionStatus.TimeLimitExceeded, timeLimitMs, 0, null, null);
                }

                if (process.ExitCode != 0)
                {
                    return (SubmissionStatus.RuntimeError, (int)stopwatch.ElapsedMilliseconds, 0, null, error);
                }

                // Compare output
                var expected = testCase.ExpectedOutput.Trim();
                var actual = output.Trim();

                if (expected == actual)
                {
                    return (SubmissionStatus.Accepted, (int)stopwatch.ElapsedMilliseconds, 0, output, null);
                }
                else
                {
                    return (SubmissionStatus.WrongAnswer, (int)stopwatch.ElapsedMilliseconds, 0, output, null);
                }
            }
            finally
            {
                // Cleanup temp files
                if (File.Exists(inputFile))
                    File.Delete(inputFile);
                if (File.Exists(outputFile))
                    File.Delete(outputFile);
            }
        }

        private async Task UpdateSubmissionStatusAsync(ApplicationDbContext context, int submissionId, SubmissionStatus status, int score = 0, int executionTimeMs = 0, string? compileError = null, string? runtimeError = null)
        {
            var submission = await context.Submissions.FindAsync(submissionId);
            if (submission != null)
            {
                submission.Status = status;
                submission.Score = score;
                submission.ExecutionTimeMs = executionTimeMs;
                submission.CompileError = compileError;
                submission.RuntimeError = runtimeError;
                submission.JudgedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        private async Task SaveSubmissionResultAsync(ApplicationDbContext context, int submissionId, int testCaseId, SubmissionStatus status, int executionTimeMs = 0, int memoryUsedKB = 0, string? output = null, string? errorMessage = null)
        {
            var result = new SubmissionResult
            {
                SubmissionId = submissionId,
                TestCaseId = testCaseId,
                Status = status,
                ExecutionTimeMs = executionTimeMs,
                MemoryUsedKB = memoryUsedKB,
                Output = output,
                ErrorMessage = errorMessage
            };

            context.SubmissionResults.Add(result);
            await context.SaveChangesAsync();
        }

        private async Task UpdateUserStatisticsAsync(ApplicationDbContext context, string userId, int problemId, CancellationToken stoppingToken)
        {
            _logger.LogInformation("UpdateUserStatisticsAsync called for user {UserId}, problem {ProblemId}", userId, problemId);
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return;
            }

            // Get the current submission ID (the one that was just accepted)
            var currentSubmissionId = await context.Submissions
                .Where(s => s.UserId == userId && s.ProblemId == problemId)
                .MaxAsync(s => s.Id, stoppingToken);

            _logger.LogInformation("Current submission ID: {CurrentSubmissionId}", currentSubmissionId);

            // Check if this is the first time the user solved this problem
            var previouslySolved = await context.Submissions
                .AnyAsync(s => s.UserId == userId
                    && s.ProblemId == problemId
                    && s.Status == SubmissionStatus.Accepted
                    && s.Id < currentSubmissionId, stoppingToken);

            _logger.LogInformation("Previously solved: {PreviouslySolved}", previouslySolved);

            if (!previouslySolved)
            {
                // First time solving this problem
                user.ProblemsSolved++;

                // ELO rating calculation
                // Base ELO gain for solving a problem: +25
                // Adjust based on problem difficulty
                var problem = await context.Problems.FindAsync(problemId);
                var eloGain = 25;

                if (problem != null)
                {
                    // Adjust ELO gain based on problem difficulty rating
                    // Higher difficulty = more ELO gain
                    eloGain += (int)(problem.DifficultyRating * 0.5);
                }

                user.Rating += eloGain;

                _logger.LogInformation("Updated user {UserId} statistics: ProblemsSolved={ProblemsSolved}, Rating={Rating}, ELOGain={ELOGain}", userId, user.ProblemsSolved, user.Rating, eloGain);

                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Database save completed for user {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("User {UserId} already solved problem {ProblemId} before, skipping statistics update", userId, problemId);
            }
        }

        private void CleanupTempDirectory(int submissionId)
        {
            try
            {
                var submissionDir = Path.Combine(_tempBasePath, submissionId.ToString());
                if (Directory.Exists(submissionDir))
                {
                    Directory.Delete(submissionDir, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup temp directory for submission {SubmissionId}", submissionId);
            }
        }
    }
}
