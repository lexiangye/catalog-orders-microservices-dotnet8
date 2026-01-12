namespace CatalogService.Repository.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }

    // Navigation Property
    public virtual Stock? Stock { get; set; }
}
