using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels;

namespace TechZone.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetProductsAsync(
                searchTerm: null,
                categoryId: null,
                minPrice: null,
                maxPrice: null,
                inStockOnly: false,
                sort: "featured",
                page: 1,
                pageSize: 12);
            var categories = await _productService.GetCategoriesAsync();
            var categoryViewModels = categories
                .Select(category => new CategoryFilterViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl
                })
                .ToList();

            ViewData["TechZoneCategories"] = categoryViewModels;

            return View(new HomeViewModel
            {
                FeaturedProducts = products.Items
                    .Select(product => new ProductCardViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        ImageUrl = product.ImageUrl,
                        CategoryId = product.CategoryId,
                        CategoryName = product.Category.Name,
                        Stock = product.Stock,
                        SalesCount = product.SalesCount
                    })
                    .ToList(),
                Categories = categoryViewModels
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
