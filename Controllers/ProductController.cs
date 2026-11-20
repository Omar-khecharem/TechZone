using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels;

namespace TechZone.Controllers
{
    public class ProductController : Controller
    {
        private const int PageSize = 8;

        private static readonly HashSet<string> AllowedSorts = new(StringComparer.OrdinalIgnoreCase)
        {
            "featured",
            "price-asc",
            "price-desc",
            "name-asc",
            "newest"
        };

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
            bool inStock = false,
            string? sort = null,
            int page = 1)
        {
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

            sort = string.IsNullOrWhiteSpace(sort) ? "featured" : sort.Trim().ToLowerInvariant();
            if (!AllowedSorts.Contains(sort))
            {
                sort = "featured";
            }

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

            var categoryCounts = await _productService.GetCategoryProductCountsAsync();
            var (_, rawMaxPrice) = await _productService.GetPriceRangeAsync();

            var priceFloor = 0m;
            var priceCeil = rawMaxPrice > 0 ? Math.Ceiling(rawMaxPrice) : 1000m;
            if (priceCeil <= priceFloor)
            {
                priceCeil = priceFloor + 1000m;
            }

            var hasPriceFilter =
                (minPriceValue.HasValue && minPriceValue.Value > priceFloor)
                || (maxPriceValue.HasValue && maxPriceValue.Value < priceCeil);

            var categoryViewModels = categories
                .Select(category => new CategoryFilterViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = categoryCounts.TryGetValue(category.Id, out var count) ? count : 0
                })
                .ToList();
            ViewData["TechZoneCategories"] = categoryViewModels;

            var products = await _productService.GetProductsAsync(
                q,
                categoryId,
                minPriceValue,
                maxPriceValue,
                inStock,
                sort,
                Math.Max(page, 1),
                PageSize);

            var model = new CatalogViewModel
            {
                SearchTerm = q,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                HasPriceFilter = hasPriceFilter,
                InStockOnly = inStock,
                Sort = sort,
                PriceFloor = priceFloor,
                PriceCeil = priceCeil,
                Categories = categoryViewModels,
                Products = new PagedResult<ProductCardViewModel>
                {
                    Items = products.Items
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
