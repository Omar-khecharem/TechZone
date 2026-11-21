using TechZone.Models;

namespace TechZone.ViewModels.Admin
{
    public class AdminCustomerRow
    {
        public string Id { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}".Trim();

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public int OrdersCount { get; set; }

        public decimal TotalSpent { get; set; }

        public DateTime? LastOrderDate { get; set; }
    }

    public class AdminCustomerListViewModel
    {
        public string? Search { get; set; }

        public PagedResult<AdminCustomerRow> Customers { get; set; } = new();

        public int TotalCount => Customers.TotalCount;
    }

    public class AdminCustomerDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}".Trim();

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public int OrdersCount { get; set; }

        public decimal TotalSpent { get; set; }

        public DateTime? LastOrderDate { get; set; }

        public IReadOnlyList<AdminOrderRow> Orders { get; set; } = Array.Empty<AdminOrderRow>();
    }
}
