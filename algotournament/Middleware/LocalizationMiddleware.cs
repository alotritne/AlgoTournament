using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace algotournament.Middleware
{
    public class LocalizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _supportedCultures = new[] { "vi", "en" };

        public LocalizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if URL has language prefix
            var path = context.Request.Path.Value ?? "";
            
            // Extract language from URL if present
            string? cultureFromUrl = null;
            if (path.Length > 1)
            {
                var potentialCulture = path.Substring(1).Split('/')[0];
                if (_supportedCultures.Contains(potentialCulture))
                {
                    cultureFromUrl = potentialCulture;
                }
            }

            // Determine culture
            string culture;
            if (cultureFromUrl != null)
            {
                culture = cultureFromUrl;
            }
            else
            {
                // Check cookie
                var cultureFromCookie = context.Request.Cookies["preferredLanguage"];
                if (cultureFromCookie != null && _supportedCultures.Contains(cultureFromCookie))
                {
                    culture = cultureFromCookie;
                }
                else
                {
                    // Check Accept-Language header
                    var cultureFromHeader = context.Request.Headers["Accept-Language"].ToString();
                    culture = GetCultureFromHeader(cultureFromHeader);
                }
            }

            // Set culture
            var cultureInfo = new CultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            // Store in context for use in pages
            context.Items["CurrentCulture"] = culture;

            await _next(context);
        }

        private string GetCultureFromHeader(string acceptLanguage)
        {
            if (string.IsNullOrEmpty(acceptLanguage))
                return "vi"; // Default to Vietnamese

            // Parse Accept-Language header
            var languages = acceptLanguage.Split(',')
                .Select(lang =>
                {
                    var parts = lang.Trim().Split(';');
                    var culture = parts[0].Trim();
                    var quality = 1.0;

                    if (parts.Length > 1 && parts[1].StartsWith("q="))
                    {
                        double.TryParse(parts[1].Substring(2), out quality);
                    }

                    return new { Culture = culture, Quality = quality };
                })
                .OrderByDescending(x => x.Quality)
                .ToList();

            // Find first supported culture
            foreach (var lang in languages)
            {
                var cultureCode = lang.Culture.Split('-')[0]; // Get primary language
                if (_supportedCultures.Contains(cultureCode))
                {
                    return cultureCode;
                }
            }

            return "vi"; // Default to Vietnamese
        }
    }
}
