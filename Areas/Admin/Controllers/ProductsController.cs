using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels.Admin;

namespace TechZone.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AdminDefaults.AdminRoleName)]
    public class ProductsController : Controller
    {
        private const int PageSize = 10;
        private const long MaxImageBytes = 3 * 1024 * 1024;

        private static readonly HashSet<string> AllowedImageExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private readonly IAdminService _adminService;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(IAdminService adminService, IWebHostEnvironment environment)
        {
            _adminService = adminService;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            ViewData["Title"] = "Produits";
            ViewData["AdminSubtitle"] = "Gérez le catalogue, les prix et les stocks";

            if (categoryId is <= 0)
            {
                categoryId = null;
            }

            var model = await _adminService.GetProductsAsync(search, categoryId, page, PageSize);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Nouveau produit";
            ViewData["AdminSubtitle"] = "Ajoutez un produit au catalogue";

            var model = new AdminProductFormViewModel
            {
                Categories = await _adminService.GetCategoriesAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProductFormViewModel model)
        {
            ViewData["Title"] = "Nouveau produit";
            ViewData["AdminSubtitle"] = "Ajoutez un produit au catalogue";

            model.Categories = await _adminService.GetCategoriesAsync();

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

            if (string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                ModelState.AddModelError(nameof(model.ImageUrl), "Fournissez une image (fichier ou URL).");
                return View(model);
            }

            var result = await _adminService.CreateProductAsync(model);

            if (!result.Succeeded)
            {
                DeleteImage(uploadedImage);
                ModelState.AddModelError(string.Empty, result.Error ?? "Impossible de créer le produit.");
                return View(model);
            }

            TempData["AdminSuccess"] = $"Le produit « {model.Name} » a été créé.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Modifier le produit";
            ViewData["AdminSubtitle"] = "Mettez à jour les informations du produit";

            var product = await _adminService.GetProductAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            var model = new AdminProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,
                ExistingImageUrl = product.ImageUrl,
                Categories = await _adminService.GetCategoriesAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminProductFormViewModel model)
        {
            ViewData["Title"] = "Modifier le produit";
            ViewData["AdminSubtitle"] = "Mettez à jour les informations du produit";

            model.Categories = await _adminService.GetCategoriesAsync();

            var existing = await _adminService.GetProductAsync(model.Id);

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

            var result = await _adminService.UpdateProductAsync(model);

            if (!result.Succeeded)
            {
                DeleteImage(uploadedImage);
                ModelState.AddModelError(string.Empty, result.Error ?? "Impossible de mettre à jour le produit.");
                return View(model);
            }

            if (uploadedImage is not null &&
                !string.Equals(uploadedImage, existing.ImageUrl, StringComparison.OrdinalIgnoreCase))
            {
                DeleteImage(existing.ImageUrl);
            }

            TempData["AdminSuccess"] = $"Le produit « {model.Name} » a été mis à jour.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _adminService.GetProductAsync(id);
            var result = await _adminService.DeleteProductAsync(id);

            if (!result.Succeeded)
            {
                TempData["AdminError"] = result.Error;
                return RedirectToAction(nameof(Index));
            }

            if (product is not null)
            {
                DeleteImage(product.ImageUrl);
            }

            TempData["AdminSuccess"] = "Le produit a été supprimé.";
            return RedirectToAction(nameof(Index));
        }

        private void ValidateImage(AdminProductFormViewModel model)
        {
            if (model.ImageFile is not { Length: > 0 })
            {
                return;
            }

            var extension = Path.GetExtension(model.ImageFile.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.ImageFile), "Formats acceptés : JPG, PNG, GIF ou WEBP.");
            }
            else if (model.ImageFile.Length > MaxImageBytes)
            {
                ModelState.AddModelError(nameof(model.ImageFile), "L'image ne doit pas dépasser 3 Mo.");
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
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "products");

            Directory.CreateDirectory(folder);

            var filePath = Path.Combine(folder, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            return $"/uploads/products/{fileName}";
        }

        private void DeleteImage(string? relativePath)
        {
            const string prefix = "/uploads/products/";

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
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "products", fileName);

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
