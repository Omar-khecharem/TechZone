using TechZone.Models;

namespace TechZone.ViewModels
{
    public class CatalogViewModel
    {
        public string? SearchTerm { get; set; }

        public int? CategoryId { get; set; }

        public string? MinPrice { get; set; }

        public string? MaxPrice { get; set; }

        public IReadOnlyList<CategoryFilterViewModel> Categories { get; set; } = Array.Empty<CategoryFilterViewModel>();

        public PagedResult<ProductCardViewModel> Products { get; set; } = new();
    }
}
