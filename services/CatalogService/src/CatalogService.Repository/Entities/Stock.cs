namespace CatalogService.Repository.Entities;

/// <summary>
/// Rappresenta la disponibilità a magazzino di un determinato prodotto.
/// </summary>
public class Stock
{
    // Identificatore univoco
    public int Id { get; set; }
    
    // ID del prodotto associato
    public int ProductId { get; set; } // FK
    
    // Quantità disponibile
    public int Quantity { get; set; }

    // Navigation Property verso il Prodotto
    public virtual Product Product { get; set; } = null!;
}
