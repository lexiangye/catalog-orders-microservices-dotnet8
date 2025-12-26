namespace CatalogOrders.Shared.Contracts;

/// <summary>
/// Rappresenta una riga dell'ordine (prodotto + quantità)
/// </summary>
public record OrderLineDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    
    /// <summary>
    /// Totale della riga (calcolato)
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;
}

// Cosa rappresenta: Una riga dell'ordine (es: "3 x Mouse @ €25.99")