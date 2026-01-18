namespace CatalogService.Repository.Entities;

/// <summary>
/// Rappresenta un prodotto all'interno del catalogo commerciale.
/// </summary>
public class Product
{
    // ID univoco del prodotto
    public int Id { get; set; }
    
    // Nome del prodotto
    public string Name { get; set; } = string.Empty;
    
    // Prezzo di vendita
    public decimal Price { get; set; }
    
    // Descrizione opzionale
    public string? Description { get; set; }

    // Navigation Property verso il magazzino (1:1)
    public virtual Stock? Stock { get; set; }
}
