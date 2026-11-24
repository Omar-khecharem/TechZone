using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TechZone.Data;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels;
using TechZone.ViewModels.Admin;

namespace TechZone.Services
{
    public class AdminService : IAdminService
    {
        private static readonly OrderStatus[] StatusOrder =
        {
            OrderStatus.Confirmed,
            OrderStatus.Shipped,
            OrderStatus.Delivered,
            OrderStatus.Cancelled
        };

        private static readonly Dictionary<OrderStatus, string> StatusColors = new()
        {
            [OrderStatus.Confirmed] = "#2563eb",
            [OrderStatus.Shipped] = "#f59e0b",
            [OrderStatus.Delivered] = "#16a34a",
            [OrderStatus.Cancelled] = "#ef4444"
        };

        private static readonly Dictionary<PaymentStatus, string> PaymentColors = new()
        {
            [PaymentStatus.Pending] = "#f59e0b",
            [PaymentStatus.Paid] = "#16a34a",
            [PaymentStatus.Failed] = "#ef4444"
        };

        private readonly ApplicationDbContext _dbContext;

        public AdminService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = monthStart.AddMonths(-1);
            var chartStart = monthStart.AddMonths(-11);

            var orders = _dbContext.Orders.AsNoTracking();
            var activeOrders = orders.Where(order => order.Status != OrderStatus.Cancelled);

            var totalRevenue = await activeOrders.SumAsync(order => (decimal?)order.TotalAmount) ?? 0m;
            var activeOrderCount = await activeOrders.CountAsync();
            var totalOrders = await orders.CountAsync();
            var totalProducts = await _dbContext.Products.CountAsync();
            var totalCustomers = await _dbContext.Users.CountAsync();

            var revenueThisMonth = await activeOrders
                .Where(order => order.OrderDate >= monthStart)
                .SumAsync(order => (decimal?)order.TotalAmount) ?? 0m;
            var revenueLastMonth = await activeOrders
                .Where(order => order.OrderDate >= lastMonthStart && order.OrderDate < monthStart)
                .SumAsync(order => (decimal?)order.TotalAmount) ?? 0m;

            var ordersThisMonth = await orders.CountAsync(order => order.OrderDate >= monthStart);
            var ordersLastMonth = await orders.CountAsync(order =>
                order.OrderDate >= lastMonthStart && order.OrderDate < monthStart);

            var activeCustomersThisMonth = await orders
                .Where(order => order.OrderDate >= monthStart)
                .Select(order => order.UserId)
                .Distinct()
                .CountAsync();

            var monthlyRaw = await activeOrders
                .Where(order => order.OrderDate >= chartStart)
                .GroupBy(order => new { order.OrderDate.Year, order.OrderDate.Month })
                .Select(group => new
                {
                    group.Key.Year,
                    group.Key.Month,
                    Amount = group.Sum(order => order.TotalAmount)
                })
                .ToListAsync();

            var french = CultureInfo.GetCultureInfo("fr-FR");
            var monthlyRevenue = new List<MonthlyRevenuePoint>();

            for (var index = 0; index < 12; index++)
            {
                var month = chartStart.AddMonths(index);
                var match = monthlyRaw.FirstOrDefault(item =>
                    item.Year == month.Year && item.Month == month.Month);

                monthlyRevenue.Add(new MonthlyRevenuePoint
                {
                    Label = month.ToString("MMM yy", french),
                    MonthStart = month,
                    Amount = match?.Amount ?? 0m,
                    IsCurrent = month.Year == now.Year && month.Month == now.Month
                });
            }

            var statusCounts = await orders
                .GroupBy(order => order.Status)
                .Select(group => new { Status = group.Key, Count = group.Count() })
                .ToListAsync();

            var ordersByStatus = StatusOrder
                .Select(status =>
                {
                    var count = statusCounts.FirstOrDefault(item => item.Status == status)?.Count ?? 0;

                    return new AdminStatusBreakdown
                    {
                        Status = status,
                        Label = OrderLabels.GetStatusLabel(status),
                        Count = count,
                        Percentage = totalOrders == 0 ? 0 : count * 100d / totalOrders,
                        Color = StatusColors[status]
                    };
                })
                .ToList();

            var paymentCounts = await orders
                .GroupBy(order => order.PaymentStatus)
                .Select(group => new { Status = group.Key, Count = group.Count() })
                .ToListAsync();

