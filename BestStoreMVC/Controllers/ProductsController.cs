using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;
        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            var products = context.Products.OrderByDescending(p => p.Id).ToList();
            return View(products);
        }
        public IActionResult Create() => View();
        [HttpPost]
        public IActionResult Create(ProductDto productDto) {
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The Image file is required");
            }
            if (!ModelState.IsValid)
            {
                return View(productDto);
            }
            string newFileName = DateTime.Now.ToString("yyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);
            string imageFullPath = environment.WebRootPath + "/images/" + newFileName;
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Categoy = productDto.Categoy,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreateAt = DateTime.Now,

            };
            context.Products.Add(product);
            context.SaveChanges();
            return RedirectToAction("Index", "Products");
        }

        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
            if (product == null) return RedirectToAction("Index", "Products");
            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Categoy = product.Categoy,
                Price = product.Price,
                Description = product.Description,
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreateAt"] = product.CreateAt.ToString("MM/dd/yyyy");

            return View(productDto);
        }
        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            if (!ModelState.IsValid) {

                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreateAt"] = product.CreateAt.ToString("MM/dd/yyyy");
                return View(productDto);
            }

            // update the image file if we have a new image file
            string newFileName = product.ImageFileName;
            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile.FileName);
                string imageFullPath = environment.WebRootPath + "/images/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }
                string oldImageFullPath = environment.WebRootPath + "/images/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Categoy = productDto.Categoy;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();
            return RedirectToAction("Index", "Products");
        }
        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
            if (product == null) 
            {
                return RedirectToAction("Index", "Products");
            }
            string imageFullPath = environment.WebRootPath + "/images/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);
            context.Products.Remove(product);
            context.SaveChanges(true);
            return RedirectToAction("Index", "Products");
        } 

        }

    }

