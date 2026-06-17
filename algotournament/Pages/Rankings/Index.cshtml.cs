using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Rankings
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<UserRanking> Users { get; set; } = new List<UserRanking>();

        public class UserRanking
        {
            public string Handle { get; set; } = string.Empty;
            public int Rating { get; set; }
            public int ProblemsSolved { get; set; }
            public int ContestsParticipated { get; set; }
            public int Rank { get; set; }
        }

        public async Task OnGetAsync()
        {
            var users = await _context.Users
                .Where(u => !u.IsBanned)
                .OrderByDescending(u => u.Rating)
                .ThenByDescending(u => u.ProblemsSolved)
                .Take(100)
                .ToListAsync();

            Users = users.Select((u, index) => new UserRanking
            {
                Handle = u.Handle,
                Rating = u.Rating,
                ProblemsSolved = u.ProblemsSolved,
                ContestsParticipated = u.ContestsParticipated,
                Rank = index + 1
            }).ToList();
        }
    }
}
