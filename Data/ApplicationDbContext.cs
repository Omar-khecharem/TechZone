using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechZone.Models;

namespace TechZone.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Cart> Carts => Set<Cart>();

        public DbSet<CartItem> CartItems => Set<CartItem>();

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<HomeMedia> HomeMedia => Set<HomeMedia>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(user => user.ProfilePictureUrl).HasMaxLength(500);
            });

            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(category => category.Name).IsUnique();
                entity.Property(category => category.ImageUrl).HasMaxLength(500);
                entity.HasMany(category => category.Products)
                    .WithOne(product => product.Category)
                    .HasForeignKey(product => product.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Product>(entity =>
            {
                entity.Property(product => product.Price).HasPrecision(18, 2);
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_Products_Price", "[Price] >= 0");
                    table.HasCheckConstraint("CK_Products_Stock", "[Stock] >= 0");
                    table.HasCheckConstraint("CK_Products_SalesCount", "[SalesCount] >= 0");
                });
            });

            builder.Entity<Cart>(entity =>
            {
                entity.HasIndex(cart => cart.UserId).IsUnique();
                entity.HasOne(cart => cart.User)
                    .WithOne(user => user.Cart)
                    .HasForeignKey<Cart>(cart => cart.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CartItem>(entity =>
            {
                entity.HasIndex(item => new { item.CartId, item.ProductId }).IsUnique();
                entity.HasOne(item => item.Cart)
                    .WithMany(cart => cart.Items)
                    .HasForeignKey(item => item.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Product)
                    .WithMany(product => product.CartItems)
                    .HasForeignKey(item => item.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.ToTable(table =>
                    table.HasCheckConstraint("CK_CartItems_Quantity", "[Quantity] > 0"));
            });

            builder.Entity<Order>(entity =>
            {
                entity.HasIndex(order => order.OrderNumber).IsUnique();
                entity.Property(order => order.TotalAmount).HasPrecision(18, 2);
                entity.Property(order => order.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                entity.Property(order => order.PaymentMethod)
                    .HasConversion<string>()
                    .HasMaxLength(30);
                entity.Property(order => order.PaymentStatus)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                entity.HasOne(order => order.User)
                    .WithMany(user => user.Orders)
                    .HasForeignKey(order => order.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.ToTable(table =>
                    table.HasCheckConstraint("CK_Orders_TotalAmount", "[TotalAmount] >= 0"));
            });

            builder.Entity<OrderItem>(entity =>
            {
                entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
                entity.HasOne(item => item.Order)
                    .WithMany(order => order.Items)
                    .HasForeignKey(item => item.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(item => item.Product)
                    .WithMany(product => product.OrderItems)
                    .HasForeignKey(item => item.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_OrderItems_UnitPrice", "[UnitPrice] >= 0");
                    table.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0");
                });
            });

            builder.Entity<HomeMedia>(entity =>
            {
                entity.Property(media => media.HeroVideoUrl).HasMaxLength(500);
                entity.Property(media => media.HeroPosterUrl).HasMaxLength(500);
                entity.Property(media => media.PromoImageUrl).HasMaxLength(500);
            });
        }
    }
}
