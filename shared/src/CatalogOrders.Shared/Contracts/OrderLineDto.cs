// Rappresenta una singola riga dell'ordine (un prodotto + quantità)

namespace CatalogOrders.Shared;

public class OrderLineDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Calcolato: quantità x prezzo unitario
    public decimal TotalPrice => Quantity * UnitPrice;
}

// Cosa rappresenta: Una riga dell'ordine (es: "3 x Mouse @ €25.99")