using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using algotournament.Models;

namespace algotournament.Pages.Account
{
    public class EditProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EditProfileModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50, MinimumLength = 3)]
            [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Handle can only contain letters, numbers, underscores and hyphens")]
            public string Handle { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; }

            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 6)]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Input = new InputModel
            {
                Handle = user.Handle,
                Email = user.Email
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Check if handle is already taken by another user
            var existingUser = await _userManager.FindByNameAsync(Input.Handle);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("Input.Handle", "This handle is already taken.");
                return Page();
            }

            user.Handle = Input.Handle;
            user.Email = Input.Email;

            // Handle password change if provided
            if (!string.IsNullOrEmpty(Input.NewPassword))
            {
                if (string.IsNullOrEmpty(Input.CurrentPassword))
                {
                    ModelState.AddModelError("Input.CurrentPassword", "Current password is required to change password.");
                    return Page();
                }

                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
                if (!passwordChangeResult.Succeeded)
                {
                    foreach (var error in passwordChangeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage("./Dashboard");
        }
    }
}
