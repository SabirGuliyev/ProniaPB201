using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.DAL;
using ProniaMVC.Models;

using ProniaMVC.ViewModels;

namespace ProniaMVC.Controllers
{

    //DI dependency injection
    //IOC/DIP inverse of control/ dependency inversion principle
    //IOC container/DI Container
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
          
            

            HomeVM homeVM = new HomeVM { 
            Slides =await _context.Slides.OrderBy(s => s.Order).Take(2).ToListAsync(),
            Products=await _context.Products
            .OrderByDescending(p=>p.CreatedAt)
            .Take(8)
            .Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary!=null))
            .ToListAsync()
            };


            return View(homeVM);
        }
    }
}
