using System.ComponentModel.DataAnnotations;

namespace TechZone.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "99999999.99")]
        public decimal UnitPrice { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; }
    }
}
