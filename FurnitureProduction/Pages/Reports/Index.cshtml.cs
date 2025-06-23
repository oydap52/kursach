using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FurnitureProduction.Data;
using FurnitureProduction.Models;
using Microsoft.AspNetCore.Authorization;
using System;

namespace FurnitureProduction.Pages.Reports
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public int TotalOrders { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalMaterials { get; set; }
        public List<OrderReport> OrderReports { get; set; }

        public class OrderReport
        {
            public int OrderId { get; set; }
            public string ClientName { get; set; }
            public string FurnitureType { get; set; }
            public string FurnitureVariant { get; set; }
            public DateTime? OrderDate { get; set; }
            public decimal? TotalCost { get; set; }
            public string Status { get; set; }
            public string EmployeeName { get; set; }
            public List<MaterialUsage> MaterialsUsed { get; set; }
        }

        public class MaterialUsage
        {
            public string Name { get; set; }
        }

        public async Task OnGetAsync()
        {
            TotalOrders = await _context.Orders.CountAsync();
            TotalEmployees = await _context.Employees.CountAsync();
            TotalMaterials = await _context.Materials.CountAsync();

            var query = _context.Orders
                .Include(o => o.Employee)
                .Include(o => o.OrderMaterials)
                .ThenInclude(om => om.Material)
                .AsQueryable();

            if (StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= StartDate.Value);
            }
            if (EndDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= EndDate.Value);
            }

            OrderReports = await query
                .Select(o => new OrderReport
                {
                    OrderId = o.Id,
                    ClientName = o.ClientName,
                    FurnitureType = o.FurnitureType,
                    FurnitureVariant = o.FurnitureVariant ?? "Не указан",
                    OrderDate = o.OrderDate,
                    TotalCost = o.TotalCost,
                    Status = o.Status,
                    EmployeeName = o.Employee != null ? o.Employee.FullName : "Не назначен",
                    MaterialsUsed = o.OrderMaterials.Select(om => new MaterialUsage
                    {
                        Name = om.Material.Name
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}