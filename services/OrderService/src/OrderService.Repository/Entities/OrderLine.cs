namespace OrderService.Repository.Entities;

/// <summary>
/// Rappresenta una riga di ordine nel database, contenente dettagli sul prodotto e la quantità ordinata.
/// </summary>
public class OrderLine
{
    // Identificatore univoco
    public int Id { get; set; }
    
    // Identificatore ordine
    public int OrderId { get; set; } // FK
    
    // Identificatore prodotto
    public int ProductId { get; set; }
    
    // Nome prodotto
    public string ProductName { get; set; } = string.Empty;
    
    // Quantità prodotto
    public int Quantity { get; set; }
    
    // Prezzo unitario prodotto
    public decimal UnitPrice { get; set; }
    
    // Ordine associato
    public virtual Order Order { get; set; } = null!; // navigation property
}