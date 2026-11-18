namespace TechZone.Models
{
    public class PlaceOrderRequest
    {
        public string ShippingFullName { get; set; } = string.Empty;

        public string ShippingPhone { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        public string ShippingCity { get; set; } = string.Empty;

        public string ShippingPostalCode { get; set; } = string.Empty;

        public string ShippingCountry { get; set; } = string.Empty;
    }
}
