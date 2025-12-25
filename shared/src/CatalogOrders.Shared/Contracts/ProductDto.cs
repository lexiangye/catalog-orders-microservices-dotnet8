namespace CatalogOrders.Shared.Contracts;

/// <summary>
/// DTO per trasferire informazioni sul prodotto via HTTP
/// </summary>
public class ProductDto
{
    public int Id { get; set; }                         // Id del prodotto
    public string Name { get; set; } = string.Empty;    // Nome del prodotto
    public decimal Price { get; set; }                  // Prezzo del prodotto
    public int AvailableQuantity { get; set; }          // Stock disponibile
}

/*
Quando viene usato:
  - OrderService chiama GET /api/products/{id} del CatalogService
  - Riceve indietro un ProductDto per verificare che il prodotto esista e quanto costa
*/