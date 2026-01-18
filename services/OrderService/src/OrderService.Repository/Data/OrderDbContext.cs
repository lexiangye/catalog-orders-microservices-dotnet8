using Microsoft.EntityFrameworkCore;
using OrderService.Repository.Entities;

namespace OrderService.Repository.Data;

/// <summary>
/// Contesto del database per il servizio ordini. 
/// Gestisce la sessione con il database e la mappatura tra classi C# e tabelle relazionali.
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    // Tabelle del database
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }

    /// <summary>
    /// Configura il modello delle entità e le relazioni tramite Fluent API.
    /// </summary>
    /// <param name="modelBuilder">Il builder utilizzato per configurare le entità.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configurazione tabella Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<int>(); // Enum -> int
        });

        // Configurazione tabella OrderLine
        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            
            entity.HasOne(ol => ol.Order)
                .WithMany(o => o.Lines)
                .HasForeignKey(ol => ol.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}