using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TechZone.ViewModels.Admin
{
    public class AdminCategoryRow
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int ProductCount { get; set; }
    }

    public class AdminCategoryListViewModel
    {
        public IReadOnlyList<AdminCategoryRow> Categories { get; set; } =
            Array.Empty<AdminCategoryRow>();
    }

    public class AdminCategoryFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom de la catégorie est obligatoire.")]
        [StringLength(100, ErrorMessage = "Le nom ne doit pas dépasser 100 caractères.")]
        [Display(Name = "Nom de la catégorie")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La description ne doit pas dépasser 500 caractères.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "URL de l'image")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Image de la catégorie")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }

        public bool IsEdit => Id > 0;
    }
}
