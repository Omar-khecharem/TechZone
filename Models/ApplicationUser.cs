using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TechZone.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public Cart? Cart { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
