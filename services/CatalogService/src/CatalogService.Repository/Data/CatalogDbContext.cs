using CatalogService.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repository.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Stock> Stocks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === CONFIGURAZIONE PRODUCT ===
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id); // Primary Key

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2); // Fondamentale per i soldi! (decimal)

            entity.Property(e => e.Description)
                .IsRequired(false); // Opzionale
        });

        // === CONFIGURAZIONE STOCK ===
        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Relazione 1:1 ESPLICITA
            entity.HasOne(s => s.Product)      // Lo stock ha un prodotto
                  .WithOne(p => p.Stock)       // Il prodotto ha uno stock
                  .HasForeignKey<Stock>(s => s.ProductId) // La chiave esterna sta nella tabella Stock
                  .OnDelete(DeleteBehavior.Cascade); // Se cancello il prodotto, sparisce lo stock
        });

        // === SEEDING DEI DATI ===
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Logitech MX Master 3", Price = 99.99m, Description = "Mouse ergonomico" },
            new Product { Id = 2, Name = "Keychron K2", Price = 89.99m, Description = "Tastiera meccanica" }
        );

        modelBuilder.Entity<Stock>().HasData(
            new Stock { Id = 1, ProductId = 1, Quantity = 10 },
            new Stock { Id = 2, ProductId = 2, Quantity = 25 }
        );
    }
}