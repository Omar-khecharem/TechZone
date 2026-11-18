namespace TechZone.ViewModels
{
    public class OrderConfirmationViewModel
    {
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
    }
}
