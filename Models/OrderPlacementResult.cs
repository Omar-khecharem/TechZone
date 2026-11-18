namespace TechZone.Models
{
    public class OrderPlacementResult
    {
        public bool Succeeded { get; init; }

        public int OrderId { get; init; }

        public string? OrderNumber { get; init; }

        public string? ErrorMessage { get; init; }

        public static OrderPlacementResult Success(int orderId, string orderNumber)
        {
            return new OrderPlacementResult
            {
                Succeeded = true,
                OrderId = orderId,
                OrderNumber = orderNumber
            };
        }

        public static OrderPlacementResult Failure(string errorMessage)
        {
            return new OrderPlacementResult
            {
                ErrorMessage = errorMessage
            };
        }
    }
}
