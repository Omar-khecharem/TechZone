using TechZone.Models;

namespace TechZone.Interfaces
{
    public interface ICartService
    {
        Task<Cart?> GetCartAsync(string userId);

        Task<AddToCartStatus> AddAsync(string userId, int productId, int quantity);

        Task<CartOperationStatus> UpdateQuantityAsync(string userId, int cartItemId, int quantity);

        Task<CartOperationStatus> RemoveItemAsync(string userId, int cartItemId);
    }
}
