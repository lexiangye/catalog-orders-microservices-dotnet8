using CatalogOrders.Shared.Enums;

namespace OrderService.Business.Dtos;

/// <summary>
/// Rappresentazione completa di un ordine restituita dai servizi e dalle API.
/// Include informazioni sullo stato e il calcolo dei totali.
/// </summary>
/// <param name="Id">ID dell'ordine.</param>
/// <param name="CreatedAt">Data e ora di creazione.</param>
/// <param name="Status">Stato corrente dell'ordine (Pending, Confirmed, ecc.).</param>
/// <param name="Lines">Elenco dettagliato delle righe dell'ordine.</param>
/// <param name="TotalAmount">L'importo totale dell'ordine (somma di tutte le righe).</param>
public record OrderDto(
    int Id,
    DateTimeOffset CreatedAt,
    OrderStatus Status,
    List<OrderLineDto> Lines,
    decimal TotalAmount // totale ordine (somma delle righe)
);

/// <summary>
/// Dettaglio di una riga d'ordine completata con i dati storici del prodotto.
/// </summary>
/// <param name="ProductId">ID del prodotto.</param>
/// <param name="ProductName">Nome del prodotto (snapshot al momento dell'acquisto).</param>
/// <param name="Quantity">Quantità acquistata.</param>
/// <param name="UnitPrice">Prezzo unitario applicato.</param>
/// <param name="TotalPrice">Importo totale della riga (Prezzo * Quantità).</param>
public record OrderLineDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice // Quantity * UnitPrice
);