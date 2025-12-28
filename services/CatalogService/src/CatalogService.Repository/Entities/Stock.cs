namespace CatalogService.Repository.Entities;

public class Stock
{
    public int Id { get; set; }
    public int ProductId { get; set; } // FK esplicita
    public int Quantity { get; set; }

    // Navigation Property
    public virtual Product Product { get; set; } = null!;
}