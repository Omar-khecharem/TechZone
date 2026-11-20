using TechZone.Models;

namespace TechZone.ViewModels
{
    public class CatalogViewModel
    {
        public string? SearchTerm { get; set; }

        public int? CategoryId { get; set; }

        public string? MinPrice { get; set; }

        public string? MaxPrice { get; set; }

        public bool HasPriceFilter { get; set; }

        public bool InStockOnly { get; set; }

        public string Sort { get; set; } = "featured";

        public decimal PriceFloor { get; set; }

        public decimal PriceCeil { get; set; }

        public IReadOnlyList<CategoryFilterViewModel> Categories { get; set; } = Array.Empty<CategoryFilterViewModel>();

        public PagedResult<ProductCardViewModel> Products { get; set; } = new();

        public CategoryFilterViewModel? ActiveCategory =>
            CategoryId.HasValue ? Categories.FirstOrDefault(category => category.Id == CategoryId.Value) : null;

        public bool HasFilters =>
            !string.IsNullOrWhiteSpace(SearchTerm)
            || CategoryId.HasValue
            || InStockOnly
            || HasPriceFilter;
    }
}
