using CatalogOrders.Shared.Enums;

namespace OrderService.Business.Dtos;

// DTO di output: rappresenta un ordine come lo vuoi esporre fuori (API) o usare nei servizi.
// Contiene anche campi calcolati (TotalAmount) e la lista delle righe.
public record OrderDto(
    int Id,
    DateTimeOffset CreatedAt,
    OrderStatus Status,
    List<OrderLineDto> Lines,
    decimal TotalAmount // totale ordine (somma delle righe)
);

// DTO di output per una riga ordine.
// Include sia i dati “salvati” (nome, prezzo, quantità) sia un totale calcolato per riga.
public record OrderLineDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice // Quantity * UnitPrice
);