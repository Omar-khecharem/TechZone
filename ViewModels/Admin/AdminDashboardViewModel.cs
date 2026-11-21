using TechZone.Models;

namespace TechZone.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }

        public decimal RevenueThisMonth { get; set; }

        public decimal RevenueLastMonth { get; set; }

        public decimal AverageOrderValue { get; set; }

        public int TotalOrders { get; set; }

        public int OrdersThisMonth { get; set; }

        public int OrdersLastMonth { get; set; }

        public int TotalProducts { get; set; }

        public int TotalCustomers { get; set; }

        public int ActiveCustomersThisMonth { get; set; }

        public int LowStockCount { get; set; }

        public int OutOfStockCount { get; set; }

        public int PendingOrders { get; set; }

        public int PendingPayments { get; set; }

        public int DeliveredOrders { get; set; }

        public int CancelledOrders { get; set; }

        public double RevenueGrowthPercent =>
            RevenueLastMonth <= 0
                ? (RevenueThisMonth > 0 ? 100 : 0)
                : (double)((RevenueThisMonth - RevenueLastMonth) / RevenueLastMonth) * 100;

        public double OrdersGrowthPercent =>
            OrdersLastMonth <= 0
                ? (OrdersThisMonth > 0 ? 100 : 0)
                : ((double)(OrdersThisMonth - OrdersLastMonth) / OrdersLastMonth) * 100;

        public IReadOnlyList<MonthlyRevenuePoint> MonthlyRevenue { get; set; } =
            Array.Empty<MonthlyRevenuePoint>();

        public IReadOnlyList<AdminStatusBreakdown> OrdersByStatus { get; set; } =
            Array.Empty<AdminStatusBreakdown>();

        public IReadOnlyList<AdminPaymentBreakdown> PaymentsByStatus { get; set; } =
            Array.Empty<AdminPaymentBreakdown>();

        public IReadOnlyList<AdminTopProduct> TopProducts { get; set; } =
            Array.Empty<AdminTopProduct>();

        public IReadOnlyList<AdminCategorySales> SalesByCategory { get; set; } =
            Array.Empty<AdminCategorySales>();

        public IReadOnlyList<AdminOrderRow> RecentOrders { get; set; } =
            Array.Empty<AdminOrderRow>();

        public IReadOnlyList<AdminLowStockProduct> LowStockProducts { get; set; } =
            Array.Empty<AdminLowStockProduct>();

        public IReadOnlyList<AdminTopCustomer> TopCustomers { get; set; } =
            Array.Empty<AdminTopCustomer>();
    }

    public class MonthlyRevenuePoint
    {
        public string Label { get; set; } = string.Empty;

        public DateTime MonthStart { get; set; }

        public decimal Amount { get; set; }

        public bool IsCurrent { get; set; }
    }

    public class AdminStatusBreakdown
    {
        public OrderStatus Status { get; set; }

        public string Label { get; set; } = string.Empty;

        public int Count { get; set; }

        public double Percentage { get; set; }

        public string Color { get; set; } = "#2563eb";
    }

    public class AdminPaymentBreakdown
    {
        public PaymentStatus Status { get; set; }

        public string Label { get; set; } = string.Empty;

        public int Count { get; set; }

        public double Percentage { get; set; }

        public string Color { get; set; } = "#2563eb";
    }

    public class AdminTopProduct
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int UnitsSold { get; set; }

        public decimal Revenue { get; set; }

        public int Stock { get; set; }
    }

    public class AdminCategorySales
    {
        public string Name { get; set; } = string.Empty;

        public int UnitsSold { get; set; }

        public decimal Revenue { get; set; }

        public double Share { get; set; }
    }

    public class AdminOrderRow
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        public string StatusLabel { get; set; } = string.Empty;

        public string StatusBadgeClass { get; set; } = "is-neutral";

        public string PaymentStatusLabel { get; set; } = string.Empty;

        public int ItemCount { get; set; }
    }

    public class AdminLowStockProduct
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public int Stock { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
    }

    public class AdminTopCustomer
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int OrdersCount { get; set; }

        public decimal TotalSpent { get; set; }
    }
}
