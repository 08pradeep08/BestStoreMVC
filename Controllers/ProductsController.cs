using BestStoreMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System;
using BestStoreMVC.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            var products = context.Products.ToList();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            if (productDto.ImageFile == null || productDto.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }

            if (!ModelState.IsValid)
            {
                return View(productDto); // Return the DTO back to the view
            }

            // Generate a unique file name
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(productDto.ImageFile.FileName);

            // Build the path to save the image
            string imageFolderPath = Path.Combine(environment.WebRootPath, "Products");
            Directory.CreateDirectory(imageFolderPath); // Ensure the directory exists
            string imageFullPath = Path.Combine(imageFolderPath, newFileName);

            // Save the image file
            using (var stream = new FileStream(imageFullPath, FileMode.Create))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            // Create and save the product
            Product product = new Product
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Price = productDto.Price,
                Description = productDto.Description,
                Category = productDto.Category,
                ImageFileName = newFileName, // Store only the file name
                Created = DateTime.Now
            };

            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index");

        }
        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Product");
            }

            //Create productdto from product
            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Price = product.Price,
                Description = product.Description,
                Category = product.Category,
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreateAt"] = product.Created.ToString("MM/dd/yyyy");


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

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreateAt"] = product.Created.ToString("MM/dd/yyyy");

                return View(productDto);
            }
            // update the image file if we have a new image file

            string newFileName = product.ImageFileName;

            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                newFileName += Path.GetExtension(productDto.ImageFile.FileName);



                string imageFullPath = environment.WebRootPath + "/products/" + newFileName;

                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }
                // delete the old image

                string oldImageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }

            // update the product in the database

            product.Name = productDto.Name;

            product.Brand = productDto.Brand;

            product.Category = productDto.Category;

            product.Price = productDto.Price;

            product.Description = productDto.Description;

            product.ImageFileName = newFileName;

            context.SaveChanges();

            return RedirectToAction("Index");

        }
        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            string imageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName; 
            System.IO.File.Delete(imageFullPath);


            context.Products.Remove(product);
            context.SaveChanges(true);

            return RedirectToAction("Index");   
        }
    }
}
