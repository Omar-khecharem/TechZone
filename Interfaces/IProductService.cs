using TechZone.Models;

namespace TechZone.Interfaces
{
    public interface IProductService
    {
        Task<PagedResult<Product>> GetProductsAsync(
            string? searchTerm,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            bool inStockOnly,
            string? sort,
            int page,
            int pageSize);

        Task<IReadOnlyList<Category>> GetCategoriesAsync();

        Task<IReadOnlyDictionary<int, int>> GetCategoryProductCountsAsync();

        Task<(decimal MinPrice, decimal MaxPrice)> GetPriceRangeAsync();

        Task<Product?> GetProductByIdAsync(int productId);
    }
}
