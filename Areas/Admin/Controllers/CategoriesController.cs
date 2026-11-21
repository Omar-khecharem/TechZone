using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels.Admin;

namespace TechZone.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AdminDefaults.AdminRoleName)]
    public class CategoriesController : Controller
    {
        private readonly IAdminService _adminService;

        public CategoriesController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Catégories";
            ViewData["AdminSubtitle"] = "Organisez le catalogue par familles de produits";

            var model = await _adminService.GetCategoryListAsync();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Nouvelle catégorie";
            ViewData["AdminSubtitle"] = "Créez une nouvelle famille de produits";

            return View(new AdminCategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCategoryFormViewModel model)
        {
            ViewData["Title"] = "Nouvelle catégorie";
            ViewData["AdminSubtitle"] = "Créez une nouvelle famille de produits";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _adminService.CreateCategoryAsync(model);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Impossible de créer la catégorie.");
                return View(model);
            }

            TempData["AdminSuccess"] = $"La catégorie « {model.Name} » a été créée.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Modifier la catégorie";
            ViewData["AdminSubtitle"] = "Mettez à jour la catégorie";

            var category = await _adminService.GetCategoryAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            return View(new AdminCategoryFormViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminCategoryFormViewModel model)
        {
            ViewData["Title"] = "Modifier la catégorie";
            ViewData["AdminSubtitle"] = "Mettez à jour la catégorie";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _adminService.UpdateCategoryAsync(model);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Impossible de mettre à jour la catégorie.");
                return View(model);
            }

            TempData["AdminSuccess"] = $"La catégorie « {model.Name} » a été mise à jour.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _adminService.DeleteCategoryAsync(id);

            if (!result.Succeeded)
            {
                TempData["AdminError"] = result.Error;
            }
            else
            {
                TempData["AdminSuccess"] = "La catégorie a été supprimée.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
