using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TechZone.ViewModels.Admin
{
    public class AdminHomeMediaViewModel
    {
        [StringLength(500)]
        [Display(Name = "URL de la vidéo du hero")]
        public string? HeroVideoUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "URL de l'image poster du hero")]
        public string? HeroPosterUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "URL de l'image de la promotion")]
        public string? PromoImageUrl { get; set; }

        [Display(Name = "Vidéo du hero")]
        public IFormFile? HeroVideoFile { get; set; }

        [Display(Name = "Image poster du hero")]
        public IFormFile? HeroPosterFile { get; set; }

        [Display(Name = "Image de la promotion")]
        public IFormFile? PromoImageFile { get; set; }

        [Display(Name = "Supprimer la vidéo")]
        public bool RemoveHeroVideo { get; set; }

        [Display(Name = "Supprimer l'image poster")]
        public bool RemoveHeroPoster { get; set; }

        [Display(Name = "Supprimer l'image de la promotion")]
        public bool RemovePromoImage { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string EffectiveHeroVideoUrl =>
            RemoveHeroVideo ? string.Empty : HeroVideoUrl ?? string.Empty;

        public string EffectiveHeroPosterUrl =>
            RemoveHeroPoster ? string.Empty : HeroPosterUrl ?? string.Empty;

        public string EffectivePromoImageUrl =>
            RemovePromoImage ? string.Empty : PromoImageUrl ?? string.Empty;
    }
}
