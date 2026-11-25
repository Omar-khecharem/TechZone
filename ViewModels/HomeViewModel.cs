namespace TechZone.ViewModels
{
    public class HomeViewModel
    {
        public IReadOnlyList<ProductCardViewModel> FeaturedProducts { get; set; } =
            Array.Empty<ProductCardViewModel>();

        public IReadOnlyList<CategoryFilterViewModel> Categories { get; set; } =
            Array.Empty<CategoryFilterViewModel>();

        public string? HeroVideoUrl { get; set; }

        public string? HeroPosterUrl { get; set; }

        public string? PromoImageUrl { get; set; }
    }
}
