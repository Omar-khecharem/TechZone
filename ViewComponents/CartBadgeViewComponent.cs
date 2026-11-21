using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;

namespace TechZone.ViewComponents
{
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartBadgeViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return View(0);
            }

            var cart = await _cartService.GetCartAsync(userId);
            var count = cart?.Items.Sum(item => item.Quantity) ?? 0;

            return View(count);
        }
    }
}
