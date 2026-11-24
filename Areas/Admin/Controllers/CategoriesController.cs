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
        private const long MaxImageBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedImageExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp",
                ".avif", ".bmp", ".tif", ".tiff", ".svg", ".ico", ".heic", ".heif"
            };

        private readonly IAdminService _adminService;
        private readonly IWebHostEnvironment _environment;

        public CategoriesController(IAdminService adminService, IWebHostEnvironment environment)
        {
            _adminService = adminService;
            _environment = environment;
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

            ValidateImage(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? uploadedImage = null;

            if (model.ImageFile is { Length: > 0 })
            {
                uploadedImage = await SaveImageAsync(model.ImageFile);
            }

            model.ImageUrl = uploadedImage ?? model.ImageUrl?.Trim();

            var result = await _adminService.CreateCategoryAsync(model);

            if (!result.Succeeded)
            {
                DeleteImage(uploadedImage);
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
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ExistingImageUrl = category.ImageUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminCategoryFormViewModel model)
        {
            ViewData["Title"] = "Modifier la catégorie";
            ViewData["AdminSubtitle"] = "Mettez à jour la catégorie";

            var existing = await _adminService.GetCategoryAsync(model.Id);

            if (existing is null)
            {
                return NotFound();
            }

            model.ExistingImageUrl = existing.ImageUrl;

            ValidateImage(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? uploadedImage = null;

            if (model.ImageFile is { Length: > 0 })
            {
                uploadedImage = await SaveImageAsync(model.ImageFile);
            }

            model.ImageUrl = uploadedImage
                ?? (string.IsNullOrWhiteSpace(model.ImageUrl) ? existing.ImageUrl : model.ImageUrl.Trim());

            var result = await _adminService.UpdateCategoryAsync(model);

            if (!result.Succeeded)
            {
                DeleteImage(uploadedImage);
                ModelState.AddModelError(string.Empty, result.Error ?? "Impossible de mettre à jour la catégorie.");
                return View(model);
            }

            if (uploadedImage is not null &&
                !string.Equals(uploadedImage, existing.ImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                DeleteImage(existing.ImageUrl);
            }

            TempData["AdminSuccess"] = $"La catégorie « {model.Name} » a été mise à jour.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _adminService.GetCategoryAsync(id);
            var result = await _adminService.DeleteCategoryAsync(id);

            if (!result.Succeeded)
            {
                TempData["AdminError"] = result.Error;
            }
            else
            {
                if (category is not null)
                {
                    DeleteImage(category.ImageUrl);
                }

                TempData["AdminSuccess"] = "La catégorie a été supprimée.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void ValidateImage(AdminCategoryFormViewModel model)
        {
            if (model.ImageFile is not { Length: > 0 })
            {
                return;
            }

            var extension = Path.GetExtension(model.ImageFile.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.ImageFile), "Formats acceptés : JPG, JPEG, PNG, GIF, WEBP, AVIF, BMP, TIFF, SVG, ICO, HEIC ou HEIF.");
            }
            else if (model.ImageFile.Length > MaxImageBytes)
            {
                ModelState.AddModelError(nameof(model.ImageFile), "L'image ne doit pas dépasser 10 Mo.");
            }
            else if (!string.IsNullOrWhiteSpace(model.ImageFile.ContentType) &&
                     !model.ImageFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.ImageFile), "Le fichier sélectionné n'est pas une image valide.");
            }
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "categories");

            Directory.CreateDirectory(folder);

            var filePath = Path.Combine(folder, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            return $"/uploads/categories/{fileName}";
        }

        private void DeleteImage(string? relativePath)
        {
            const string prefix = "/uploads/categories/";

            if (string.IsNullOrWhiteSpace(relativePath) ||
                !relativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var fileName = Path.GetFileName(relativePath);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "categories", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
