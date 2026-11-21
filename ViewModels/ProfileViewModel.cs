using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TechZone.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [StringLength(50, ErrorMessage = "Le prénom ne doit pas dépasser 50 caractères.")]
        [Display(Name = "Prénom")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(50, ErrorMessage = "Le nom ne doit pas dépasser 50 caractères.")]
        [Display(Name = "Nom")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Adresse e-mail")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide.")]
        [StringLength(30, ErrorMessage = "Le numéro de téléphone ne doit pas dépasser 30 caractères.")]
        [Display(Name = "Téléphone")]
        public string? PhoneNumber { get; set; }

        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Photo de profil")]
        public IFormFile? ProfilePicture { get; set; }

        public IReadOnlyList<OrderSummaryViewModel> RecentOrders { get; set; } =
            Array.Empty<OrderSummaryViewModel>();

        public int TotalOrders { get; set; }
    }
}
