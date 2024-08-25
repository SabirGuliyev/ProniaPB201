using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaMVC.Areas.ProniaAdmin.ViewModels;
using ProniaMVC.DAL;
using ProniaMVC.Models;
using ProniaMVC.Utilities.Enums;
using ProniaMVC.Utilities.Extensions;

namespace ProniaMVC.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class SlideController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {

            List<Slide> slides = await _context.Slides.Where(s => s.IsDeleted == false).ToListAsync();

            return View(slides);
        }

        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Test()
        {

            //56021ef9-0075-4fd0-9ff2-f48609bdf586 flower.jpg
            return Content(Guid.NewGuid().ToString());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSlideVM slideVM)
        {
            if (!ModelState.IsValid) return View();


            if (!slideVM.Photo.ValidateType("image/"))
            {

                ModelState.AddModelError("Photo", "File type is not correct");
                return View();
            }

            if (!slideVM.Photo.ValidateSize(FileSize.MB,2))
            {
                ModelState.AddModelError("Photo", "File size must be less than 2mb");
                return View();
            }

            string fileName =await slideVM.Photo.CreateFileAsync(_env.WebRootPath,"assets","images","website-images");


            Slide slide = new Slide
            {
                Title = slideVM.Title,
                SubTitle = slideVM.SubTitle,
                Description = slideVM.Description,
                Order = slideVM.Order,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                Image = fileName
            };

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (slide is null) return NotFound();

            UpdateSlideVM slideVM = new UpdateSlideVM {
            Image= slide.Image,
            Title = slide.Title,
            SubTitle= slide.SubTitle,
            Description = slide.Description,
            Order= slide.Order
            
            };
            return View(slideVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id,UpdateSlideVM slideVM)
        {
            if (!ModelState.IsValid) return View(slideVM);

            Slide existed=await _context.Slides.FirstOrDefaultAsync(s=>s.Id== id);
            //slideVM.Image = existed.Image;
            if (existed is null) return NotFound();

            if(slideVM.Photo is not null)
            {
                if (!slideVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "File type is not correct");
                    return View(slideVM);
                }
                if (!slideVM.Photo.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "File size is not correct");
                    return View(slideVM);
                }
                string fileName = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                existed.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.Image = fileName;
            }

            existed.Title = slideVM.Title;
            existed.Description = slideVM.Description;
            existed.SubTitle= slideVM.SubTitle;
            existed.Order= slideVM.Order;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide is null) return NotFound();

            slide.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");

            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

          
        }
    }
}
