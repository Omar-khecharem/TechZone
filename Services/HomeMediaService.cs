using Microsoft.EntityFrameworkCore;
using TechZone.Data;
using TechZone.Interfaces;
using TechZone.Models;
using TechZone.ViewModels.Admin;

namespace TechZone.Services
{
    public class HomeMediaService : IHomeMediaService
    {
        private readonly ApplicationDbContext _dbContext;

        public HomeMediaService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HomeMedia> GetAsync()
        {
            var media = await _dbContext.HomeMedia
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return media ?? new HomeMedia();
        }

        public async Task<AdminOperationResult> UpdateAsync(
            string? heroVideoUrl,
            string? heroPosterUrl,
            string? promoImageUrl)
        {
            var media = await _dbContext.HomeMedia.FirstOrDefaultAsync();

            if (media is null)
            {
                media = new HomeMedia();
                _dbContext.HomeMedia.Add(media);
            }

            media.HeroVideoUrl = Normalize(heroVideoUrl);
            media.HeroPosterUrl = Normalize(heroPosterUrl);
            media.PromoImageUrl = Normalize(promoImageUrl);
            media.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return AdminOperationResult.Success();
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
