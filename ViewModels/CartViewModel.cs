using TechZone.Models;

namespace TechZone.ViewModels
{
    public class CartViewModel
    {
        public IReadOnlyList<CartItemViewModel> Items { get; set; } = Array.Empty<CartItemViewModel>();

        public decimal Subtotal => Items.Sum(item => item.LineTotal);

        public decimal Total => Subtotal;

        public int TotalQuantity => Items.Sum(item => item.Quantity);

        public bool IsEmpty => Items.Count == 0;

        public static CartViewModel FromCart(Cart? cart)
        {
            var items = cart?.Items
                .OrderBy(item => item.Id)
                .Select(item => new CartItemViewModel
                {
                    CartItemId = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ImageUrl = item.Product.ImageUrl,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity,
                    Stock = item.Product.Stock
                })
                .ToList() ?? new List<CartItemViewModel>();

            return new CartViewModel { Items = items };
        }
    }
}
