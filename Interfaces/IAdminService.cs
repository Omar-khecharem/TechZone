using TechZone.Models;
using TechZone.ViewModels.Admin;

namespace TechZone.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();

        Task<AdminProductListViewModel> GetProductsAsync(
            string? search,
            int? categoryId,
            int page,
            int pageSize);

        Task<IReadOnlyList<Category>> GetCategoriesAsync();

        Task<Product?> GetProductAsync(int id);

        Task<AdminOperationResult> CreateProductAsync(AdminProductFormViewModel model);

        Task<AdminOperationResult> UpdateProductAsync(AdminProductFormViewModel model);

        Task<AdminOperationResult> DeleteProductAsync(int id);

        Task<AdminCategoryListViewModel> GetCategoryListAsync();

        Task<Category?> GetCategoryAsync(int id);

        Task<AdminOperationResult> CreateCategoryAsync(AdminCategoryFormViewModel model);

        Task<AdminOperationResult> UpdateCategoryAsync(AdminCategoryFormViewModel model);

        Task<AdminOperationResult> DeleteCategoryAsync(int id);

        Task<AdminOrderListViewModel> GetOrdersAsync(
            string? search,
            OrderStatus? status,
            int page,
            int pageSize);

        Task<AdminOrderDetailsViewModel?> GetOrderAsync(int id);

        Task<AdminOperationResult> UpdateOrderAsync(
            int id,
            OrderStatus status,
            PaymentStatus paymentStatus);

        Task<AdminCustomerListViewModel> GetCustomersAsync(
            string? search,
            int page,
            int pageSize);

        Task<AdminCustomerDetailsViewModel?> GetCustomerAsync(string id);
    }
}
