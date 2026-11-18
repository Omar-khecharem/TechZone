using System.ComponentModel.DataAnnotations;

namespace TechZone.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Le nom complet est obligatoire.")]
        [StringLength(100, ErrorMessage = "Le nom complet ne peut pas dépasser 100 caractères.")]
        [Display(Name = "Nom complet")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro de téléphone est obligatoire.")]
        [StringLength(30, ErrorMessage = "Le numéro de téléphone ne peut pas dépasser 30 caractères.")]
        [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide.")]
        [Display(Name = "Numéro de téléphone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse d'expédition est obligatoire.")]
        [StringLength(250, ErrorMessage = "L'adresse ne peut pas dépasser 250 caractères.")]
        [Display(Name = "Adresse d'expédition")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ville est obligatoire.")]
        [StringLength(100, ErrorMessage = "La ville ne peut pas dépasser 100 caractères.")]
        [Display(Name = "Ville")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le code postal est obligatoire.")]
        [StringLength(20, ErrorMessage = "Le code postal ne peut pas dépasser 20 caractères.")]
        [Display(Name = "Code postal")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le pays est obligatoire.")]
        [StringLength(100, ErrorMessage = "Le pays ne peut pas dépasser 100 caractères.")]
        [Display(Name = "Pays")]
        public string Country { get; set; } = string.Empty;

        public CartViewModel Cart { get; set; } = new();
    }
}
