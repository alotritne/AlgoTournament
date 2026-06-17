using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Duels
{
    [Authorize(Policy = "RequireAdminRole")]
    public class ParticipantsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ParticipantsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<DuelParticipant> Participants { get; set; } = new();

        public async Task OnGetAsync()
        {
            Participants = await _context.DuelParticipants
                .Include(p => p.User)
                .Include(p => p.Room)
                .OrderByDescending(p => p.JoinedAt)
                .ToListAsync();
        }
    }
}
