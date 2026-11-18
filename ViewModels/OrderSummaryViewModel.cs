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

        public static OrderSummaryViewModel FromOrder(Order order)
        {
            return new OrderSummaryViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                StatusLabel = OrderLabels.GetStatusLabel(order.Status)
            };
        }
    }
}
