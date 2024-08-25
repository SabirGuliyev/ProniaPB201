using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.DAL;
using ProniaMVC.Models;
using ProniaMVC.ViewModels;

namespace ProniaMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null||id<=0) return BadRequest();

            Product product=await _context.Products
                .Include(p=>p.Category)
                .Include(p=>p.ProductImages.OrderByDescending(pi=>pi.IsPrimary))
                .Include(p=>p.ProductTags).ThenInclude(pt=>pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            DetailVM detailVM = new DetailVM
            {
                Product = product,
                Products =await _context.Products.Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .Take(8)
                .ToListAsync()
            };

            return View(detailVM);
        }
    }
}
