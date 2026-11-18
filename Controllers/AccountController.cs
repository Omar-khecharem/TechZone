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
        private readonly IAccountService _accountService;
        private readonly IOrderService _orderService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            IAccountService accountService,
            IOrderService orderService,
            SignInManager<ApplicationUser> signInManager)
        {
            _accountService = accountService;
            _orderService = orderService;
            _signInManager = signInManager;
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
                PhoneNumber = user.PhoneNumber
            };

            await PopulateOrderSummaryAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RestoreEmailAsync(model);
                await PopulateOrderSummaryAsync(model);
                return View(model);
            }

            var result = await _accountService.UpdateProfileAsync(
                CurrentUserId,
                model.FirstName,
                model.LastName,
                model.PhoneNumber);

            if (!result.Succeeded)
            {
                AddErrors(result);
                await RestoreEmailAsync(model);
                await PopulateOrderSummaryAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Votre profil a été mis à jour.";
            return RedirectToAction(nameof(Profile));
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private async Task RestoreEmailAsync(ProfileViewModel model)
        {
            var user = await _accountService.GetByIdAsync(CurrentUserId);
            model.Email = user?.Email ?? string.Empty;
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
