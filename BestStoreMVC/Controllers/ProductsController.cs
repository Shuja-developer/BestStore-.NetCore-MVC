using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace BestStoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext  context;

        private readonly IWebHostEnvironment environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            var products = context.products.OrderByDescending(p => p.Id).ToList();

            return View(products);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            if(productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }
            if (!ModelState.IsValid)
            {
                return View(productDto);
            }
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);
            string ImageFullPath = environment.WebRootPath + "/Images/" + newFileName;
            using (var stream = System.IO.File.Create(ImageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }
            Product product = new Product()
            {
             Name = productDto.Name,
             Brand = productDto .Brand,
             Category = productDto .Category,
             Price = productDto.Price,
             Description = productDto .Description,
             ImageFileName = newFileName,
             CreatedAt = DateTime .Now,
            };
            context.products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }
        public IActionResult Edit(int id)
        {
            var product = context.products.Find(id);

            if(product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,

            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");



            return View(productDto);
        }
        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

                return View(productDto);
            }

            string newFileName = product.ImageFileName;
            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") +
                              Path.GetExtension(productDto.ImageFile.FileName);

                string imageFullPath = Path.Combine(environment.WebRootPath, "Images", newFileName);
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                string oldImageFullPath = Path.Combine(environment.WebRootPath, "Images", product.ImageFileName);
                if (System.IO.File.Exists(oldImageFullPath))
                {
                    System.IO.File.Delete(oldImageFullPath);
                }
            }

            // Update product in the database
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }
        public IActionResult Delete(int id)
        {
            var product = context.products.Find(id);
            if(product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            string imageFullPath = environment.WebRootPath + "/Images/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);
            context.products.Remove(product);
            context.SaveChanges(true);

            return RedirectToAction("Index", "Products");
        }

    }
}

