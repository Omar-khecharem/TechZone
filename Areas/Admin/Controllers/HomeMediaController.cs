using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels.Admin;

namespace TechZone.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AdminDefaults.AdminRoleName)]
    public class HomeMediaController : Controller
    {
        private const long MaxVideoBytes = 50 * 1024 * 1024;
        private const long MaxImageBytes = 10 * 1024 * 1024;
        private const string UploadRoot = "home";

        private static readonly HashSet<string> AllowedVideoExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".mp4", ".webm", ".ogv", ".ogg", ".m4v"
            };

        private static readonly HashSet<string> AllowedImageExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp",
                ".avif", ".bmp", ".tif", ".tiff", ".svg", ".ico", ".heic", ".heif"
            };

        private readonly IHomeMediaService _homeMediaService;
        private readonly IWebHostEnvironment _environment;

        public HomeMediaController(IHomeMediaService homeMediaService, IWebHostEnvironment environment)
        {
            _homeMediaService = homeMediaService;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            SetPageCopy();

            var media = await _homeMediaService.GetAsync();

            return View(new AdminHomeMediaViewModel
            {
                HeroVideoUrl = media.HeroVideoUrl,
                HeroPosterUrl = media.HeroPosterUrl,
                PromoImageUrl = media.PromoImageUrl,
                UpdatedAt = media.UpdatedAt == default ? null : media.UpdatedAt
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AdminHomeMediaViewModel model)
        {
            SetPageCopy();

            var existing = await _homeMediaService.GetAsync();

            ValidateVideo(model.HeroVideoFile, nameof(model.HeroVideoFile));
            ValidateImage(model.HeroPosterFile, nameof(model.HeroPosterFile));
            ValidateImage(model.PromoImageFile, nameof(model.PromoImageFile));

            if (!ModelState.IsValid)
            {
                RestoreExisting(model, existing);
                return View(nameof(Index), model);
            }

            var uploadedVideo = model.HeroVideoFile is { Length: > 0 }
                ? await SaveFileAsync(model.HeroVideoFile, "videos")
                : null;
            var uploadedPoster = model.HeroPosterFile is { Length: > 0 }
                ? await SaveFileAsync(model.HeroPosterFile, "images")
                : null;
            var uploadedPromo = model.PromoImageFile is { Length: > 0 }
                ? await SaveFileAsync(model.PromoImageFile, "images")
                : null;

            var heroVideoUrl = ResolveMediaUrl(existing.HeroVideoUrl, model.HeroVideoUrl, uploadedVideo, model.RemoveHeroVideo);
            var heroPosterUrl = ResolveMediaUrl(existing.HeroPosterUrl, model.HeroPosterUrl, uploadedPoster, model.RemoveHeroPoster);
            var promoImageUrl = ResolveMediaUrl(existing.PromoImageUrl, model.PromoImageUrl, uploadedPromo, model.RemovePromoImage);

            var result = await _homeMediaService.UpdateAsync(heroVideoUrl, heroPosterUrl, promoImageUrl);

            if (!result.Succeeded)
            {
                DeleteFile(uploadedVideo);
                DeleteFile(uploadedPoster);
                DeleteFile(uploadedPromo);
                ModelState.AddModelError(string.Empty, result.Error ?? "Impossible d'enregistrer les médias.");
                RestoreExisting(model, existing);
                return View(nameof(Index), model);
            }

            DeleteIfReplaced(existing.HeroVideoUrl, heroVideoUrl);
            DeleteIfReplaced(existing.HeroPosterUrl, heroPosterUrl);
            DeleteIfReplaced(existing.PromoImageUrl, promoImageUrl);

            TempData["AdminSuccess"] = "Les médias de l'accueil ont été mis à jour.";
            return RedirectToAction(nameof(Index));
        }

        private void SetPageCopy()
        {
            ViewData["Title"] = "Médias de l'accueil";
            ViewData["AdminSubtitle"] = "Contrôlez la vidéo du hero et les images de la page d'accueil";
        }

        private static void RestoreExisting(AdminHomeMediaViewModel model, HomeMedia existing)
        {
            model.HeroVideoUrl = existing.HeroVideoUrl;
            model.HeroPosterUrl = existing.HeroPosterUrl;
            model.PromoImageUrl = existing.PromoImageUrl;
            model.UpdatedAt = existing.UpdatedAt == default ? null : existing.UpdatedAt;
        }

        private static string? ResolveMediaUrl(
            string? existingUrl,
            string? typedUrl,
            string? uploadedUrl,
            bool remove)
        {
            if (uploadedUrl is not null)
            {
                return uploadedUrl;
            }

            if (remove)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(typedUrl))
            {
                return typedUrl.Trim();
            }

            return existingUrl;
        }

        private void ValidateVideo(IFormFile? file, string fieldName)
        {
            if (file is not { Length: > 0 })
            {
                return;
            }

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedVideoExtensions.Contains(extension))
            {
                ModelState.AddModelError(fieldName, "Formats vidéo acceptés : MP4, WEBM, OGG ou M4V.");
            }
            else if (file.Length > MaxVideoBytes)
            {
                ModelState.AddModelError(fieldName, "La vidéo ne doit pas dépasser 50 Mo.");
            }
            else if (!string.IsNullOrWhiteSpace(file.ContentType) &&
                     !file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(file.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(fieldName, "Le fichier sélectionné n'est pas une vidéo valide.");
            }
        }

        private void ValidateImage(IFormFile? file, string fieldName)
        {
            if (file is not { Length: > 0 })
            {
                return;
            }

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError(fieldName, "Formats acceptés : JPG, PNG, GIF, WEBP, AVIF, BMP, TIFF, SVG, ICO, HEIC ou HEIF.");
            }
            else if (file.Length > MaxImageBytes)
            {
                ModelState.AddModelError(fieldName, "L'image ne doit pas dépasser 10 Mo.");
            }
            else if (!string.IsNullOrWhiteSpace(file.ContentType) &&
                     !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(fieldName, "Le fichier sélectionné n'est pas une image valide.");
            }
        }

        private async Task<string> SaveFileAsync(IFormFile file, string subfolder)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var folder = Path.Combine(_environment.WebRootPath, "uploads", UploadRoot, subfolder);

            Directory.CreateDirectory(folder);

            var filePath = Path.Combine(folder, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            return $"/uploads/{UploadRoot}/{subfolder}/{fileName}";
        }

        private void DeleteIfReplaced(string? oldUrl, string? newUrl)
        {
            if (string.IsNullOrWhiteSpace(oldUrl) ||
                string.Equals(oldUrl, newUrl, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            DeleteFile(oldUrl);
        }

        private void DeleteFile(string? relativePath)
        {
            var prefix = $"/uploads/{UploadRoot}/";

            if (string.IsNullOrWhiteSpace(relativePath) ||
                !relativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                var relative = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var filePath = Path.Combine(_environment.WebRootPath, relative);
                var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", UploadRoot);

                if (!filePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

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
