namespace TechZone.Models
{
    public class OrderHistoryPage
    {
        public IReadOnlyList<Order> Orders { get; init; } = Array.Empty<Order>();

        public int Page { get; init; }

        public int PageSize { get; init; }

        public int TotalCount { get; init; }

        public int TotalPages => TotalCount == 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => Page > 1;

        public bool HasNextPage => Page < TotalPages;
    }
}
