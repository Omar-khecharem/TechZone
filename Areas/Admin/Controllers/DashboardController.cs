using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;

namespace TechZone.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AdminDefaults.AdminRoleName)]
    public class DashboardController : Controller
    {
        private readonly IAdminService _adminService;

        public DashboardController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Tableau de bord";
            ViewData["AdminSubtitle"] = "Vue d'ensemble de la boutique TechZone";

            var model = await _adminService.GetDashboardAsync();

            return View(model);
        }
    }
}
