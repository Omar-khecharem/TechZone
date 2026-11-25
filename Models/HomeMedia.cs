using System.ComponentModel.DataAnnotations;

namespace TechZone.Models
{
    public class HomeMedia
    {
        public int Id { get; set; }

        [StringLength(500)]
        public string? HeroVideoUrl { get; set; }

        [StringLength(500)]
        public string? HeroPosterUrl { get; set; }

        [StringLength(500)]
        public string? PromoImageUrl { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
