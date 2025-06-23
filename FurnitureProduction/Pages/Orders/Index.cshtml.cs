using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FurnitureProduction.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FurnitureProduction.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Order> Orders { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinCost { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxCost { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Orders
                .Include(o => o.Employee)
                .Include(o => o.OrderMaterials)
                .ThenInclude(om => om.Material)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(o => o.ClientName.Contains(SearchString) || o.FurnitureType.Contains(SearchString) || o.Status.Contains(SearchString));
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(o => o.Status == StatusFilter);
            }

            if (StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= EndDate.Value);
            }

            if (MinCost.HasValue)
            {
                query = query.Where(o => o.TotalCost >= MinCost.Value);
            }

            if (MaxCost.HasValue)
            {
                query = query.Where(o => o.TotalCost <= MaxCost.Value);
            }

            Orders = await query.ToListAsync();
        }
    }
}