using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Pages.Admin.Duels
{
    [Authorize(Policy = "RequireAdminRole")]
    public class RoomsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RoomsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<DuelRoom> Rooms { get; set; } = new();

        public async Task OnGetAsync()
        {
            Rooms = await _context.DuelRooms
                .Include(r => r.Problem)
                .Include(r => r.HostUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
