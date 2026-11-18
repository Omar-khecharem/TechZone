namespace TechZone.ViewModels
{
    public class OrderItemSummaryViewModel
    {
        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
