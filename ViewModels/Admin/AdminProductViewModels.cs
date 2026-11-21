using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TechZone.Models;

namespace TechZone.ViewModels.Admin
{
    public class AdminProductRow
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public int SalesCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsLowStock => Stock <= 5;
    }

    public class AdminProductListViewModel
    {
        public string? Search { get; set; }

        public int? CategoryId { get; set; }

        public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();

        public PagedResult<AdminProductRow> Products { get; set; } = new();

        public int TotalCount => Products.TotalCount;
    }

    public class AdminProductFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom du produit est obligatoire.")]
        [StringLength(200, ErrorMessage = "Le nom ne doit pas dépasser 200 caractères.")]
        [Display(Name = "Nom du produit")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La description est obligatoire.")]
        [StringLength(4000, ErrorMessage = "La description ne doit pas dépasser 4000 caractères.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "Le prix doit être positif.")]
        [Display(Name = "Prix (€)")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Le stock doit être positif.")]
        [Display(Name = "Stock disponible")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Veuillez choisir une catégorie.")]
        [Display(Name = "Catégorie")]
        public int CategoryId { get; set; }

        [StringLength(500)]
        [Display(Name = "URL de l'image")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Image du produit")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }

        public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();

        public bool IsEdit => Id > 0;
    }
}
