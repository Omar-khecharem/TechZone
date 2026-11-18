namespace TechZone.ViewModels
{
    public class OrderHistoryViewModel
    {
        public IReadOnlyList<OrderSummaryViewModel> Orders { get; set; } =
            Array.Empty<OrderSummaryViewModel>();

        public int Page { get; set; } = 1;

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }

        public bool IsEmpty => Orders.Count == 0;

        public bool HasPreviousPage => Page > 1;

        public bool HasNextPage => Page < TotalPages;
    }
}
