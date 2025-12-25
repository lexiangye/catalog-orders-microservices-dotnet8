// DTO completo di un ordine, usato nelle API per restituire ordini

namespace CatalogOrders.Shared.Contracts;

using CatalogOrders.Shared.Enums;

/// <summary>
/// DTO completo di un ordine (usato nelle API)
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    
    // Lista delle righe d'ordine
    public List<OrderLineDto> Lines { get; set; } = new();
    
    
    /// <summary>
    /// Totale ordine (calcolato sommando le righe)
    /// </summary>
    public decimal TotalAmount => Lines.Sum(l => l.TotalPrice);
}


// Quando viene usato: Quando l'utente chiede GET /api/orders/123