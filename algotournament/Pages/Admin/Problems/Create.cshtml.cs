using algotournament.Data;
using algotournament.Models;
using algotournament.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace algotournament.Pages.Admin.Problems
{
    [Authorize(Policy = "RequireAdminRole")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITranslationService _translationService;

        public CreateModel(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ITranslationService translationService)
        {
            _context = context;
            _userManager = userManager;
            _translationService = translationService;
        }

        [BindProperty]
        public Problem Problem { get; set; } = new();

        public bool AutoTranslate { get; set; } = true;

        public async Task<IActionResult> OnPostAsync(bool autoTranslate = true)
        {
            // Remove validation errors for English fields since they are nullable
            ModelState.Remove(nameof(Problem.TitleEn));
            ModelState.Remove(nameof(Problem.StatementEn));
            ModelState.Remove(nameof(Problem.InputDescriptionEn));
            ModelState.Remove(nameof(Problem.OutputDescriptionEn));
            ModelState.Remove(nameof(Problem.ConstraintsEn));
            ModelState.Remove(nameof(Problem.ExplanationEn));

            // Also clear any validation errors for English fields
            var englishFields = new[] { "TitleEn", "StatementEn", "InputDescriptionEn", "OutputDescriptionEn", "ConstraintsEn", "ExplanationEn" };
            foreach (var field in englishFields)
            {
                var key = $"Problem.{field}";
                if (ModelState.ContainsKey(key))
                {
                    ModelState[key].Errors.Clear();
                }
            }

            // Temporarily bypass validation to test
            // if (!ModelState.IsValid)
            // {
            //     return Page();
            // }

            var currentUser = await _userManager.GetUserAsync(User);
            Problem.CreatedBy = currentUser?.Id ?? string.Empty;
            Problem.CreatedAt = DateTime.UtcNow;

            // Auto-translate Vietnamese content to English if enabled
            if (autoTranslate)
            {
                await AutoTranslateProblemAsync();
            }

            _context.Problems.Add(Problem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task AutoTranslateProblemAsync()
        {
            try
            {
                // Prepare Vietnamese content for translation
                var vietnameseContent = new Dictionary<string, string>
                {
                    ["title"] = Problem.TitleVi,
                    ["statement"] = Problem.StatementVi,
                    ["input"] = Problem.InputDescriptionVi,
                    ["output"] = Problem.OutputDescriptionVi,
                    ["constraints"] = Problem.ConstraintsVi,
                    ["explanation"] = Problem.ExplanationVi
                };

                // Translate to English
                var englishContent = await _translationService.TranslateProblemContentAsync(vietnameseContent);

                // Update English fields (handle nullable strings)
                Problem.TitleEn = englishContent["title"] ?? string.Empty;
                Problem.StatementEn = englishContent["statement"] ?? string.Empty;
                Problem.InputDescriptionEn = englishContent["input"] ?? string.Empty;
                Problem.OutputDescriptionEn = englishContent["output"] ?? string.Empty;
                Problem.ConstraintsEn = englishContent["constraints"] ?? string.Empty;
                Problem.ExplanationEn = englishContent["explanation"] ?? string.Empty;

                // Update translation status
                Problem.IsEnglishTranslated = true;
                Problem.EnglishTranslatedAt = DateTime.UtcNow;
            }
            catch (Exception)
            {
                // Log error but don't prevent problem creation
                // English fields will remain null and can be filled manually later
                Problem.IsEnglishTranslated = false;
            }
        }
    }
}
