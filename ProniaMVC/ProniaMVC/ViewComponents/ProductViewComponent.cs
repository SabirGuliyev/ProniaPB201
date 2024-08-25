using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.DAL;
using ProniaMVC.Models;
using ProniaMVC.Utilities.Enums;

namespace ProniaMVC.ViewComponents
{
    public class ProductViewComponent:ViewComponent
    {
        private readonly AppDbContext _context;

        public ProductViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(SortType type)
        {
            IQueryable<Product> query = _context.Products.Where(p => p.IsDeleted == false);

            switch (type)
            {
                case SortType.Name:
                    query=query.OrderBy(p => p.Name);
                    break;
                case SortType.Price:
                    query=query.OrderByDescending(p => p.Price);
                    break;
                case SortType.Newest:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break; 
            }

            query = query.Take(8).Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null));

            return View(await query.ToListAsync());

            //return View(await Task.FromResult(products));
        }
    }
}
