using Microsoft.EntityFrameworkCore;
using TechZone.Models;

namespace TechZone.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.Products.AnyAsync())
            {
                return;
            }

            var existingCategories = await dbContext.Categories.ToListAsync();

            Category GetCategory(string name, string description)
            {
                var category = existingCategories.FirstOrDefault(item => item.Name == name);

                if (category is null)
                {
                    category = new Category
                    {
                        Name = name,
                        Description = description
                    };

                    dbContext.Categories.Add(category);
                    existingCategories.Add(category);
                }

                return category;
            }

            var laptops = GetCategory(
                "Ordinateurs portables",
                "Ordinateurs portables pour le travail, les études et les loisirs.");
            var smartphones = GetCategory(
                "Smartphones",
                "Téléphones mobiles et accessoires associés.");
            var accessories = GetCategory(
                "Accessoires",
                "Périphériques et accessoires pour ordinateurs.");
            var screens = GetCategory(
                "Écrans",
                "Moniteurs et écrans haute définition.");

            var now = DateTime.UtcNow;

            dbContext.Products.AddRange(
                new Product
                {
                    Name = "UltraBook Pro 14",
                    Description = "Ordinateur portable 14 pouces avec processeur dernière génération, 16 Go de mémoire et SSD de 512 Go.",
                    Price = 1299.99m,
                    Stock = 12,
                    SalesCount = 42,
                    ImageUrl = "/images/products/ultrabook-pro-14.svg",
                    CreatedAt = now.AddDays(-100),
                    Category = laptops
                },
                new Product
                {
                    Name = "Notebook Air 13",
                    Description = "Portable léger de 13 pouces, idéal pour les déplacements, avec 8 Go de mémoire et un écran Full HD.",
                    Price = 949.00m,
                    Stock = 18,
                    SalesCount = 35,
                    ImageUrl = "/images/products/notebook-air-13.svg",
                    CreatedAt = now.AddDays(-90),
                    Category = laptops
                },
                new Product
                {
                    Name = "Studio Laptop 16",
                    Description = "Ordinateur portable 16 pouces pour la création, avec 32 Go de mémoire et une carte graphique dédiée.",
                    Price = 1599.00m,
                    Stock = 7,
                    SalesCount = 18,
                    ImageUrl = "/images/products/studio-laptop-16.svg",
                    CreatedAt = now.AddDays(-80),
                    Category = laptops
                },
                new Product
                {
                    Name = "Pulse X5",
                    Description = "Smartphone 6,5 pouces avec écran OLED, appareil photo 50 Mpx et 256 Go de stockage.",
                    Price = 699.99m,
                    Stock = 25,
                    SalesCount = 64,
                    ImageUrl = "/images/products/pulse-x5.svg",
                    CreatedAt = now.AddDays(-70),
                    Category = smartphones
                },
                new Product
                {
                    Name = "Pulse Mini",
                    Description = "Smartphone compact de 5,8 pouces avec 128 Go de stockage et une autonomie de deux jours.",
                    Price = 449.00m,
                    Stock = 30,
                    SalesCount = 27,
                    ImageUrl = "/images/products/pulse-mini.svg",
                    CreatedAt = now.AddDays(-60),
                    Category = smartphones
                },
                new Product
                {
                    Name = "Nova Ultra",
                    Description = "Smartphone haut de gamme avec écran 120 Hz, triple capteur photo et charge rapide.",
                    Price = 899.00m,
                    Stock = 14,
                    SalesCount = 51,
                    ImageUrl = "/images/products/nova-ultra.svg",
                    CreatedAt = now.AddDays(-50),
                    Category = smartphones
                },
                new Product
                {
                    Name = "Clavier mécanique K80",
                    Description = "Clavier mécanique rétroéclairé avec switches rouges et repose-poignets amovible.",
                    Price = 89.90m,
                    Stock = 40,
                    SalesCount = 73,
                    ImageUrl = "/images/products/clavier-mecanique-k80.svg",
                    CreatedAt = now.AddDays(-40),
                    Category = accessories
                },
                new Product
                {
                    Name = "Souris ergonomique M20",
                    Description = "Souris sans fil ergonomique, capteur optique précis et batterie rechargeable.",
                    Price = 39.90m,
                    Stock = 60,
                    SalesCount = 48,
                    ImageUrl = "/images/products/souris-ergonomique-m20.svg",
                    CreatedAt = now.AddDays(-30),
                    Category = accessories
                },
                new Product
                {
                    Name = "Casque Bluetooth H7",
                    Description = "Casque sans fil avec réduction de bruit active et 30 heures d'autonomie.",
                    Price = 129.00m,
                    Stock = 22,
                    SalesCount = 39,
                    ImageUrl = "/images/products/casque-bluetooth-h7.svg",
                    CreatedAt = now.AddDays(-20),
                    Category = accessories
                },
                new Product
                {
                    Name = "Chargeur USB-C 65 W",
                    Description = "Chargeur compact USB-C de 65 W compatible avec ordinateurs portables et smartphones.",
                    Price = 34.90m,
                    Stock = 80,
                    SalesCount = 85,
                    ImageUrl = "/images/products/chargeur-usb-c-65w.svg",
                    CreatedAt = now.AddDays(-15),
                    Category = accessories
                },
                new Product
                {
                    Name = "Écran 27 pouces QHD",
                    Description = "Moniteur 27 pouces QHD avec dalle IPS, 75 Hz et connectique HDMI et DisplayPort.",
                    Price = 279.00m,
                    Stock = 16,
                    SalesCount = 22,
                    ImageUrl = "/images/products/ecran-27-qhd.svg",
                    CreatedAt = now.AddDays(-10),
                    Category = screens
                },
                new Product
                {
                    Name = "Écran 32 pouces 4K",
                    Description = "Moniteur 32 pouces 4K avec dalle IPS, HDR10 et pied réglable en hauteur.",
                    Price = 449.00m,
                    Stock = 10,
                    SalesCount = 30,
                    ImageUrl = "/images/products/ecran-32-4k.svg",
                    CreatedAt = now.AddDays(-5),
                    Category = screens
                });

            await dbContext.SaveChangesAsync();
        }
    }
}
