using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace algotournament.Pages.Admin.Problems
{
    [Authorize(Policy = "RequireAdminRole")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(ApplicationDbContext context, ILogger<DeleteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Problem Problem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Problem = await _context.Problems
                .Include(p => p.TestCases)
                .Include(p => p.Submissions)
                .Include(p => p.ContestProblems)
                // .Include(p => p.DuelRooms) // Commented out - requires database migration
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Problem == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var problem = await _context.Problems
                .Include(p => p.TestCases)
                .Include(p => p.Submissions)
                .Include(p => p.ContestProblems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (problem == null)
            {
                return NotFound();
            }

            // Get duel rooms count via query
            var duelRoomsCount = await _context.DuelRooms.CountAsync(dr => dr.ProblemId == problem.Id);

            if (problem != null)
            {
                _logger.LogInformation("Starting delete for Problem ID: {ProblemId} with {Submissions} submissions, {TestCases} test cases, {DuelRooms} duel rooms", 
                    problem.Id, problem.Submissions.Count, problem.TestCases.Count, duelRoomsCount);

                // Remove submissions first (foreign key constraint)
                if (problem.Submissions != null && problem.Submissions.Any())
                {
                    _logger.LogInformation("Removing {Count} submissions", problem.Submissions.Count);
                    _context.Submissions.RemoveRange(problem.Submissions);
                }

                // Remove test cases
                if (problem.TestCases != null && problem.TestCases.Any())
                {
                    _logger.LogInformation("Removing {Count} test cases", problem.TestCases.Count);
                    _context.TestCases.RemoveRange(problem.TestCases);
                }

                // Remove contest-problem relationships
                if (problem.ContestProblems != null && problem.ContestProblems.Any())
                {
                    _logger.LogInformation("Removing {Count} contest-problem relationships", problem.ContestProblems.Count);
                    _context.ContestProblems.RemoveRange(problem.ContestProblems);
                }

                // Remove duel rooms and their matches
                var duelRooms = await _context.DuelRooms.Where(dr => dr.ProblemId == problem.Id).ToListAsync();
                if (duelRooms.Any())
                {
                    _logger.LogInformation("Removing {Count} duel rooms", duelRooms.Count);
                    
                    // Get duel room IDs
                    var duelRoomIds = duelRooms.Select(dr => dr.Id).ToList();
                    
                    // Query duel matches by room IDs
                    var duelMatches = await _context.DuelMatches
                        .Where(dm => duelRoomIds.Contains(dm.DuelRoomId))
                        .ToListAsync();
                    
                    if (duelMatches.Any())
                    {
                        // Remove submissions linked to duel matches
                        var duelMatchIds = duelMatches.Select(dm => dm.Id).ToList();
                        var duelSubmissions = await _context.Submissions
                            .Where(s => s.DuelMatchId.HasValue && duelMatchIds.Contains(s.DuelMatchId.Value))
                            .ToListAsync();
                        if (duelSubmissions.Any())
                        {
                            _logger.LogInformation("Removing {Count} submissions for duel matches", duelSubmissions.Count);
                            _context.Submissions.RemoveRange(duelSubmissions);
                        }
                        
                        // Remove duel matches
                        _logger.LogInformation("Removing {Count} duel matches", duelMatches.Count);
                        _context.DuelMatches.RemoveRange(duelMatches);
                    }
                    
                    _context.DuelRooms.RemoveRange(duelRooms);
                }

                // Then remove the problem
                _logger.LogInformation("Removing problem");
                _context.Problems.Remove(problem);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Problem deleted successfully");
            }

            return RedirectToPage("./Index");
        }
    }
}
