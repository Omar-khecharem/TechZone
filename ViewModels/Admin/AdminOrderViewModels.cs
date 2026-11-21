using TechZone.Models;

namespace TechZone.ViewModels.Admin
{
    public class AdminOrderListViewModel
    {
        public string? Search { get; set; }

        public OrderStatus? Status { get; set; }

        public IReadOnlyList<StatusOption> StatusOptions { get; set; } = Array.Empty<StatusOption>();

        public PagedResult<AdminOrderRow> Orders { get; set; } = new();

        public int TotalCount => Orders.TotalCount;
    }

    public class StatusOption
    {
        public OrderStatus Status { get; set; }

        public string Label { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class AdminOrderDetailsViewModel
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        public string StatusLabel { get; set; } = string.Empty;

        public string StatusBadgeClass { get; set; } = "is-neutral";

        public PaymentMethod PaymentMethod { get; set; }

        public string PaymentMethodLabel { get; set; } = string.Empty;

        public PaymentStatus PaymentStatus { get; set; }

        public string PaymentStatusLabel { get; set; } = string.Empty;

        public string CustomerId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string ShippingFullName { get; set; } = string.Empty;

        public string ShippingPhone { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        public string ShippingCity { get; set; } = string.Empty;

        public string ShippingPostalCode { get; set; } = string.Empty;

        public string ShippingCountry { get; set; } = string.Empty;

        public IReadOnlyList<AdminOrderItemRow> Items { get; set; } = Array.Empty<AdminOrderItemRow>();
    }

    public class AdminOrderItemRow
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
