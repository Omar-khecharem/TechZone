using TechZone.Models;

namespace TechZone.ViewModels
{
    public class OrderSummaryViewModel
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string StatusLabel { get; set; } = string.Empty;

        public string StatusBadgeClass { get; set; } = "is-neutral";

        public static OrderSummaryViewModel FromOrder(Order order)
        {
            return new OrderSummaryViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                StatusLabel = OrderLabels.GetStatusLabel(order.Status),
                StatusBadgeClass = GetStatusBadgeClass(order.Status)
            };
        }

        private static string GetStatusBadgeClass(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Confirmed => "is-confirmed",
                OrderStatus.Shipped => "is-shipped",
                OrderStatus.Delivered => "is-delivered",
                OrderStatus.Cancelled => "is-cancelled",
                _ => "is-neutral"
            };
        }
    }
}
