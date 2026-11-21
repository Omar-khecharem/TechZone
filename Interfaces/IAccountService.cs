using Microsoft.AspNetCore.Identity;
using TechZone.Models;

namespace TechZone.Interfaces
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(string firstName, string lastName, string email, string password);

        Task<ApplicationUser?> GetByIdAsync(string userId);

        Task<IdentityResult> UpdateProfileAsync(
            string userId,
            string firstName,
            string lastName,
            string? phoneNumber,
            string? profilePictureUrl);
    }
}
