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
        public class IndexModel : PageModel
        {
            private readonly FurnitureProduction.Data.AppDbContext _context;

            public IndexModel(FurnitureProduction.Data.AppDbContext context)
            {
                _context = context;
            }

            public IList<Material> Material { get;set; } = default!;

            public async Task OnGetAsync()
            {
                Material = await _context.Materials.ToListAsync();
            }
        }
    }
