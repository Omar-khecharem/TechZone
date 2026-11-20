using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;

namespace TechZone.ViewComponents
{
    public class CategoryNavViewComponent : ViewComponent
    {
        private readonly IProductService _productService;

        public CategoryNavViewComponent(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool showPrimaryLinks = true)
        {
            const string cacheKey = "TechZone.CategoryNav";

            ViewData["ShowPrimaryLinks"] = showPrimaryLinks;

            if (HttpContext.Items.TryGetValue(cacheKey, out var cached) &&
                cached is IReadOnlyList<Category> cachedCategories)
            {
                return View(cachedCategories);
            }

            var categories = await _productService.GetCategoriesAsync();
            HttpContext.Items[cacheKey] = categories;

            return View(categories);
        }
    }
}
