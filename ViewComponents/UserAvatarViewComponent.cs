using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;

namespace TechZone.ViewComponents
{
    public class UserAvatarViewComponent : ViewComponent
    {
        private readonly IAccountService _accountService;

        public UserAvatarViewComponent(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return View((ApplicationUser?)null);
            }

            var user = await _accountService.GetByIdAsync(userId);

            return View(user);
        }
    }
}
