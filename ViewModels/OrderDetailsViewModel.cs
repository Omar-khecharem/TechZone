using TechZone.Models;

namespace TechZone.ViewModels
{
    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string StatusLabel { get; set; } = string.Empty;

        public string PaymentMethodLabel { get; set; } = string.Empty;

        public string PaymentStatusLabel { get; set; } = string.Empty;

        public string ShippingFullName { get; set; } = string.Empty;

        public string ShippingPhone { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        public string ShippingCity { get; set; } = string.Empty;

        public string ShippingPostalCode { get; set; } = string.Empty;

        public string ShippingCountry { get; set; } = string.Empty;

        public IReadOnlyList<OrderItemSummaryViewModel> Items { get; set; } =
            Array.Empty<OrderItemSummaryViewModel>();

        public static OrderDetailsViewModel FromOrder(Order order)
        {
            return new OrderDetailsViewModel
            {
                OrderId = order.Id,
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
        }
    }
}
