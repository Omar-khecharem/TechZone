using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;

namespace TechZone.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AdminDefaults.AdminRoleName)]
    public class OrdersController : Controller
    {
        private const int PageSize = 12;

        private readonly IAdminService _adminService;

        public OrdersController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index(string? search, OrderStatus? status, int page = 1)
        {
            ViewData["Title"] = "Commandes";
            ViewData["AdminSubtitle"] = "Suivez et mettez à jour les commandes";

            if (status.HasValue && !Enum.IsDefined(status.Value))
            {
                status = null;
            }

            var model = await _adminService.GetOrdersAsync(search, status, page, PageSize);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Détail de la commande";
            ViewData["AdminSubtitle"] = "Consultez et traitez la commande";

            var model = await _adminService.GetOrderAsync(id);

            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, OrderStatus status, PaymentStatus paymentStatus)
        {
            var result = await _adminService.UpdateOrderAsync(id, status, paymentStatus);

            if (!result.Succeeded)
            {
                TempData["AdminError"] = result.Error;
            }
            else
            {
                TempData["AdminSuccess"] = "La commande a été mise à jour.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
