namespace CatalogOrders.Shared.Contracts;

/// <summary>
/// DTO per trasferire informazioni sul prodotto via HTTP
/// </summary>
public record ProductDto
{
    public int Id { get; init; }                         // Id del prodotto
    public string Name { get; init; } = string.Empty;    // Nome del prodotto
    public decimal Price { get; init; }                  // Prezzo del prodotto
    public int AvailableQuantity { get; init; }          // Stock disponibile
}

/*
Quando viene usato:
  - OrderService chiama GET /api/products/{id} del CatalogService
  - Riceve indietro un ProductDto per verificare che il prodotto esista e quanto costa
*/