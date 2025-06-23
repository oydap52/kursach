using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FurnitureProduction.Data;
using FurnitureProduction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurnitureProduction.Pages.Orders
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Order Order { get; set; }

        public SelectList FurnitureTypes { get; set; }
        public List<MaterialViewModel> MaterialViewModels { get; set; }
        public List<FurnitureTypeConfig> FurnitureTypeConfigs { get; set; }
        public SelectList Employees { get; set; }
        public Dictionary<string, List<object>> FurnitureVariants { get; set; }
        [BindProperty]
        public List<string> SelectedMaterialIds { get; set; } = new List<string>();
        public Dictionary<string, List<string>> MaterialCategories { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _context.Orders
                .Include(o => o.OrderMaterials)
                .ThenInclude(om => om.Material)
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Order == null)
            {
                return NotFound();
            }

            var furnitureTypesList = await _context.FurnitureTypeConfigs.Select(c => c.Type).Distinct().ToListAsync();
            FurnitureTypes = new SelectList(furnitureTypesList, null, null, Order.FurnitureType);

            MaterialViewModels = await _context.Materials.Select(m => new MaterialViewModel
            {
                Id = m.Id,
                Name = m.Name,
                UnitPrice = m.UnitPrice,
                Category = m.Category ?? "Другие"
            }).ToListAsync();
            FurnitureTypeConfigs = await _context.FurnitureTypeConfigs.ToListAsync();
            FurnitureVariants = FurnitureTypeConfigs
                .GroupBy(c => c.Type.ToLower(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => new { variant = c.Variant, baseCost = c.BaseCost, baseWeight = c.BaseWeight }).Cast<object>().ToList(),
                    StringComparer.OrdinalIgnoreCase
                );

            var employeeList = await _context.Employees.ToListAsync();
            Employees = new SelectList(employeeList, "Id", "FullName");

            SelectedMaterialIds = Order.OrderMaterials?.Select(om => om.MaterialId.ToString())?.ToList() ?? new List<string>();

            var templates = await _context.FurnitureMaterialTemplates.ToListAsync();
            MaterialCategories = templates
                .GroupBy(t => t.FurnitureType.ToLower(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => t.Category).Distinct().ToList(),
                    StringComparer.OrdinalIgnoreCase
                ) ?? new Dictionary<string, List<string>>();

            var allTypes = FurnitureTypeConfigs.Select(c => c.Type.ToLower()).Distinct();
            foreach (var type in allTypes)
            {
                if (!MaterialCategories.ContainsKey(type))
                {
                    MaterialCategories[type] = new List<string> { "Древесина", "Фурнитура", "Отделка" };
                    if (type.Contains("зеркало".ToLower())) MaterialCategories[type].Add("Стекло");
                    if (type.Contains("диван".ToLower()) || type.Contains("кровать".ToLower())) MaterialCategories[type].AddRange(new[] { "Ткань", "Наполнитель" });
                    if (type.Contains("шкаф".ToLower()) || type.Contains("стеллаж".ToLower())) MaterialCategories[type].Add("Композиты");
                    if (type.Contains("стол".ToLower()) || type.Contains("тумба".ToLower())) MaterialCategories[type].Add("Древесина");
                    if (type.Contains("кресло".ToLower())) MaterialCategories[type].AddRange(new[] { "Ткань", "Наполнитель" });
                    if (type.Contains("вешалка".ToLower())) MaterialCategories[type].Add("Металл");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                FurnitureTypes = new SelectList(await _context.FurnitureTypeConfigs.Select(c => c.Type).Distinct().ToListAsync());
                MaterialViewModels = await _context.Materials.Select(m => new MaterialViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    UnitPrice = m.UnitPrice,
                    Category = m.Category ?? "Другие"
                }).ToListAsync();
                FurnitureTypeConfigs = await _context.FurnitureTypeConfigs.ToListAsync();
                FurnitureVariants = FurnitureTypeConfigs
                    .GroupBy(c => c.Type.ToLower(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(c => new { variant = c.Variant, baseCost = c.BaseCost, baseWeight = c.BaseWeight }).Cast<object>().ToList(),
                        StringComparer.OrdinalIgnoreCase
                    );
                var employeeList = await _context.Employees.ToListAsync();
                Employees = new SelectList(employeeList, "Id", "FullName");

                var templates = await _context.FurnitureMaterialTemplates.ToListAsync();
                MaterialCategories = templates
                    .GroupBy(t => t.FurnitureType.ToLower(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(t => t.Category).Distinct().ToList(),
                        StringComparer.OrdinalIgnoreCase
                    ) ?? new Dictionary<string, List<string>>();
                var allTypes = FurnitureTypeConfigs.Select(c => c.Type.ToLower()).Distinct();
                foreach (var type in allTypes)
                {
                    if (!MaterialCategories.ContainsKey(type))
                    {
                        MaterialCategories[type] = new List<string> { "Древесина", "Фурнитура", "Отделка" };
                        if (type.Contains("зеркало".ToLower())) MaterialCategories[type].Add("Стекло");
                        if (type.Contains("диван".ToLower()) || type.Contains("кровать".ToLower())) MaterialCategories[type].AddRange(new[] { "Ткань", "Наполнитель" });
                        if (type.Contains("шкаф".ToLower()) || type.Contains("стеллаж".ToLower())) MaterialCategories[type].Add("Композиты");
                        if (type.Contains("стол".ToLower()) || type.Contains("тумба".ToLower())) MaterialCategories[type].Add("Древесина");
                        if (type.Contains("кресло".ToLower())) MaterialCategories[type].AddRange(new[] { "Ткань", "Наполнитель" });
                        if (type.Contains("вешалка".ToLower())) MaterialCategories[type].Add("Металл");
                    }
                }
                return Page();
            }

            var orderToUpdate = await _context.Orders
                .Include(o => o.OrderMaterials)
                .FirstOrDefaultAsync(o => o.Id == Order.Id);

            if (orderToUpdate == null)
            {
                return NotFound();
            }

            orderToUpdate.ClientName = Order.ClientName;
            orderToUpdate.PhoneNumber = Order.PhoneNumber;
            orderToUpdate.FurnitureType = Order.FurnitureType;
            orderToUpdate.FurnitureVariant = Order.FurnitureVariant;
            orderToUpdate.Dimensions = Order.Dimensions;
            orderToUpdate.Status = Order.Status;
            orderToUpdate.EmployeeId = Order.EmployeeId;

            if (orderToUpdate.OrderDate == DateTime.MinValue)
            {
                orderToUpdate.OrderDate = DateTime.Now;
            }

            _context.OrderMaterials.RemoveRange(orderToUpdate.OrderMaterials);
            orderToUpdate.OrderMaterials.Clear();

            if (SelectedMaterialIds != null && SelectedMaterialIds.Any())
            {
                foreach (var materialId in SelectedMaterialIds.Select(int.Parse))
                {
                    var material = await _context.Materials.FindAsync(materialId);
                    if (material != null)
                    {
                        _context.OrderMaterials.Add(new OrderMaterial { OrderId = orderToUpdate.Id, MaterialId = materialId, Quantity = 1 });
                    }
                }
            }
            await _context.SaveChangesAsync();

            var updatedOrder = await _context.Orders
                .Include(o => o.OrderMaterials)
                .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(o => o.Id == orderToUpdate.Id);
            if (updatedOrder != null)
            {
                updatedOrder.CalculateWeight(_context);
                updatedOrder.CalculateTotalCost(_context);
                Console.WriteLine($"After edit - Weight: {updatedOrder.Weight}, TotalCost: {updatedOrder.TotalCost}");
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}