using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels;

namespace TechZone.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IAccountService _accountService;

        public OrderController(
            ICartService cartService,
            IOrderService orderService,
            IAccountService accountService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            var cart = await _cartService.GetCartAsync(userId);
            var cartViewModel = CartViewModel.FromCart(cart);

            if (cartViewModel.IsEmpty)
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _accountService.GetByIdAsync(userId);
            var model = new CheckoutViewModel
            {
                FullName = user is null
                    ? string.Empty
                    : $"{user.FirstName} {user.LastName}".Trim(),
                Phone = user?.PhoneNumber ?? string.Empty,
                Cart = cartViewModel
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            var cart = await _cartService.GetCartAsync(userId);
            var cartViewModel = CartViewModel.FromCart(cart);

            if (cartViewModel.IsEmpty)
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction("Index", "Cart");
            }

            model.Cart = cartViewModel;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new PlaceOrderRequest
            {
                ShippingFullName = model.FullName,
                ShippingPhone = model.Phone,
                ShippingAddress = model.Address,
                ShippingCity = model.City,
                ShippingPostalCode = model.PostalCode,
                ShippingCountry = model.Country
            };

            var result = await _orderService.PlaceOrderAsync(userId, request);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Checkout));
            }

            return RedirectToAction(nameof(Confirmation), new { id = result.OrderId });
        }

        [HttpGet]
        public async Task<IActionResult> History(int page = 1)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            const int pageSize = 10;
            var history = await _orderService.GetOrdersAsync(userId, page, pageSize);

            var model = new OrderHistoryViewModel
            {
                Orders = history.Orders
                    .Select(OrderSummaryViewModel.FromOrder)
                    .ToList(),
                Page = history.Page,
                TotalPages = history.TotalPages,
                TotalCount = history.TotalCount
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            var order = await _orderService.GetOrderAsync(userId, id);

            if (order is null)
            {
                return NotFound();
            }

            var model = new OrderConfirmationViewModel
            {
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                StatusLabel = OrderLabels.GetStatusLabel(order.Status),
                PaymentMethodLabel = OrderLabels.GetPaymentMethodLabel(order.PaymentMethod),
                PaymentStatusLabel = OrderLabels.GetPaymentStatusLabel(order.PaymentStatus),
                ShippingFullName = order.ShippingFullName,
                ShippingPhone = order.ShippingPhone,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingCountry = order.ShippingCountry,
                Items = order.Items
                    .OrderBy(item => item.Id)
                    .Select(item => new OrderItemSummaryViewModel
                    {
                        ProductName = item.ProductName,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Challenge();
            }

            var order = await _orderService.GetOrderAsync(userId, id);

            if (order is null)
            {
                return NotFound();
            }

            return View(OrderDetailsViewModel.FromOrder(order));
        }

        private bool TryGetCurrentUserId(out string userId)
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return userId.Length > 0;
        }
    }
}
