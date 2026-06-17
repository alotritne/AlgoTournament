using algotournament.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created and migrated
            await context.Database.MigrateAsync();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed SuperAdmin User
            await SeedSuperAdminAsync(userManager, roleManager);

            // Seed Seasons
            await SeedSeasonsAsync(context);

            // Seed Tournaments
            await SeedTournamentsAsync(context);

            // Seed other initial data if needed
            await SeedAnnouncementsAsync(context);
            
            // Seed Hello World problem
            await SeedHelloWorld.SeedHelloWorldProblemAsync(context);
            
            // Seed custom data: 1 tournament, 3 contests, 10 problems
            await SeedCustomDataAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "SuperAdmin", "Admin", "Judge", "ContestManager", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"Created role: {role}");
                }
            }
        }

        private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            const string adminEmail = "admin@algotournament.com";
            const string adminPassword = "Admin@123";
            const string adminHandle = "admin";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Handle = adminHandle,
                    Rating = 3000,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "SuperAdmin");
                    Console.WriteLine($"Created SuperAdmin user: {adminEmail}");
                }
                else
                {
                    Console.WriteLine($"Failed to create SuperAdmin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Ensure admin is in SuperAdmin role
                if (!await userManager.IsInRoleAsync(adminUser, "SuperAdmin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                }
            }
        }

        private static async Task SeedSeasonsAsync(ApplicationDbContext context)
        {
            if (!context.Seasons.Any())
            {
                var seasons = new[]
                {
                    new Season
                    {
                        Name = "Season 2024",
                        Description = "Mùa giải 2024",
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddYears(1),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Seasons.AddRangeAsync(seasons);
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded initial seasons");
            }
        }

        private static async Task SeedTournamentsAsync(ApplicationDbContext context)
        {
            if (!context.Tournaments.Any())
            {
                // Get the first season
                var season = await context.Seasons.FirstOrDefaultAsync();
                if (season == null)
                {
                    Console.WriteLine("No season found, skipping tournament seeding");
                    return;
                }

                var tournaments = new[]
                {
                    new Tournament
                    {
                        Name = "Global Round",
                        Description = "Global tournament for all participants",
                        SeasonId = season.Id,
                        IsPrivate = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Tournament
                    {
                        Name = "School Championship",
                        Description = "Tournament for school students",
                        SeasonId = season.Id,
                        IsPrivate = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Tournament
                    {
                        Name = "Practice Arena",
                        Description = "Practice tournament for beginners",
                        SeasonId = season.Id,
                        IsPrivate = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Tournaments.AddRangeAsync(tournaments);
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded initial tournaments");
            }
        }

        private static async Task SeedAnnouncementsAsync(ApplicationDbContext context)
        {
            if (!context.Announcements.Any())
            {
                var adminUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == "admin@algotournament.com");

                if (adminUser == null)
                {
                    return;
                }

                var announcements = new[]
                {
                    new Announcement
                    {
                        Title = "Welcome to Algo Tournament!",
                        Content = "Welcome to the new Algo Tournament platform. Get started by exploring problems, joining contests, and improving your algorithmic skills.",
                        CreatedBy = adminUser.Id,
                        IsGlobal = true,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(30),
                        IsActive = true
                    },
                    new Announcement
                    {
                        Title = "First Global Round Coming Soon",
                        Content = "Our first Global Round is scheduled for next month. Register now to participate!",
                        CreatedBy = adminUser.Id,
                        IsGlobal = true,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(15),
                        IsActive = true
                    }
                };

                await context.Announcements.AddRangeAsync(announcements);
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded initial announcements");
            }
        }

        private static async Task SeedCustomDataAsync(ApplicationDbContext context)
        {
            var adminUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "admin@algotournament.com");

            if (adminUser == null)
            {
                Console.WriteLine("Admin user not found, skipping custom data seeding");
                return;
            }

            // Check if custom tournament already exists and remove it to force re-seeding
            var existingTournament = await context.Tournaments
                .FirstOrDefaultAsync(t => t.Name == "Vietnam Algorithm Championship 2024");
            
            if (existingTournament != null)
            {
                Console.WriteLine("Found existing tournament, removing for re-seeding...");
                
                // First delete related contests (due to foreign key constraint)
                var relatedContests = await context.Contests
                    .Where(c => c.TournamentId == existingTournament.Id)
                    .ToListAsync();
                
                if (relatedContests.Any())
                {
                    Console.WriteLine($"Removing {relatedContests.Count} related contests...");
                    
                    // Delete contest-problem relationships
                    var contestIds = relatedContests.Select(c => c.Id).ToList();
                    var existingContestProblems = await context.ContestProblems
                        .Where(cp => contestIds.Contains(cp.ContestId))
                        .ToListAsync();
                    if (existingContestProblems.Any())
                    {
                        context.ContestProblems.RemoveRange(existingContestProblems);
                        await context.SaveChangesAsync();
                    }
                    
                    context.Contests.RemoveRange(relatedContests);
                    await context.SaveChangesAsync();
                }
                
                // Then delete the tournament
                context.Tournaments.Remove(existingTournament);
                await context.SaveChangesAsync();
            }
            
            // Delete existing problems with custom data slugs to avoid duplicates
            var customSlugs = new[] 
            { 
                "tong-hai-so", "uoc-so-chung-lon-nhat", "fibonacci", "sap-xep-mang", 
                "tim-kiem-nhi-phan", "so-nguyen-to", "cong-ma-tran", "day-con-tang-dai-nhat", 
                "can-bang-thap", "duong-di-ngan-nhat" 
            };
            
            var existingProblems = await context.Problems
                .Where(p => customSlugs.Contains(p.Slug))
                .ToListAsync();
            
            if (existingProblems.Any())
            {
                Console.WriteLine($"Removing {existingProblems.Count} existing problems...");
                
                var problemIds = existingProblems.Select(p => p.Id).ToList();
                
                // Delete duel rooms and related data
                var duelRooms = await context.DuelRooms
                    .Where(dr => problemIds.Contains(dr.ProblemId))
                    .ToListAsync();
                if (duelRooms.Any())
                {
                    var duelRoomIds = duelRooms.Select(dr => dr.Id).ToList();
                    
                    // Delete duel matches related to these duel rooms
                    var duelMatches = await context.DuelMatches
                        .Where(dm => duelRoomIds.Contains(dm.DuelRoomId))
                        .ToListAsync();
                    if (duelMatches.Any())
                    {
                        var duelMatchIds = duelMatches.Select(dm => dm.Id).ToList();
                        
                        // Delete submissions that reference these duel matches
                        var submissions = await context.Submissions
                            .Where(s => s.DuelMatchId.HasValue && duelMatchIds.Contains(s.DuelMatchId.Value))
                            .ToListAsync();
                        if (submissions.Any())
                        {
                            // Delete submission results for these submissions
                            var submissionIds = submissions.Select(s => s.Id).ToList();
                            var submissionResults = await context.SubmissionResults
                                .Where(sr => submissionIds.Contains(sr.SubmissionId))
                                .ToListAsync();
                            if (submissionResults.Any())
                            {
                                context.SubmissionResults.RemoveRange(submissionResults);
                                await context.SaveChangesAsync();
                            }
                            
                            context.Submissions.RemoveRange(submissions);
                            await context.SaveChangesAsync();
                        }
                        
                        context.DuelMatches.RemoveRange(duelMatches);
                        await context.SaveChangesAsync();
                    }
                    
                    context.DuelRooms.RemoveRange(duelRooms);
                    await context.SaveChangesAsync();
                }
                
                // Delete test cases for these problems
                var existingTestCases = await context.TestCases
                    .Where(tc => problemIds.Contains(tc.ProblemId))
                    .ToListAsync();
                if (existingTestCases.Any())
                {
                    // Delete submission results that reference these test cases
                    var testCaseIds = existingTestCases.Select(tc => tc.Id).ToList();
                    var submissionResults = await context.SubmissionResults
                        .Where(sr => testCaseIds.Contains(sr.TestCaseId))
                        .ToListAsync();
                    if (submissionResults.Any())
                    {
                        context.SubmissionResults.RemoveRange(submissionResults);
                        await context.SaveChangesAsync();
                    }
                    
                    context.TestCases.RemoveRange(existingTestCases);
                    await context.SaveChangesAsync();
                }
                
                context.Problems.RemoveRange(existingProblems);
                await context.SaveChangesAsync();
            }
            
            Console.WriteLine("Starting custom data seeding...");

            // Get the first season
            var season = await context.Seasons.FirstOrDefaultAsync();
            if (season == null)
            {
                Console.WriteLine("No season found, skipping custom data seeding");
                return;
            }

            // Create Tournament
            var tournament = new Tournament
            {
                Name = "Vietnam Algorithm Championship 2024",
                Description = "Giải đấu thuật toán hàng đầu Việt Nam dành cho các lập trình viên trẻ tài năng.",
                SeasonId = season.Id,
                IsPrivate = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Tournaments.AddAsync(tournament);
            await context.SaveChangesAsync();
            Console.WriteLine("Created tournament: Vietnam Algorithm Championship 2024");

            // Create Contests
            var contest1 = new Contest
            {
                Name = "VAC 2024 - Vòng Loại",
                Description = "Vòng loại giải đấu Vietnam Algorithm Championship 2024",
                TournamentId = tournament.Id,
                StartTime = DateTime.UtcNow.AddDays(7),
                EndTime = DateTime.UtcNow.AddDays(7).AddHours(2),
                DurationMinutes = 120,
                IsRated = true,
                ScoringMode = ScoringMode.ACM,
                CreatedAt = DateTime.UtcNow
            };

            var contest2 = new Contest
            {
                Name = "VAC 2024 - Vòng Chung Kết",
                Description = "Vòng chung kết giải đấu Vietnam Algorithm Championship 2024",
                TournamentId = tournament.Id,
                StartTime = DateTime.UtcNow.AddDays(14),
                EndTime = DateTime.UtcNow.AddDays(14).AddHours(3),
                DurationMinutes = 180,
                IsRated = true,
                ScoringMode = ScoringMode.ACM,
                CreatedAt = DateTime.UtcNow
            };

            var contest3 = new Contest
            {
                Name = "VAC 2024 - Vòng Thử Thách",
                Description = "Vòng thử thách đặc biệt với các bài toán khó",
                TournamentId = tournament.Id,
                StartTime = DateTime.UtcNow.AddDays(21),
                EndTime = DateTime.UtcNow.AddDays(21).AddHours(2),
                DurationMinutes = 120,
                IsRated = true,
                ScoringMode = ScoringMode.OI,
                CreatedAt = DateTime.UtcNow
            };

            await context.Contests.AddRangeAsync(contest1, contest2, contest3);
            await context.SaveChangesAsync();
            Console.WriteLine("Created 3 contests");

            // Create Problems
            var problems = new[]
            {
                new Problem
                {
                    Slug = "tong-hai-so",
                    TitleVi = "Tổng Hai Số",
                    StatementVi = "Cho hai số nguyên a và b. Hãy tính tổng của chúng.",
                    InputDescriptionVi = "Dòng đầu tiên chứa số nguyên a. Dòng thứ hai chứa số nguyên b.",
                    OutputDescriptionVi = "In ra tổng của a và b.",
                    ConstraintsVi = "-10^9 ≤ a, b ≤ 10^9",
                    ExplanationVi = "Chỉ cần cộng hai số lại với nhau.",
                    TitleEn = "Sum of Two Numbers",
                    StatementEn = "Given two integers a and b. Calculate their sum.",
                    InputDescriptionEn = "The first line contains integer a. The second line contains integer b.",
                    OutputDescriptionEn = "Output the sum of a and b.",
                    ConstraintsEn = "-10^9 ≤ a, b ≤ 10^9",
                    ExplanationEn = "Simply add the two numbers together.",
                    SampleInput = "5\n7",
                    SampleOutput = "12",
                    TimeLimitMs = 1000,
                    MemoryLimitMB = 256,
                    DifficultyRating = 800,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsEnglishTranslated = true
                },
                new Problem
                {
                    Slug = "uoc-so-chung-lon-nhat",
                    TitleVi = "Ước Số Chung Lớn Nhất",
                    StatementVi = "Cho hai số nguyên dương a và b. Hãy tìm ước số chung lớn nhất của chúng.",
                    InputDescriptionVi = "Dòng đầu tiên chứa số nguyên dương a. Dòng thứ hai chứa số nguyên dương b.",
                    OutputDescriptionVi = "In ra ước số chung lớn nhất của a và b.",
                    ConstraintsVi = "1 ≤ a, b ≤ 10^9",
                    ExplanationVi = "Sử dụng thuật toán Euclid để tìm GCD.",
                    TitleEn = "Greatest Common Divisor",
                    StatementEn = "Given two positive integers a and b. Find their greatest common divisor.",
                    InputDescriptionEn = "The first line contains positive integer a. The second line contains positive integer b.",
                    OutputDescriptionEn = "Output the GCD of a and b.",
                    ConstraintsEn = "1 ≤ a, b ≤ 10^9",
                    ExplanationEn = "Use Euclidean algorithm to find GCD.",
                    SampleInput = "12\n18",
                    SampleOutput = "6",
                    TimeLimitMs = 1000,
                    MemoryLimitMB = 256,
                    DifficultyRating = 1000,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsEnglishTranslated = true
                },
                new Problem
                {
                    Slug = "fibonacci",
                    TitleVi = "Dãy Fibonacci",
                    StatementVi = "Cho số nguyên n. Hãy tìm số Fibonacci thứ n.",
                    InputDescriptionVi = "Dòng đầu tiên chứa số nguyên n (1 ≤ n ≤ 90).",
                    OutputDescriptionVi = "In ra số Fibonacci thứ n.",
                    ConstraintsVi = "1 ≤ n ≤ 90",
                    ExplanationVi = "F(1) = 1, F(2) = 1, F(n) = F(n-1) + F(n-2) với n > 2.",
                    TitleEn = "Fibonacci Sequence",
                    StatementEn = "Given integer n. Find the nth Fibonacci number.",
                    InputDescriptionEn = "The first line contains integer n (1 ≤ n ≤ 90).",
                    OutputDescriptionEn = "Output the nth Fibonacci number.",
                    ConstraintsEn = "1 ≤ n ≤ 90",
                    ExplanationEn = "F(1) = 1, F(2) = 1, F(n) = F(n-1) + F(n-2) for n > 2.",
                    SampleInput = "10",
                    SampleOutput = "55",
                    TimeLimitMs = 1000,
                    MemoryLimitMB = 256,
                    DifficultyRating = 1200,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsEnglishTranslated = true
                },
                new Problem
                {
                    Slug = "sap-xep-mang",
                    TitleVi = "Sắp Xếp Mảng",
                    StatementVi = "Cho mảng n số nguyên. Hãy sắp xếp mảng theo thứ tự tăng dần.",
                    InputDescriptionVi = "Dòng đầu tiên chứa số nguyên n (1 ≤ n ≤ 10^5). Dòng thứ hai chứa n số nguyên.",
                    OutputDescriptionVi = "In ra mảng đã sắp xếp tăng dần.",
                    ConstraintsVi = "1 ≤ n ≤ 10^5, -10^9 ≤ a[i] ≤ 10^9",
                    ExplanationVi = "Sử dụng thuật toán sắp xếp có độ phức tạp O(n log n).",
                    TitleEn = "Array Sorting",
                    StatementEn = "Given an array of n integers. Sort the array in ascending order.",
                    InputDescriptionEn = "The first line contains integer n (1 ≤ n ≤ 10^5). The second line contains n integers.",
                    OutputDescriptionEn = "Output the sorted array in ascending order.",
                    ConstraintsEn = "1 ≤ n ≤ 10^5, -10^9 ≤ a[i] ≤ 10^9",
                    ExplanationEn = "Use sorting algorithm with O(n log n) complexity.",
                    SampleInput = "5\n3 1 4 2 5",
                    SampleOutput = "1 2 3 4 5",
                    TimeLimitMs = 1000,
                    MemoryLimitMB = 256,
                    DifficultyRating = 900,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsEnglishTranslated = true
                },
                new Problem
                {
                    Slug = "tim-kiem-nhi-phan",
                    TitleVi = "Tìm Kiếm Nhị Phân",
                    StatementVi = "Cho mảng đã sắp xếp tăng dần và số x. Hãy tìm vị trí của x trong mảng.",
                    InputDescriptionVi = "Dòng đầu tiên chứa n và x. Dòng thứ hai chứa n số nguyên đã sắp xếp.",
                    OutputDescriptionVi = "In ra vị trí của x (1-indexed) hoặc -1 nếu không tìm thấy.",
                    ConstraintsVi = "1 ≤ n ≤ 10^5, -10^9 ≤ a[i] ≤ 10^9",
                    ExplanationVi = "Sử dụng thuật toán tìm kiếm nhị phân.",
                    TitleEn = "Binary Search",
                    StatementEn = "Given a sorted array and number x. Find the position of x in the array.",
                    InputDescriptionEn = "The first line contains n and x. The second line contains n sorted integers.",
                    OutputDescriptionEn = "Output the position of x (1-indexed) or -1 if not found.",
                    ConstraintsEn = "1 ≤ n ≤ 10^5, -10^9 ≤ a[i] ≤ 10^9",
                    ExplanationEn = "Use binary search algorithm.",
                    SampleInput = "5 3\n1 2 3 4 5",
                    SampleOutput = "3",
                    TimeLimitMs = 1000,
                    MemoryLimitMB = 256,
                    DifficultyRating = 1100,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsEnglishTranslated = true
                },
                new Problem
                {
                    Slug = "so-nguyen-to",
                    TitleVi = "Số Nguyên Tố",
                    StatementVi = "Cho số nguyên n. Kiểm tra xem n có phải là số nguyên tố không.",
                    InputDescriptionVi = "Dòng đầu tiên chứa số nguyên n (1 ≤ n ≤ 10^12).",
                    OutputDescriptionVi = "In ra \"YES\" nếu n là số nguyên tố, \"NO\" nếu không.",
                    ConstraintsVi = "1 ≤ n ≤ 10^12",
                    ExplanationVi = "Sử dụng thuật toán kiểm tra số nguyên tố.",
                    TitleEn = "Prime Number",
                    StatementEn = "Given integer n. Check if n is a prime number.",
                    InputDescriptionEn = "The first line contains integer n (1 ≤ n ≤ 10^12).",
                    OutputDescriptionEn = "Output \"YES\" if n is prime, \"NO\" otherwise.",
                    ConstraintsEn = "1 ≤ n ≤ 10^12",
                    ExplanationEn = "Use primality testing algorithm.",
                    SampleInput = "17",
                    SampleOutput = "YES",
                    TimeLimitMs = 1000,
                    MemoryLimitMB = 256,
                    DifficultyRating = 1300,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsEnglishTranslated = true
                },
                new Problem
                {
                    Slug = "cong-ma-tran",
                    TitleVi = "Cộng Ma Trận",
                    StatementVi = "Cho hai ma trận A và B cùng kích thước n x m. Hãy tính ma trận tổng C = A + B.",
                    InputDescriptionVi = "Dòng đầu tiên chứa n và m. n dòng tiếp theo chứa ma trận A. n dòng tiếp theo chứa ma trận B.",
                    OutputDescriptionVi = "In ra ma trận tổng C.",
                    ConstraintsVi = "1 ≤ n, m ≤ 100, -10^9 ≤ A[i][j], B[i][j] ≤ 10^9",
                    ExplanationVi = "C[i][j] = A[i][j] + B[i][j].",
                    TitleEn = "Matrix Addition",
                    StatementEn = "Given two matrices A and B of size n x m. Calculate the sum matrix C = A + B.",
                    InputDescriptionEn = "The first line contains n and m. Next n lines contain matrix A. Next n lines contain matrix B.",
                    OutputDescriptionEn = "Output the sum matrix C.",
                    ConstraintsEn = "1 ≤ n, m ≤ 100, -10^9 ≤ A[i][j], B[i][j] ≤ 10^9",
                    ExplanationEn = "C[i][j] = A[i][j] + B[i][j].",
                    SampleInput = "2 2\n1 2\n3 4\n5 6\n7 8",
                    SampleOutput = "6 8\n10 12",
                    TimeLimitMs = 1000,
                    MemoryLimitMB = 256,
                    DifficultyRating = 1000,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsEnglishTranslated = true
                },
                new Problem
                {
                    Slug = "day-con-tang-dai-nhat",
                    TitleVi = "Dãy Con Tăng Dài Nhất",
                    StatementVi = "Cho dãy số a1, a2, ..., an. Tìm độ dài dãy con tăng dài nhất.",
                    InputDescriptionVi = "Dòng đầu tiên chứa n. Dòng thứ hai chứa n số nguyên.",
                    OutputDescriptionVi = "In ra độ dài dãy con tăng dài nhất.",
                    ConstraintsVi = "1 ≤ n ≤ 10^5, -10^9 ≤ a[i] ≤ 10^9",
                    ExplanationVi = "Sử dụng quy hoạch động hoặc binary search.",
                    TitleEn = "Longest Increasing Subsequence",
                    StatementEn = "Given sequence a1, a2, ..., an. Find the length of the longest increasing subsequence.",
                    InputDescriptionEn = "The first line contains n. The second line contains n integers.",
                    OutputDescriptionEn = "Output the length of the longest increasing subsequence.",
                    ConstraintsEn = "1 ≤ n ≤ 10^5, -10^9 ≤ a[i] ≤ 10^9",
                    ExplanationEn = "Use dynamic programming or binary search.",
                    SampleInput = "6\n3 1 4 2 5 6",
                    SampleOutput = "4",
                    TimeLimitMs = 1000,
                    MemoryLimitMB = 256,
                    DifficultyRating = 1600,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsEnglishTranslated = true
                },
                new Problem
                {
                    Slug = "can-bang-thap",
                    TitleVi = "Cân Bằng Tháp",
                    StatementVi = "Cho n khối với trọng lượng w1, w2, ..., wn. Có thể chia thành hai nhóm có tổng trọng lượng bằng nhau không?",
                    InputDescriptionVi = "Dòng đầu tiên chứa n. Dòng thứ hai chứa n số nguyên dương.",
                    OutputDescriptionVi = "In ra \"YES\" nếu có thể, \"NO\" nếu không.",
                    ConstraintsVi = "1 ≤ n ≤ 20, 1 ≤ w[i] ≤ 10^9",
                    ExplanationVi = "Sử dụng quy hoạch động hoặc bitmask.",
                    TitleEn = "Tower Balance",
                    StatementEn = "Given n blocks with weights w1, w2, ..., wn. Can they be divided into two groups with equal total weight?",
                    InputDescriptionEn = "The first line contains n. The second line contains n positive integers.",
                    OutputDescriptionEn = "Output \"YES\" if possible, \"NO\" otherwise.",
                    ConstraintsEn = "1 ≤ n ≤ 20, 1 ≤ w[i] ≤ 10^9",
                    ExplanationEn = "Use dynamic programming or bitmask.",
                    SampleInput = "3\n1 2 3",
                    SampleOutput = "YES",
                    TimeLimitMs = 1000,
                    MemoryLimitMB = 256,
                    DifficultyRating = 1800,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsEnglishTranslated = true
                },
                new Problem
                {
                    Slug = "duong-di-ngan-nhat",
                    TitleVi = "Đường Đi Ngắn Nhất",
                    StatementVi = "Cho đồ thị có trọng lượng với n đỉnh và m cạnh. Tìm đường đi ngắn nhất từ đỉnh 1 đến đỉnh n.",
                    InputDescriptionVi = "Dòng đầu tiên chứa n và m. m dòng tiếp theo chứa u, v, w (cạnh từ u đến v với trọng lượng w).",
                    OutputDescriptionVi = "In ra độ dài đường đi ngắn nhất hoặc -1 nếu không có đường đi.",
                    ConstraintsVi = "1 ≤ n ≤ 10^5, 1 ≤ m ≤ 2*10^5, 1 ≤ w ≤ 10^9",
                    ExplanationVi = "Sử dụng thuật toán Dijkstra.",
                    TitleEn = "Shortest Path",
                    StatementEn = "Given a weighted graph with n vertices and m edges. Find the shortest path from vertex 1 to vertex n.",
                    InputDescriptionEn = "The first line contains n and m. Next m lines contain u, v, w (edge from u to v with weight w).",
                    OutputDescriptionEn = "Output the shortest path length or -1 if no path exists.",
                    ConstraintsEn = "1 ≤ n ≤ 10^5, 1 ≤ m ≤ 2*10^5, 1 ≤ w ≤ 10^9",
                    ExplanationEn = "Use Dijkstra algorithm.",
                    SampleInput = "3 3\n1 2 2\n2 3 3\n1 3 6",
                    SampleOutput = "5",
                    TimeLimitMs = 2000,
                    MemoryLimitMB = 512,
                    DifficultyRating = 2000,
                    IsPublic = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUser.Id,
                    IsEnglishTranslated = true
                }
            };

            await context.Problems.AddRangeAsync(problems);
            await context.SaveChangesAsync();
            Console.WriteLine("Created 10 problems");

            // Create ContestProblems
            // Contest 1: Vòng Loại (Problems 1-4)
            var contestProblems = new List<ContestProblem>
            {
                new ContestProblem { ContestId = contest1.Id, ProblemId = problems[0].Id, ProblemLetter = "A", Order = 1, Points = 100 },
                new ContestProblem { ContestId = contest1.Id, ProblemId = problems[1].Id, ProblemLetter = "B", Order = 2, Points = 100 },
                new ContestProblem { ContestId = contest1.Id, ProblemId = problems[2].Id, ProblemLetter = "C", Order = 3, Points = 150 },
                new ContestProblem { ContestId = contest1.Id, ProblemId = problems[3].Id, ProblemLetter = "D", Order = 4, Points = 150 },
                // Contest 2: Vòng Chung Kết (Problems 5-8)
                new ContestProblem { ContestId = contest2.Id, ProblemId = problems[4].Id, ProblemLetter = "A", Order = 1, Points = 100 },
                new ContestProblem { ContestId = contest2.Id, ProblemId = problems[5].Id, ProblemLetter = "B", Order = 2, Points = 150 },
                new ContestProblem { ContestId = contest2.Id, ProblemId = problems[6].Id, ProblemLetter = "C", Order = 3, Points = 200 },
                new ContestProblem { ContestId = contest2.Id, ProblemId = problems[7].Id, ProblemLetter = "D", Order = 4, Points = 250 },
                // Contest 3: Vòng Thử Thách (Problems 9-10)
                new ContestProblem { ContestId = contest3.Id, ProblemId = problems[8].Id, ProblemLetter = "A", Order = 1, Points = 300 },
                new ContestProblem { ContestId = contest3.Id, ProblemId = problems[9].Id, ProblemLetter = "B", Order = 2, Points = 400 }
            };

            await context.ContestProblems.AddRangeAsync(contestProblems);
            await context.SaveChangesAsync();
            Console.WriteLine("Created contest-problem relationships");

            // Create TestCases for each problem
            var testCases = new List<TestCase>();

            // Problem 1: Tổng Hai Số
            testCases.AddRange(new[]
            {
                new TestCase { ProblemId = problems[0].Id, Input = "5\n7", ExpectedOutput = "12", Points = 10, IsSample = true, Order = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[0].Id, Input = "-10\n20", ExpectedOutput = "10", Points = 10, IsSample = false, Order = 2, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[0].Id, Input = "0\n0", ExpectedOutput = "0", Points = 10, IsSample = false, Order = 3, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[0].Id, Input = "1000000000\n-1000000000", ExpectedOutput = "0", Points = 10, IsSample = false, Order = 4, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[0].Id, Input = "123456789\n987654321", ExpectedOutput = "1111111110", Points = 10, IsSample = false, Order = 5, CreatedAt = DateTime.UtcNow }
            });

            // Problem 2: Ước Số Chung Lớn Nhất
            testCases.AddRange(new[]
            {
                new TestCase { ProblemId = problems[1].Id, Input = "12\n18", ExpectedOutput = "6", Points = 10, IsSample = true, Order = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[1].Id, Input = "7\n13", ExpectedOutput = "1", Points = 10, IsSample = false, Order = 2, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[1].Id, Input = "100\n50", ExpectedOutput = "50", Points = 10, IsSample = false, Order = 3, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[1].Id, Input = "17\n17", ExpectedOutput = "17", Points = 10, IsSample = false, Order = 4, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[1].Id, Input = "1\n1000000000", ExpectedOutput = "1", Points = 10, IsSample = false, Order = 5, CreatedAt = DateTime.UtcNow }
            });

            // Problem 3: Dãy Fibonacci
            testCases.AddRange(new[]
            {
                new TestCase { ProblemId = problems[2].Id, Input = "10", ExpectedOutput = "55", Points = 10, IsSample = true, Order = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[2].Id, Input = "1", ExpectedOutput = "1", Points = 10, IsSample = false, Order = 2, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[2].Id, Input = "2", ExpectedOutput = "1", Points = 10, IsSample = false, Order = 3, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[2].Id, Input = "20", ExpectedOutput = "6765", Points = 10, IsSample = false, Order = 4, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[2].Id, Input = "50", ExpectedOutput = "12586269025", Points = 10, IsSample = false, Order = 5, CreatedAt = DateTime.UtcNow }
            });

            // Problem 4: Sắp Xếp Mảng
            testCases.AddRange(new[]
            {
                new TestCase { ProblemId = problems[3].Id, Input = "5\n3 1 4 2 5", ExpectedOutput = "1 2 3 4 5", Points = 10, IsSample = true, Order = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[3].Id, Input = "3\n5 4 3", ExpectedOutput = "3 4 5", Points = 10, IsSample = false, Order = 2, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[3].Id, Input = "1\n42", ExpectedOutput = "42", Points = 10, IsSample = false, Order = 3, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[3].Id, Input = "6\n-1 -2 -3 0 1 2", ExpectedOutput = "-3 -2 -1 0 1 2", Points = 10, IsSample = false, Order = 4, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[3].Id, Input = "4\n1 1 1 1", ExpectedOutput = "1 1 1 1", Points = 10, IsSample = false, Order = 5, CreatedAt = DateTime.UtcNow }
            });

            // Problem 5: Tìm Kiếm Nhị Phân
            testCases.AddRange(new[]
            {
                new TestCase { ProblemId = problems[4].Id, Input = "5 3\n1 2 3 4 5", ExpectedOutput = "3", Points = 10, IsSample = true, Order = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[4].Id, Input = "5 6\n1 2 3 4 5", ExpectedOutput = "-1", Points = 10, IsSample = false, Order = 2, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[4].Id, Input = "1 1\n1", ExpectedOutput = "1", Points = 10, IsSample = false, Order = 3, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[4].Id, Input = "5 1\n1 2 3 4 5", ExpectedOutput = "1", Points = 10, IsSample = false, Order = 4, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[4].Id, Input = "5 5\n1 2 3 4 5", ExpectedOutput = "5", Points = 10, IsSample = false, Order = 5, CreatedAt = DateTime.UtcNow }
            });

            // Problem 6: Số Nguyên Tố
            testCases.AddRange(new[]
            {
                new TestCase { ProblemId = problems[5].Id, Input = "17", ExpectedOutput = "YES", Points = 10, IsSample = true, Order = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[5].Id, Input = "1", ExpectedOutput = "NO", Points = 10, IsSample = false, Order = 2, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[5].Id, Input = "2", ExpectedOutput = "YES", Points = 10, IsSample = false, Order = 3, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[5].Id, Input = "100", ExpectedOutput = "NO", Points = 10, IsSample = false, Order = 4, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[5].Id, Input = "997", ExpectedOutput = "YES", Points = 10, IsSample = false, Order = 5, CreatedAt = DateTime.UtcNow }
            });

            // Problem 7: Cộng Ma Trận
            testCases.AddRange(new[]
            {
                new TestCase { ProblemId = problems[6].Id, Input = "2 2\n1 2\n3 4\n5 6\n7 8", ExpectedOutput = "6 8\n10 12", Points = 10, IsSample = true, Order = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[6].Id, Input = "1 1\n5\n10", ExpectedOutput = "15", Points = 10, IsSample = false, Order = 2, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[6].Id, Input = "2 3\n1 2 3\n4 5 6\n7 8 9\n10 11 12", ExpectedOutput = "8 10 12\n14 16 18", Points = 10, IsSample = false, Order = 3, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[6].Id, Input = "3 2\n1 2\n3 4\n5 6\n7 8\n9 10\n11 12", ExpectedOutput = "8 10\n12 14\n16 18", Points = 10, IsSample = false, Order = 4, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[6].Id, Input = "2 2\n0 0\n0 0\n0 0\n0 0", ExpectedOutput = "0 0\n0 0", Points = 10, IsSample = false, Order = 5, CreatedAt = DateTime.UtcNow }
            });

            // Problem 8: Dãy Con Tăng Dài Nhất
            testCases.AddRange(new[]
            {
                new TestCase { ProblemId = problems[7].Id, Input = "6\n3 1 4 2 5 6", ExpectedOutput = "4", Points = 10, IsSample = true, Order = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[7].Id, Input = "5\n5 4 3 2 1", ExpectedOutput = "1", Points = 10, IsSample = false, Order = 2, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[7].Id, Input = "5\n1 2 3 4 5", ExpectedOutput = "5", Points = 10, IsSample = false, Order = 3, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[7].Id, Input = "4\n2 2 2 2", ExpectedOutput = "1", Points = 10, IsSample = false, Order = 4, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[7].Id, Input = "7\n1 3 2 4 3 5 4", ExpectedOutput = "4", Points = 10, IsSample = false, Order = 5, CreatedAt = DateTime.UtcNow }
            });

            // Problem 9: Cân Bằng Tháp
            testCases.AddRange(new[]
            {
                new TestCase { ProblemId = problems[8].Id, Input = "3\n1 2 3", ExpectedOutput = "YES", Points = 10, IsSample = true, Order = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[8].Id, Input = "2\n1 2", ExpectedOutput = "NO", Points = 10, IsSample = false, Order = 2, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[8].Id, Input = "4\n2 2 2 2", ExpectedOutput = "YES", Points = 10, IsSample = false, Order = 3, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[8].Id, Input = "1\n5", ExpectedOutput = "NO", Points = 10, IsSample = false, Order = 4, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[8].Id, Input = "5\n1 1 1 1 2", ExpectedOutput = "YES", Points = 10, IsSample = false, Order = 5, CreatedAt = DateTime.UtcNow }
            });

            // Problem 10: Đường Đi Ngắn Nhất
            testCases.AddRange(new[]
            {
                new TestCase { ProblemId = problems[9].Id, Input = "3 3\n1 2 2\n2 3 3\n1 3 6", ExpectedOutput = "5", Points = 10, IsSample = true, Order = 1, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[9].Id, Input = "2 1\n1 2 5", ExpectedOutput = "5", Points = 10, IsSample = false, Order = 2, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[9].Id, Input = "3 2\n1 2 1\n2 3 2", ExpectedOutput = "3", Points = 10, IsSample = false, Order = 3, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[9].Id, Input = "4 3\n1 2 1\n2 3 1\n3 4 1", ExpectedOutput = "3", Points = 10, IsSample = false, Order = 4, CreatedAt = DateTime.UtcNow },
                new TestCase { ProblemId = problems[9].Id, Input = "3 1\n1 3 10", ExpectedOutput = "10", Points = 10, IsSample = false, Order = 5, CreatedAt = DateTime.UtcNow }
            });

            await context.TestCases.AddRangeAsync(testCases);
            await context.SaveChangesAsync();
            Console.WriteLine("Created 50 test cases (5 per problem)");

            Console.WriteLine("Custom data seeding completed successfully!");
        }
    }
}