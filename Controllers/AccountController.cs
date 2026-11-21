using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels;

namespace TechZone.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private const long MaxProfilePictureBytes = 2 * 1024 * 1024;

        private static readonly HashSet<string> AllowedProfilePictureExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private readonly IAccountService _accountService;
        private readonly IOrderService _orderService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;

        public AccountController(
            IAccountService accountService,
            IOrderService orderService,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment)
        {
            _accountService = accountService;
            _orderService = orderService;
            _signInManager = signInManager;
            _environment = environment;
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Profile));
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _accountService.RegisterAsync(
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password);

            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(model);
            }

            var signInResult = await _signInManager.PasswordSignInAsync(
                model.Email.Trim(),
                model.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Votre compte a été créé. Vous pouvez maintenant vous connecter.";
                return RedirectToAction(nameof(Login));
            }

            TempData["SuccessMessage"] = "Votre compte a été créé avec succès.";
            return RedirectToAction(nameof(Profile));
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Profile));
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email.Trim(),
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Ce compte est temporairement verrouillé. Veuillez réessayer plus tard.");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "La connexion n'est pas autorisée pour ce compte.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Adresse e-mail ou mot de passe incorrect.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _accountService.GetByIdAsync(CurrentUserId);

            if (user is null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            await PopulateOrderSummaryAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _accountService.GetByIdAsync(CurrentUserId);

            if (user is null)
            {
                return NotFound();
            }

            var previousPictureUrl = user.ProfilePictureUrl;
            model.ProfilePictureUrl = previousPictureUrl;
            model.Email = user.Email ?? string.Empty;

            if (model.ProfilePicture is { Length: > 0 })
            {
                var validationError = GetProfilePictureValidationError(model.ProfilePicture);

                if (validationError is not null)
                {
                    ModelState.AddModelError(nameof(model.ProfilePicture), validationError);
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateOrderSummaryAsync(model);
                return View(model);
            }

            string? uploadedPictureUrl = null;

            if (model.ProfilePicture is { Length: > 0 })
            {
                uploadedPictureUrl = await SaveProfilePictureAsync(model.ProfilePicture, CurrentUserId);
            }

            var result = await _accountService.UpdateProfileAsync(
                CurrentUserId,
                model.FirstName,
                model.LastName,
                model.PhoneNumber,
                uploadedPictureUrl ?? previousPictureUrl);

            if (!result.Succeeded)
            {
                DeleteProfilePicture(uploadedPictureUrl);
                AddErrors(result);
                model.ProfilePictureUrl = previousPictureUrl;
                await PopulateOrderSummaryAsync(model);
                return View(model);
            }

            if (uploadedPictureUrl is not null &&
                !string.Equals(uploadedPictureUrl, previousPictureUrl, StringComparison.OrdinalIgnoreCase))
            {
                DeleteProfilePicture(previousPictureUrl);
            }

            TempData["SuccessMessage"] = "Votre profil a été mis à jour.";
            return RedirectToAction(nameof(Profile));
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private static string? GetProfilePictureValidationError(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedProfilePictureExtensions.Contains(extension))
            {
                return "Formats acceptés : JPG, PNG, GIF ou WEBP.";
            }

            if (file.Length > MaxProfilePictureBytes)
            {
                return "L'image ne doit pas dépasser 2 Mo.";
            }

            if (!string.IsNullOrWhiteSpace(file.ContentType) &&
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return "Le fichier sélectionné n'est pas une image valide.";
            }

            return null;
        }

        private async Task<string> SaveProfilePictureAsync(IFormFile file, string userId)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{userId}-{Guid.NewGuid():N}{extension}";
            var folder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

            Directory.CreateDirectory(folder);

            var filePath = Path.Combine(folder, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            return $"/uploads/avatars/{fileName}";
        }

        private void DeleteProfilePicture(string? relativePath)
        {
            const string prefix = "/uploads/avatars/";

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
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "avatars", fileName);

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

        private async Task PopulateOrderSummaryAsync(ProfileViewModel model)
        {
            var page = await _orderService.GetOrdersAsync(CurrentUserId, 1, 3);

            model.TotalOrders = page.TotalCount;
            model.RecentOrders = page.Orders
                .Select(OrderSummaryViewModel.FromOrder)
                .ToList();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