            var paymentsByStatus = paymentCounts
                .Select(item => new AdminPaymentBreakdown
                {
                    Status = item.Status,
                    Label = OrderLabels.GetPaymentStatusLabel(item.Status),
                    Count = item.Count,
                    Percentage = totalOrders == 0 ? 0 : item.Count * 100d / totalOrders,
                    Color = PaymentColors.TryGetValue(item.Status, out var color) ? color : "#94a3b8"
                })
                .OrderByDescending(item => item.Count)
                .ToList();

            var topProductData = await (
                    from item in _dbContext.OrderItems.AsNoTracking()
                    join order in _dbContext.Orders.AsNoTracking() on item.OrderId equals order.Id
                    where order.Status != OrderStatus.Cancelled
                    group item by item.ProductId
                    into grouped
                    select new
                    {
                        ProductId = grouped.Key,
                        Name = grouped.Max(item => item.ProductName),
                        UnitsSold = grouped.Sum(item => item.Quantity),
                        Revenue = grouped.Sum(item => item.UnitPrice * item.Quantity)
                    })
                .OrderByDescending(item => item.Revenue)
                .Take(6)
                .ToListAsync();

            var topProductIds = topProductData.Select(item => item.ProductId).ToList();
            var topProductStocks = await _dbContext.Products
                .AsNoTracking()
                .Where(product => topProductIds.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id, product => product.Stock);

            var topProducts = topProductData
                .Select(item => new AdminTopProduct
                {
                    ProductId = item.ProductId,
                    Name = item.Name,
                    UnitsSold = item.UnitsSold,
                    Revenue = item.Revenue,
                    Stock = topProductStocks.TryGetValue(item.ProductId, out var stock) ? stock : 0
                })
                .ToList();

            var categorySalesData = await (
                    from item in _dbContext.OrderItems.AsNoTracking()
                    join order in _dbContext.Orders.AsNoTracking() on item.OrderId equals order.Id
                    join product in _dbContext.Products.AsNoTracking() on item.ProductId equals product.Id
                    join category in _dbContext.Categories.AsNoTracking() on product.CategoryId equals category.Id
                    where order.Status != OrderStatus.Cancelled
                    group item by category.Name
                    into grouped
                    select new
                    {
                        Name = grouped.Key,
                        UnitsSold = grouped.Sum(item => item.Quantity),
                        Revenue = grouped.Sum(item => item.UnitPrice * item.Quantity)
                    })
                .OrderByDescending(item => item.Revenue)
                .ToListAsync();

            var categoryRevenueTotal = categorySalesData.Sum(item => item.Revenue);

            var salesByCategory = categorySalesData
                .Select(item => new AdminCategorySales
                {
                    Name = item.Name,
                    UnitsSold = item.UnitsSold,
                    Revenue = item.Revenue,
                    Share = categoryRevenueTotal == 0
                        ? 0
                        : (double)(item.Revenue / categoryRevenueTotal) * 100
                })
                .ToList();

            var recentOrderEntities = await orders
                .Include(order => order.User)
                .Include(order => order.Items)
                .OrderByDescending(order => order.OrderDate)
                .ThenByDescending(order => order.Id)
                .Take(6)
                .ToListAsync();

            var recentOrders = recentOrderEntities.Select(MapOrderRow).ToList();

            var lowStockEntities = await _dbContext.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .Where(product => product.Stock <= 5)
                .OrderBy(product => product.Stock)
                .ThenBy(product => product.Name)
                .Take(6)
                .ToListAsync();

            var lowStockProducts = lowStockEntities
                .Select(product => new AdminLowStockProduct
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryName = product.Category.Name,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl
                })
                .ToList();

