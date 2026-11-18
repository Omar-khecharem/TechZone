using Microsoft.AspNetCore.Identity;
using TechZone.Interfaces;
using TechZone.Models;

namespace TechZone.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> RegisterAsync(string firstName, string lastName, string email, string password)
        {
            var normalizedEmail = email.Trim();

            var user = new ApplicationUser
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = normalizedEmail,
                UserName = normalizedEmail
            };

            return await _userManager.CreateAsync(user, password);
        }

        public Task<ApplicationUser?> GetByIdAsync(string userId)
        {
            return _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityResult> UpdateProfileAsync(string userId, string firstName, string lastName, string? phoneNumber)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "Utilisateur introuvable."
                });
            }

            user.FirstName = firstName.Trim();
            user.LastName = lastName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

            return await _userManager.UpdateAsync(user);
        }
    }
}
