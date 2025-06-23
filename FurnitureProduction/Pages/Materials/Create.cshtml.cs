using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FurnitureProduction.Data;
using FurnitureProduction.Models;
using Microsoft.AspNetCore.Authorization;

namespace FurnitureProduction.Pages.Materials
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Material Material { get; set; } = new Material();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingMaterial = await _context.Materials.FirstOrDefaultAsync(m => m.Name == Material.Name && m.Type == Material.Type);
            if (existingMaterial != null)
            {
                ModelState.AddModelError("", "Материал с таким названием и типом уже существует.");
                return Page();
            }

            try
            {
                _context.Materials.Add(Material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Материал {Material.Name} успешно добавлен.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при добавлении материала: {ex.Message}";
                return Page();
            }

            return RedirectToPage("/Warehouse/Index");
        }
    }
}