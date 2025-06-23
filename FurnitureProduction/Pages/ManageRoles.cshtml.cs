using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurnitureProduction.Pages
{
    [Authorize(Roles = "Admin")]
    public class ManageRolesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageRolesModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserRoleViewModel> Users { get; set; } = new List<UserRoleViewModel>(); // Инициализация по умолчанию

        public class UserRoleViewModel
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public string CurrentRole { get; set; }
            public string NewRole { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            if (users == null)
            {
                Users = new List<UserRoleViewModel>();
                return Page();
            }

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault() ?? "User";

                Users.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    CurrentRole = currentRole,
                    NewRole = currentRole
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string userId, string newRole)
        {

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
            {
                ModelState.AddModelError("", "Ошибка: данные пользователя или роли не переданы.");
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "Пользователь не найден.");
                return Page();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRoleAsync(user, currentRoles.First());
            }
            await _userManager.AddToRoleAsync(user, newRole);

            await OnGetAsync();
            TempData["success"] = true;
            return RedirectToPage("/ManageRoles");
        }
    }
}