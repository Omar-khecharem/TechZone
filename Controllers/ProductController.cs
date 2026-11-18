using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels;

namespace TechZone.Controllers
{
    public class ProductController : Controller
    {
        private const int PageSize = 9;

        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(
            string? q,
            int? categoryId,
            string? minPrice,
            string? maxPrice,
            int page = 1)
        {
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

            if (q?.Length > 100)
            {
                q = q[..100];
            }

            decimal? minPriceValue = null;
            decimal? maxPriceValue = null;

            if (!string.IsNullOrWhiteSpace(minPrice))
            {
                if (TryParsePrice(minPrice, out var parsedMinPrice))
                {
                    minPriceValue = parsedMinPrice;
                }
                else
                {
                    ModelState.AddModelError("minPrice", "Le prix minimum doit être un nombre positif.");
                }
            }

            if (!string.IsNullOrWhiteSpace(maxPrice))
            {
                if (TryParsePrice(maxPrice, out var parsedMaxPrice))
                {
                    maxPriceValue = parsedMaxPrice;
                }
                else
                {
                    ModelState.AddModelError("maxPrice", "Le prix maximum doit être un nombre positif.");
                }
            }

            if (minPriceValue.HasValue && maxPriceValue.HasValue && minPriceValue > maxPriceValue)
            {
                (minPriceValue, maxPriceValue) = (maxPriceValue, minPriceValue);
                (minPrice, maxPrice) = (maxPrice, minPrice);
            }

            if (minPriceValue.HasValue)
            {
                minPrice = minPriceValue.Value.ToString("0.##", CultureInfo.CurrentCulture);
            }

            if (maxPriceValue.HasValue)
            {
                maxPrice = maxPriceValue.Value.ToString("0.##", CultureInfo.CurrentCulture);
            }

            if (categoryId is <= 0)
            {
                categoryId = null;
            }

            var categories = await _productService.GetCategoriesAsync();

            if (categoryId.HasValue && categories.All(category => category.Id != categoryId.Value))
            {
                ModelState.AddModelError("categoryId", "La catégorie sélectionnée n'existe pas.");
                categoryId = null;
            }

            var products = await _productService.GetProductsAsync(
                q,
                categoryId,
                minPriceValue,
                maxPriceValue,
                Math.Max(page, 1),
                PageSize);

            var model = new CatalogViewModel
            {
                SearchTerm = q,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Categories = categories
                    .Select(category => new CategoryFilterViewModel
                    {
                        Id = category.Id,
                        Name = category.Name
                    })
                    .ToList(),
                Products = new PagedResult<ProductCardViewModel>
                {
                    Items = products.Items
                        .Select(product => new ProductCardViewModel
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Price = product.Price,
                            ImageUrl = product.ImageUrl,
                            CategoryName = product.Category.Name
                        })
                        .ToList(),
                    Page = products.Page,
                    PageSize = products.PageSize,
                    TotalCount = products.TotalCount
                }
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var product = await _productService.GetProductByIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            return View(new ProductDetailsViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CategoryName = product.Category.Name,
                Stock = product.Stock
            });
        }

        private static bool TryParsePrice(string value, out decimal price)
        {
            var normalizedValue = value
                .Trim()
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty)
                .Replace(',', '.');

            return decimal.TryParse(
                       normalizedValue,
                       NumberStyles.Number,
                       CultureInfo.InvariantCulture,
                       out price)
                   && price >= 0;
        }
    }
}
