using Microsoft.EntityFrameworkCore;
using TechZone.Data;
using TechZone.Interfaces;
using TechZone.Models;

namespace TechZone.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _dbContext;

        public CartService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Cart?> GetCartAsync(string userId)
        {
            return _dbContext.Carts
                .AsNoTracking()
                .Include(cart => cart.Items)
                .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(cart => cart.UserId == userId);
        }

        public async Task<AddToCartStatus> AddAsync(string userId, int productId, int quantity)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            var product = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(product => product.Id == productId);

            if (product is null)
            {
                return AddToCartStatus.ProductNotFound;
            }

            if (product.Stock <= 0)
            {
                return AddToCartStatus.OutOfStock;
            }

            var cart = await _dbContext.Carts
                .Include(cart => cart.Items)
                .FirstOrDefaultAsync(cart => cart.UserId == userId);

            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                _dbContext.Carts.Add(cart);
            }

            var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            var newQuantity = (cartItem?.Quantity ?? 0) + quantity;

            if (newQuantity > product.Stock)
            {
                return AddToCartStatus.InsufficientStock;
            }

            if (cartItem is null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }
            else
            {
                cartItem.Quantity = newQuantity;
            }

            await _dbContext.SaveChangesAsync();
            return AddToCartStatus.Added;
        }

        public async Task<CartOperationStatus> UpdateQuantityAsync(string userId, int cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                return CartOperationStatus.InvalidQuantity;
            }

            var cartItem = await _dbContext.CartItems
                .Include(item => item.Cart)
                .Include(item => item.Product)
                .FirstOrDefaultAsync(item => item.Id == cartItemId && item.Cart.UserId == userId);

            if (cartItem is null)
            {
                return CartOperationStatus.ItemNotFound;
            }

            if (quantity > cartItem.Product.Stock)
            {
                return CartOperationStatus.InsufficientStock;
            }

            cartItem.Quantity = quantity;
            await _dbContext.SaveChangesAsync();

            return CartOperationStatus.Success;
        }

        public async Task<CartOperationStatus> RemoveItemAsync(string userId, int cartItemId)
        {
            var cartItem = await _dbContext.CartItems
                .Include(item => item.Cart)
                .FirstOrDefaultAsync(item => item.Id == cartItemId && item.Cart.UserId == userId);

            if (cartItem is null)
            {
                return CartOperationStatus.ItemNotFound;
            }

            _dbContext.CartItems.Remove(cartItem);
            await _dbContext.SaveChangesAsync();

            return CartOperationStatus.Success;
        }
    }
}
