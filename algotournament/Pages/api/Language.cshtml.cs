using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace algotournament.Pages.api
{
    public class LanguageModel : PageModel
    {
        public string? Language { get; set; }

        public void OnGet(string? lang = null)
        {
            // Get language from query parameter or cookie
            Language = lang ?? Request.Cookies["preferredLanguage"] ?? "vi";
            
            // Set cookie if language is provided
            if (lang != null)
            {
                Response.Cookies.Append("preferredLanguage", lang, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });
            }
        }

        public IActionResult OnPost(string lang)
        {
            if (string.IsNullOrEmpty(lang) || (lang != "vi" && lang != "en"))
            {
                return new JsonResult(new { success = false, error = "Invalid language" });
            }

            // Set cookie
            Response.Cookies.Append("preferredLanguage", lang, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });

            return new JsonResult(new { success = true, currentLanguage = lang });
        }
    }
}
