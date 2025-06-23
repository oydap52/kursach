using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FurnitureProduction.Data;
using FurnitureProduction.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurnitureProduction.Pages.Warehouse
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<WarehouseViewModel> Materials { get; set; } = new List<WarehouseViewModel>();

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; } = "name_asc";

        public async Task OnGetAsync()
        {
            var materials = _context.Materials
                .Select(m => new WarehouseViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Type = m.Type,
                    Category = m.Category,
                    Quantity = m.Quantity,
                    UnitPrice = m.UnitPrice,
                    Supplier = m.Supplier
                })
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
            {
                materials = materials.Where(m => m.Name.Contains(SearchString) || m.Type.Contains(SearchString) || m.Supplier.Contains(SearchString) || m.Category.Contains(SearchString));
            }

            materials = SortOrder switch
            {
                "name_desc" => materials.OrderByDescending(m => m.Name),
                "type_asc" => materials.OrderBy(m => m.Type),
                "type_desc" => materials.OrderByDescending(m => m.Type),
                "category_asc" => materials.OrderBy(m => m.Category),
                "category_desc" => materials.OrderByDescending(m => m.Category),
                "quantity_asc" => materials.OrderBy(m => m.Quantity),
                "quantity_desc" => materials.OrderByDescending(m => m.Quantity),
                "price_asc" => materials.OrderBy(m => m.UnitPrice),
                "price_desc" => materials.OrderByDescending(m => m.UnitPrice),
                _ => materials.OrderBy(m => m.Name)
            };

            Materials = await materials.ToListAsync();
        }
    }

    public class WarehouseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Supplier { get; set; }
    }
}