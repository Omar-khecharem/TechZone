using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels;

namespace TechZone.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            var cart = await _cartService.GetCartAsync(userId);

            return View(CartViewModel.FromCart(cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, string? returnUrl = null)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            var result = await _cartService.AddAsync(userId, productId, 1);

            TempData[result == AddToCartStatus.Added ? "SuccessMessage" : "ErrorMessage"] =
                GetAddMessage(result);

            if (result == AddToCartStatus.ProductNotFound)
            {
                return RedirectToAction("Index", "Product");
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "Product", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            var result = await _cartService.UpdateQuantityAsync(userId, cartItemId, quantity);

            TempData[result == CartOperationStatus.Success ? "SuccessMessage" : "ErrorMessage"] =
                GetUpdateMessage(result);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            var result = await _cartService.RemoveItemAsync(userId, cartItemId);

            TempData[result == CartOperationStatus.Success ? "SuccessMessage" : "ErrorMessage"] =
                GetRemoveMessage(result);

            return RedirectToAction(nameof(Index));
        }

        private bool TryGetCurrentUserId(out string userId)
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return userId.Length > 0;
        }

        private static string GetAddMessage(AddToCartStatus status)
        {
            return status switch
            {
                AddToCartStatus.Added => "Le produit a été ajouté au panier.",
                AddToCartStatus.OutOfStock => "Ce produit est en rupture de stock.",
                AddToCartStatus.InsufficientStock => "La quantité demandée dépasse le stock disponible.",
                _ => "Le produit est introuvable."
            };
        }

        private static string GetUpdateMessage(CartOperationStatus status)
        {
            return status switch
            {
                CartOperationStatus.Success => "La quantité a été mise à jour.",
                CartOperationStatus.ItemNotFound => "Cet article n'existe pas dans votre panier.",
                CartOperationStatus.InvalidQuantity => "La quantité doit être supérieure à zéro.",
                CartOperationStatus.InsufficientStock => "La quantité demandée dépasse le stock disponible.",
                _ => "Impossible de mettre à jour la quantité."
            };
        }

        private static string GetRemoveMessage(CartOperationStatus status)
        {
            return status switch
            {
                CartOperationStatus.Success => "L'article a été supprimé du panier.",
                CartOperationStatus.ItemNotFound => "Cet article n'existe pas dans votre panier.",
                _ => "Impossible de supprimer cet article."
            };
        }
    }
}