            var topCustomerData = await _dbContext.Users
                .AsNoTracking()
                .Select(user => new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    OrdersCount = user.Orders.Count(),
                    TotalSpent = user.Orders
                        .Where(order => order.Status != OrderStatus.Cancelled)
                        .Sum(order => (decimal?)order.TotalAmount) ?? 0m
                })
                .Where(item => item.OrdersCount > 0)
                .OrderByDescending(item => item.TotalSpent)
                .Take(5)
                .ToListAsync();

            var topCustomers = topCustomerData
                .Select(item => new AdminTopCustomer
                {
                    Id = item.Id,
                    FullName = $"{item.FirstName} {item.LastName}".Trim(),
                    Email = item.Email ?? string.Empty,
                    OrdersCount = item.OrdersCount,
                    TotalSpent = item.TotalSpent
                })
                .ToList();

            return new AdminDashboardViewModel
            {
                TotalRevenue = totalRevenue,
                RevenueThisMonth = revenueThisMonth,
                RevenueLastMonth = revenueLastMonth,
                AverageOrderValue = activeOrderCount == 0 ? 0 : totalRevenue / activeOrderCount,
                TotalOrders = totalOrders,
                OrdersThisMonth = ordersThisMonth,
                OrdersLastMonth = ordersLastMonth,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                ActiveCustomersThisMonth = activeCustomersThisMonth,
                LowStockCount = await _dbContext.Products.CountAsync(product => product.Stock > 0 && product.Stock <= 5),
                OutOfStockCount = await _dbContext.Products.CountAsync(product => product.Stock == 0),
                PendingOrders = statusCounts.FirstOrDefault(item => item.Status == OrderStatus.Confirmed)?.Count ?? 0,
                PendingPayments = paymentCounts.FirstOrDefault(item => item.Status == PaymentStatus.Pending)?.Count ?? 0,
                DeliveredOrders = statusCounts.FirstOrDefault(item => item.Status == OrderStatus.Delivered)?.Count ?? 0,
                CancelledOrders = statusCounts.FirstOrDefault(item => item.Status == OrderStatus.Cancelled)?.Count ?? 0,
                MonthlyRevenue = monthlyRevenue,
                OrdersByStatus = ordersByStatus,
                PaymentsByStatus = paymentsByStatus,
                TopProducts = topProducts,
                SalesByCategory = salesByCategory,
                RecentOrders = recentOrders,
                LowStockProducts = lowStockProducts,
                TopCustomers = topCustomers
            };
        }

        public async Task<AdminProductListViewModel> GetProductsAsync(
            string? search,
            int? categoryId,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _dbContext.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(product => product.Name.Contains(term));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(product => product.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var products = await query
                .OrderByDescending(product => product.CreatedAt)
                .ThenBy(product => product.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new AdminProductListViewModel
            {
                Search = search,
                CategoryId = categoryId,
                Categories = await GetCategoriesAsync(),
                Products = new PagedResult<AdminProductRow>
                {
                    Items = products
                        .Select(product => new AdminProductRow
                        {
                            Id = product.Id,
                            Name = product.Name,
                            ImageUrl = product.ImageUrl,
                            CategoryName = product.Category.Name,
                            Price = product.Price,
                            Stock = product.Stock,
                            SalesCount = product.SalesCount,
                            CreatedAt = product.CreatedAt
                        })
                        .ToList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                }
            };
        }

        public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .ToListAsync();
        }

        public Task<Product?> GetProductAsync(int id)
        {
            return _dbContext.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .FirstOrDefaultAsync(product => product.Id == id);
        }

        public async Task<AdminOperationResult> CreateProductAsync(AdminProductFormViewModel model)
        {
            if (!await _dbContext.Categories.AnyAsync(category => category.Id == model.CategoryId))
            {
                return AdminOperationResult.Failure("La catégorie sélectionnée n'existe pas.");
            }

            var product = new Product
            {
                Name = model.Name.Trim(),
                Description = model.Description.Trim(),
                Price = model.Price,
                Stock = model.Stock,
                CategoryId = model.CategoryId,
                ImageUrl = model.ImageUrl?.Trim() ?? string.Empty,
                SalesCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            return AdminOperationResult.Success();
        }

        public async Task<AdminOperationResult> UpdateProductAsync(AdminProductFormViewModel model)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == model.Id);

            if (product is null)
            {
                return AdminOperationResult.Failure("Produit introuvable.");
            }

            if (!await _dbContext.Categories.AnyAsync(category => category.Id == model.CategoryId))
            {
                return AdminOperationResult.Failure("La catégorie sélectionnée n'existe pas.");
            }

            product.Name = model.Name.Trim();
            product.Description = model.Description.Trim();
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.CategoryId = model.CategoryId;
            product.ImageUrl = model.ImageUrl?.Trim() ?? product.ImageUrl;

            await _dbContext.SaveChangesAsync();

            return AdminOperationResult.Success();
        }

        public async Task<AdminOperationResult> DeleteProductAsync(int id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == id);

            if (product is null)
            {
                return AdminOperationResult.Failure("Produit introuvable.");
            }

            if (await _dbContext.OrderItems.AnyAsync(item => item.ProductId == id))
            {
                return AdminOperationResult.Failure(
                    "Ce produit figure dans des commandes et ne peut pas être supprimé. Modifiez plutôt son stock.");
            }

            var cartItems = await _dbContext.CartItems
                .Where(item => item.ProductId == id)
                .ToListAsync();

            if (cartItems.Count > 0)
            {
                _dbContext.CartItems.RemoveRange(cartItems);
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            return AdminOperationResult.Success();
        }

        public async Task<AdminCategoryListViewModel> GetCategoryListAsync()
        {
            var categories = await _dbContext.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .Select(category => new AdminCategoryRow
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    ProductCount = category.Products.Count
                })
                .ToListAsync();

            return new AdminCategoryListViewModel { Categories = categories };
        }

        public Task<Category?> GetCategoryAsync(int id)
        {
            return _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == id);
        }

        public async Task<AdminOperationResult> CreateCategoryAsync(AdminCategoryFormViewModel model)
        {
            var name = model.Name.Trim();

            if (await _dbContext.Categories.AnyAsync(category => category.Name == name))
            {
                return AdminOperationResult.Failure("Une catégorie porte déjà ce nom.");
            }

            _dbContext.Categories.Add(new Category
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim()
            });

            await _dbContext.SaveChangesAsync();

            return AdminOperationResult.Success();
        }

        public async Task<AdminOperationResult> UpdateCategoryAsync(AdminCategoryFormViewModel model)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(item => item.Id == model.Id);

            if (category is null)
            {
                return AdminOperationResult.Failure("Catégorie introuvable.");
            }

            var name = model.Name.Trim();

            if (await _dbContext.Categories.AnyAsync(item => item.Id != model.Id && item.Name == name))
            {
                return AdminOperationResult.Failure("Une catégorie porte déjà ce nom.");
            }

            category.Name = name;
            category.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            category.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();

            await _dbContext.SaveChangesAsync();

            return AdminOperationResult.Success();
        }

        public async Task<AdminOperationResult> DeleteCategoryAsync(int id)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(item => item.Id == id);

            if (category is null)
            {
                return AdminOperationResult.Failure("Catégorie introuvable.");
            }

            if (await _dbContext.Products.AnyAsync(product => product.CategoryId == id))
            {
                return AdminOperationResult.Failure(
                    "Cette catégorie contient encore des produits. Déplacez ou supprimez ces produits d'abord.");
            }

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();

            return AdminOperationResult.Success();
        }

        public async Task<AdminOrderListViewModel> GetOrdersAsync(
            string? search,
            OrderStatus? status,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _dbContext.Orders
                .AsNoTracking()
                .Include(order => order.User)
                .Include(order => order.Items)
                .AsQueryable();

            if (status.HasValue && Enum.IsDefined(status.Value))
            {
                query = query.Where(order => order.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(order =>
                    order.OrderNumber.Contains(term) ||
                    order.User.FirstName.Contains(term) ||
                    order.User.LastName.Contains(term) ||
                    (order.User.Email != null && order.User.Email.Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var orders = await query
                .OrderByDescending(order => order.OrderDate)
                .ThenByDescending(order => order.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var statusCounts = await _dbContext.Orders
                .AsNoTracking()
                .GroupBy(order => order.Status)
                .Select(group => new { Status = group.Key, Count = group.Count() })
                .ToListAsync();

            return new AdminOrderListViewModel
            {
                Search = search,
                Status = status,
                StatusOptions = StatusOrder
                    .Select(item => new StatusOption
                    {
                        Status = item,
                        Label = OrderLabels.GetStatusLabel(item),
                        Count = statusCounts.FirstOrDefault(group => group.Status == item)?.Count ?? 0
                    })
                    .ToList(),
                Orders = new PagedResult<AdminOrderRow>
                {
                    Items = orders.Select(MapOrderRow).ToList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                }
            };
        }

        public async Task<AdminOrderDetailsViewModel?> GetOrderAsync(int id)
        {
            var order = await _dbContext.Orders
                .AsNoTracking()
                .Include(item => item.User)
                .Include(item => item.Items)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (order is null)
            {
                return null;
            }

            return new AdminOrderDetailsViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                StatusLabel = OrderLabels.GetStatusLabel(order.Status),
                StatusBadgeClass = GetStatusBadgeClass(order.Status),
                PaymentMethod = order.PaymentMethod,
                PaymentMethodLabel = OrderLabels.GetPaymentMethodLabel(order.PaymentMethod),
                PaymentStatus = order.PaymentStatus,
                PaymentStatusLabel = OrderLabels.GetPaymentStatusLabel(order.PaymentStatus),
                CustomerId = order.UserId,
                CustomerName = $"{order.User.FirstName} {order.User.LastName}".Trim(),
                CustomerEmail = order.User.Email ?? string.Empty,
                CustomerPhone = order.User.PhoneNumber ?? string.Empty,
                ShippingFullName = order.ShippingFullName,
                ShippingPhone = order.ShippingPhone,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingPostalCode = order.ShippingPostalCode,
                ShippingCountry = order.ShippingCountry,
                Items = order.Items
                    .OrderBy(item => item.Id)
                    .Select(item => new AdminOrderItemRow
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity
                    })
                    .ToList()
            };
        }

        public async Task<AdminOperationResult> UpdateOrderAsync(
            int id,
            OrderStatus status,
            PaymentStatus paymentStatus)
        {
            if (!Enum.IsDefined(status))
            {
                return AdminOperationResult.Failure("Statut de commande invalide.");
            }

            if (!Enum.IsDefined(paymentStatus))
            {
                return AdminOperationResult.Failure("Statut de paiement invalide.");
            }

            var order = await _dbContext.Orders.FirstOrDefaultAsync(item => item.Id == id);

            if (order is null)
            {
                return AdminOperationResult.Failure("Commande introuvable.");
            }

            order.Status = status;
            order.PaymentStatus = paymentStatus;

            await _dbContext.SaveChangesAsync();

            return AdminOperationResult.Success();
        }

        public async Task<AdminCustomerListViewModel> GetCustomersAsync(
            string? search,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _dbContext.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(user =>
                    user.FirstName.Contains(term) ||
                    user.LastName.Contains(term) ||
                    (user.Email != null && user.Email.Contains(term)) ||
                    (user.PhoneNumber != null && user.PhoneNumber.Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var customers = await query
                .OrderBy(user => user.LastName)
                .ThenBy(user => user.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(user => new AdminCustomerRow
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    Phone = user.PhoneNumber,
                    OrdersCount = user.Orders.Count(),
                    TotalSpent = user.Orders
                        .Where(order => order.Status != OrderStatus.Cancelled)
                        .Sum(order => (decimal?)order.TotalAmount) ?? 0m,
                    LastOrderDate = user.Orders.Max(order => (DateTime?)order.OrderDate)
                })
                .ToListAsync();

            return new AdminCustomerListViewModel
            {
                Search = search,
                Customers = new PagedResult<AdminCustomerRow>
                {
                    Items = customers,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                }
            };
        }

        public async Task<AdminCustomerDetailsViewModel?> GetCustomerAsync(string id)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (user is null)
            {
                return null;
            }

            var orders = await _dbContext.Orders
                .AsNoTracking()
                .Include(order => order.User)
                .Include(order => order.Items)
                .Where(order => order.UserId == id)
                .OrderByDescending(order => order.OrderDate)
                .ThenByDescending(order => order.Id)
                .ToListAsync();

            var orderRows = orders.Select(MapOrderRow).ToList();

            return new AdminCustomerDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                OrdersCount = orderRows.Count,
                TotalSpent = orders
                    .Where(order => order.Status != OrderStatus.Cancelled)
                    .Sum(order => order.TotalAmount),
                LastOrderDate = orderRows.Count == 0 ? null : orderRows.Max(order => order.OrderDate),
                Orders = orderRows
            };
        }

        private static AdminOrderRow MapOrderRow(Order order)
        {
            return new AdminOrderRow
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = $"{order.User.FirstName} {order.User.LastName}".Trim(),
                CustomerEmail = order.User.Email ?? string.Empty,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                StatusLabel = OrderLabels.GetStatusLabel(order.Status),
                StatusBadgeClass = GetStatusBadgeClass(order.Status),
                PaymentStatusLabel = OrderLabels.GetPaymentStatusLabel(order.PaymentStatus),
                ItemCount = order.Items.Sum(item => item.Quantity)
            };
        }

        private static string GetStatusBadgeClass(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Confirmed => "is-confirmed",
                OrderStatus.Shipped => "is-shipped",
                OrderStatus.Delivered => "is-delivered",
                OrderStatus.Cancelled => "is-cancelled",
                _ => "is-neutral"
            };
        }
    }
}
