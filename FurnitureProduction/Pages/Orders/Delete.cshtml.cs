using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurnitureProduction.Data;
using System.Threading.Tasks;
using FurnitureProduction.Models;

namespace FurnitureProduction.Pages.Orders
{
    [Authorize]
    public class DeleteModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Order Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Order = await _context.Orders
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Order == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Order = await _context.Orders.FindAsync(id);

            if (Order != null)
            {
                try
                {
                    // Удаляем все связанные записи из OrderMaterials
                    var orderMaterials = await _context.OrderMaterials
                        .Where(om => om.OrderId == id)
                        .ToListAsync();

                    if (orderMaterials.Any())
                    {
                        // Возвращаем количество материалов перед удалением
                        foreach (var orderMaterial in orderMaterials)
                        {
                            var material = await _context.Materials.FindAsync(orderMaterial.MaterialId);
                            if (material != null)
                            {
                                material.Quantity += orderMaterial.Quantity;
                            }
                        }
                        _context.OrderMaterials.RemoveRange(orderMaterials);
                        await _context.SaveChangesAsync();
                    }

                    // Удаляем сам заказ
                    _context.Orders.Remove(Order);
                    await _context.SaveChangesAsync();

                    return RedirectToPage("./Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ошибка при удалении заказа: {ex.Message}");
                    return Page();
                }
            }

            return RedirectToPage("./Index");
        }
    }
}