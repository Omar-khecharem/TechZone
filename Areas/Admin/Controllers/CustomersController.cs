using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;

namespace TechZone.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AdminDefaults.AdminRoleName)]
    public class CustomersController : Controller
    {
        private const int PageSize = 12;

        private readonly IAdminService _adminService;

        public CustomersController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            ViewData["Title"] = "Clients";
            ViewData["AdminSubtitle"] = "Consultez les comptes et l'activité d'achat";

            var model = await _adminService.GetCustomersAsync(search, page, PageSize);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            ViewData["Title"] = "Fiche client";
            ViewData["AdminSubtitle"] = "Historique et informations du client";

            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var model = await _adminService.GetCustomerAsync(id);

            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }
    }
}
