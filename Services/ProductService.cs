using Microsoft.EntityFrameworkCore;
using TechZone.Data;
using TechZone.Interfaces;
using TechZone.Models;

namespace TechZone.Services
{
    public class ProductService : IProductService
    {
        private const int DefaultPageSize = 8;

        private readonly ApplicationDbContext _dbContext;

        public ProductService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<Product>> GetProductsAsync(
            string? searchTerm,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            bool inStockOnly,
            string? sort,
            int page,
            int pageSize)
        {
            var query = _dbContext.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(product => product.Name.Contains(term));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(product => product.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(product => product.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(product => product.Price <= maxPrice.Value);
            }

            if (inStockOnly)
            {
                query = query.Where(product => product.Stock > 0);
            }

            query = sort switch
            {
                "price-asc" => query.OrderBy(product => product.Price).ThenBy(product => product.Name),
                "price-desc" => query.OrderByDescending(product => product.Price).ThenBy(product => product.Name),
                "name-asc" => query.OrderBy(product => product.Name),
                "newest" => query.OrderByDescending(product => product.CreatedAt).ThenBy(product => product.Name),
                _ => query.OrderByDescending(product => product.SalesCount).ThenBy(product => product.Name)
            };

            page = Math.Max(page, 1);
            pageSize = pageSize > 0 ? pageSize : DefaultPageSize;

            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)pageSize);
            var currentPage = totalPages == 0 ? 1 : Math.Min(page, totalPages);

            var products = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = products,
                Page = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .ToListAsync();
        }

        public async Task<IReadOnlyDictionary<int, int>> GetCategoryProductCountsAsync()
        {
            return await _dbContext.Products
                .AsNoTracking()
                .GroupBy(product => product.CategoryId)
                .Select(group => new { CategoryId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.CategoryId, item => item.Count);
        }

        public async Task<(decimal MinPrice, decimal MaxPrice)> GetPriceRangeAsync()
        {
            var minPrice = await _dbContext.Products
                .AsNoTracking()
                .MinAsync(product => (decimal?)product.Price);
            var maxPrice = await _dbContext.Products
                .AsNoTracking()
                .MaxAsync(product => (decimal?)product.Price);

            return (minPrice ?? 0m, maxPrice ?? 0m);
        }

        public Task<Product?> GetProductByIdAsync(int productId)
        {
            return _dbContext.Products
                .AsNoTracking()
                .Include(product => product.Category)
                .FirstOrDefaultAsync(product => product.Id == productId);
        }
    }
}
