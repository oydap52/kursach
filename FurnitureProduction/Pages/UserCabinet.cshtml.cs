using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FurnitureProduction.Data;
using Microsoft.EntityFrameworkCore;
using FurnitureProduction.Models;

namespace FurnitureProduction.Pages
{
    public class UserCabinetModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserCabinetModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Order> Orders { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Фильтрация по ClientName (почта), которая совпадает с UserName
            Orders = await _context.Orders
                .Include(o => o.OrderMaterials)
                .ThenInclude(om => om.Material)
                .Where(o => o.ClientName == user.UserName) // Используем UserName как почту
                .OrderBy(o => o.Id)
                .ToListAsync();

            return Page();
        }
    }
}