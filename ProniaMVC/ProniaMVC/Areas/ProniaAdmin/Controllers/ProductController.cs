using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using ProniaMVC.Areas.ProniaAdmin.ViewModels;

using ProniaMVC.DAL;
using ProniaMVC.Models;
using ProniaMVC.Utilities.Enums;
using ProniaMVC.Utilities.Extensions;
using static System.Net.Mime.MediaTypeNames;

namespace ProniaMVC.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<GetAdminProductVM> productVMs = await _context.Products
                .Where(p => p.IsDeleted == false)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Select(p => new GetAdminProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    Image = p.ProductImages.FirstOrDefault(pi=>pi.IsPrimary==true).ImageURL
                })
                .ToListAsync();

            return View(productVMs);
        }

        public async Task<IActionResult> Create()
        {


            CreateProductVM productVM = new CreateProductVM
            {
                Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync(),
                Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync()
            };
            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
            productVM.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();
            if (!ModelState.IsValid)
            {

                return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError("MainPhoto", "File type is not correct");
                return View(productVM);
            }
            if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 1))
            {
                ModelState.AddModelError("MainPhoto", "File size is not correct");
                return View(productVM);
            }

            if (!productVM.HoverPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError("HoverPhoto", "File type is not correct");
                return View(productVM);
            }
            if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 1))
            {
                ModelState.AddModelError("HoverPhoto", "File size is not correct");
                return View(productVM);
            }





            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId && c.IsDeleted == false);
            if (!result)
            {
                ModelState.AddModelError("CategoryId", "Category does not exist");


                return View(productVM);
            }

            if (productVM.TagIds is not null)
            {

                bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));
                if (tagResult)
                {
                    ModelState.AddModelError("TagIds", "Tag does not exist");
                    return View(productVM);
                }
            }

            ProductImage mainImage = new ProductImage
            {
                CreatedAt=DateTime.Now,
                IsDeleted=false,
                IsPrimary=true,
                ImageURL=await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath,"assets","images","website-images")
            };
            ProductImage hoverImage = new ProductImage
            {
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                IsPrimary = false,
                ImageURL = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
            };

            Product product = new Product
            {
                CategoryId = productVM.CategoryId.Value,
                SKU = productVM.SKU,
                Description = productVM.Description,
                Name = productVM.Name,
                Price = productVM.Price,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                ProductImages=new List<ProductImage> { mainImage, hoverImage }

            };

            if(productVM.Photos is not null)
            {
                string text = string.Empty;
                foreach (IFormFile file in productVM.Photos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text += $"{file.FileName} named file type is not correct";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 1))
                    {
                        text += $"{file.FileName} named file size is not correct";
                        continue;
                    }
                    ProductImage image = new ProductImage
                    {
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null,
                        ImageURL = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                    };
                    product.ProductImages.Add(image);


                }
                TempData["ErrorMessage"] = text;
            }

            if (productVM.TagIds is not null)
            {
                product.ProductTags = productVM.TagIds.Select(tId => new ProductTag
                {
                    TagId = tId

                }).ToList();
            }


            //foreach (var tId in productVM.TagIds)
            //{

            //    ProductTag pTag = new ProductTag
            //    {
            //        TagId = tId,
            //        Product = product
            //    };
            //    _context.ProductTags.Add(pTag);
            //}
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) return BadRequest();
            Product product = await _context.Products.Include(p=>p.ProductImages).Include(p => p.ProductTags).FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);
            if (product == null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                TagIds = product.ProductTags.Select(pt => pt.TagId).ToList(),
                Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync(),
                Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync(),
                Images=product.ProductImages.ToList()
            };

            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM productVM)
        {
            if (id == null || id < 1) return BadRequest();
            Product existed = await _context.Products.Include(p=>p.ProductImages).Include(p => p.ProductTags).FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);
            if (existed == null) return NotFound();


            productVM.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
            productVM.Tags = await _context.Tags.Where(c => !c.IsDeleted).ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(productVM);
            }

            if(productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError("MainPhoto", "File type is not correct");
                    return View(productVM);
                }
                if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError("MainPhoto", "File size is not correct");
                    return View(productVM);
                }

            }
            if(productVM.HoverPhoto is not null)
            {
                if (!productVM.HoverPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError("HoverPhoto", "File type is not correct");
                    return View(productVM);
                }
                if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError("HoverPhoto", "File size is not correct");
                    return View(productVM);
                }

            }

            if (existed.CategoryId != productVM.CategoryId)
            {
                bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId && c.IsDeleted == false);
                if (!result)
                {
                    ModelState.AddModelError("CategoryId", "Category does not exist");
                    return View(productVM);
                }
            }

            if(productVM.MainPhoto is not null)
            {
                ProductImage main = new ProductImage
                {
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    IsPrimary = true,
                    ImageURL = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                };
                ProductImage existedMain = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true);
                existedMain.ImageURL.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(existedMain);
                existed.ProductImages.Add(main);
            }
            if (productVM.HoverPhoto is not null)
            {
                ProductImage hover = new ProductImage
                {
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    IsPrimary = false,
                    ImageURL = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                };
                ProductImage existedHover = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false);
                existedHover.ImageURL.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(existedHover);
                existed.ProductImages.Add(hover);
            }
            
            //1 2 3 4     0
            if (productVM.ImageIds is null)
            {
                productVM.ImageIds = new List<int>();
            }
          
            var deleteImages = existed.ProductImages.Where(pi => !productVM.ImageIds.Exists(imgId => pi.Id == imgId)&&pi.IsPrimary==null).ToList();
            foreach (ProductImage delImage in deleteImages)
            {
                delImage.ImageURL.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                //existed.ProductImages.Remove(delImage);
            }

            if(productVM.Photos is not null)
            {
                string text = string.Empty;
                foreach (IFormFile file in productVM.Photos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text += $"{file.FileName} named file type is not correct";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 1))
                    {
                        text += $"{file.FileName} named file size is not correct";
                        continue;
                    }
                    ProductImage image = new ProductImage
                    {
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null,
                        ImageURL = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                    };
                    existed.ProductImages.Add(image);

                }
                TempData["ErrorMessage"] = text;
            }
           

            _context.ProductImages.RemoveRange(deleteImages);
            _context.ProductTags.RemoveRange(existed.ProductTags.Where(pt => !productVM.TagIds.Exists(tId => tId == pt.TagId)).ToList());
            existed.ProductTags.AddRange(productVM.TagIds.Where(tId => !existed.ProductTags.Any(pt => pt.TagId == tId)).Select(tId => new ProductTag { TagId = tId }));

            //foreach (int tagId in productVM.TagIds)
            //{
            //    if (!existed.ProductTags.Any(pt => pt.TagId == tagId))
            //    {
            //        ProductTag pTag = new ProductTag
            //        {
            //            TagId = tagId
            //        };
            //        existed.ProductTags.Add(pTag);
            //    }
            //}
            existed.Name=productVM.Name;
            existed.Description=productVM.Description;
            existed.Price=productVM.Price;
            existed.SKU=productVM.SKU;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        
    }
}
