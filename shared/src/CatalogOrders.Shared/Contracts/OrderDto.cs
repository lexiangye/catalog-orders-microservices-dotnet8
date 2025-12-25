namespace CatalogOrders.Shared.Contracts;

using CatalogOrders.Shared.Enums;

/// <summary>
/// DTO completo di un ordine (usato nelle API)
/// </summary>
public record OrderDto
{
    public int Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public OrderStatus Status { get; init; }
    
    // Lista delle righe d'ordine
    public List<OrderLineDto> Lines { get; init; } = new();
    
    
    /// <summary>
    /// Totale ordine (calcolato sommando le righe)
    /// </summary>
    public decimal TotalAmount => Lines.Sum(l => l.TotalPrice);
}


// Quando viene usato: Quando l'utente chiede GET /api/orders/123