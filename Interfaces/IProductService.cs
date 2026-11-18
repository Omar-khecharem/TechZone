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
            int page,
            int pageSize);

        Task<IReadOnlyList<Category>> GetCategoriesAsync();

        Task<Product?> GetProductByIdAsync(int productId);
    }
}
