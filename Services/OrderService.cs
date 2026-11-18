using System.Data;
using Microsoft.EntityFrameworkCore;
using TechZone.Data;
using TechZone.Interfaces;
using TechZone.Models;

namespace TechZone.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OrderPlacementResult> PlaceOrderAsync(string userId, PlaceOrderRequest request)
        {
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var cart = await _dbContext.Carts
                    .Include(cart => cart.Items)
                    .ThenInclude(item => item.Product)
                    .FirstOrDefaultAsync(cart => cart.UserId == userId);

                if (cart is null || cart.Items.Count == 0)
                {
                    await transaction.RollbackAsync();
                    return OrderPlacementResult.Failure("Votre panier est vide.");
                }

                foreach (var item in cart.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        await transaction.RollbackAsync();
                        return OrderPlacementResult.Failure(
                            "Le panier contient une quantité invalide.");
                    }

                    if (item.Quantity > item.Product.Stock)
                    {
                        await transaction.RollbackAsync();
                        return OrderPlacementResult.Failure(
                            $"Stock insuffisant pour « {item.Product.Name} ». " +
                            $"Stock disponible : {item.Product.Stock}.");
                    }
                }

                var orderDate = DateTime.UtcNow;
                var totalAmount = cart.Items.Sum(item => item.Product.Price * item.Quantity);

                var order = new Order
                {
                    OrderNumber = $"TEMP-{Guid.NewGuid():N}"[..29],
                    UserId = userId,
                    OrderDate = orderDate,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Confirmed,
                    PaymentMethod = PaymentMethod.CashOnDelivery,
                    PaymentStatus = PaymentStatus.Pending,
                    ShippingFullName = request.ShippingFullName.Trim(),
                    ShippingPhone = request.ShippingPhone.Trim(),
                    ShippingAddress = request.ShippingAddress.Trim(),
                    ShippingCity = request.ShippingCity.Trim(),
                    ShippingPostalCode = request.ShippingPostalCode.Trim(),
                    ShippingCountry = request.ShippingCountry.Trim()
                };

                foreach (var item in cart.Items)
                {
                    order.Items.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        UnitPrice = item.Product.Price,
                        Quantity = item.Quantity
                    });
                }

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                order.OrderNumber = $"TZ-{orderDate:yyyyMMdd}-{order.Id:D5}";

                foreach (var item in cart.Items)
                {
                    item.Product.Stock -= item.Quantity;
                    item.Product.SalesCount += item.Quantity;
                }

                _dbContext.CartItems.RemoveRange(cart.Items);
                _dbContext.Carts.Remove(cart);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return OrderPlacementResult.Success(order.Id, order.OrderNumber);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderHistoryPage> GetOrdersAsync(string userId, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _dbContext.Orders
                .AsNoTracking()
                .Where(order => order.UserId == userId);

            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var orders = totalCount == 0
                ? new List<Order>()
                : await query
                    .OrderByDescending(order => order.OrderDate)
                    .ThenByDescending(order => order.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            return new OrderHistoryPage
            {
                Orders = orders,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public Task<Order?> GetOrderAsync(string userId, int orderId)
        {
            return _dbContext.Orders
                .AsNoTracking()
                .Include(order => order.Items)
                .FirstOrDefaultAsync(order => order.Id == orderId && order.UserId == userId);
        }
    }
}
