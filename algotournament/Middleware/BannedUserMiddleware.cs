using algotournament.Data;
using algotournament.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace algotournament.Middleware
{
    public class BannedUserMiddleware
    {
        private readonly RequestDelegate _next;

        public BannedUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, SignInManager<ApplicationUser> signInManager, ApplicationDbContext dbContext)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = signInManager.UserManager.GetUserId(context.User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var isBanned = await dbContext.Users
                        .AsNoTracking()
                        .Where(u => u.Id == userId)
                        .Select(u => (bool?)u.IsBanned)
                        .FirstOrDefaultAsync();

                    if (isBanned == true)
                    {
                        await signInManager.SignOutAsync();
                        var path = context.Request.Path.Value ?? "";
                        if (!path.StartsWith("/Account/Login", StringComparison.OrdinalIgnoreCase) &&
                            !path.StartsWith("/Account/Logout", StringComparison.OrdinalIgnoreCase) &&
                            !path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) &&
                            !path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) &&
                            !path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.Redirect("/Account/Login?banned=1");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
