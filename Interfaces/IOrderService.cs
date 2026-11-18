using TechZone.Models;

namespace TechZone.Interfaces
{
    public interface IOrderService
    {
        Task<OrderPlacementResult> PlaceOrderAsync(string userId, PlaceOrderRequest request);

        Task<OrderHistoryPage> GetOrdersAsync(string userId, int page, int pageSize);

        Task<Order?> GetOrderAsync(string userId, int orderId);
    }
}
