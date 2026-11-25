using TechZone.Models;
using TechZone.ViewModels.Admin;

namespace TechZone.Interfaces
{
    public interface IHomeMediaService
    {
        Task<HomeMedia> GetAsync();

        Task<AdminOperationResult> UpdateAsync(
            string? heroVideoUrl,
            string? heroPosterUrl,
            string? promoImageUrl);
    }
}
