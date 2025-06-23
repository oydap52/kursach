using FurnitureProduction.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace FurnitureProduction.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public RegisterModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Электронная почта обязательна.")]
            [EmailAddress(ErrorMessage = "Введите корректный email.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "ФИО обязательно.")]
            [StringLength(200, ErrorMessage = "ФИО не должно превышать 200 символов.")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Паспортные данные обязательны.")]
            [StringLength(20, ErrorMessage = "Паспортные данные не должны превышать 20 символов.")]
            public string PassportData { get; set; }

            [Required(ErrorMessage = "Пароль обязателен.")]
            [StringLength(100, ErrorMessage = "Пароль должен содержать от {2} до {1} символов.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page(); // Останавливаемся и показываем ошибки валидации
            }

            try
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FullName = Input.FullName,
                    PassportData = Input.PassportData
                };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("User"))
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    await _userManager.AddToRoleAsync(user, "User");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(ReturnUrl);
                }

                // Привязываем ошибки пароля к полю Password
                foreach (var error in result.Errors)
                {
                    if (error.Code == "PasswordRequiresDigit" ||
                        error.Code == "PasswordRequiresLower" ||
                        error.Code == "PasswordRequiresUpper" ||
                        error.Code == "PasswordRequiresNonAlphanumeric" ||
                        error.Code == "PasswordTooShort")
                    {
                        ModelState.AddModelError("Input.Password", error.Description);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error.Description); // Остальные ошибки в общем блоке
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Произошла ошибка при регистрации: {ex.Message}");
                Console.WriteLine($"Registration Error: {ex}");
            }

            // Сохраняем данные формы при ошибке
            return Page();
        }
    }
}