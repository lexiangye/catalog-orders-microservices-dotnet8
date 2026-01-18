using CatalogService.Business.Dtos;

namespace CatalogService.Business.Interfaces;

/// <summary>
/// Servizio per la gestione delle operazioni di business sul catalogo prodotti.
/// </summary>
public interface ICatalogService
{
    /// <summary>Recupera tutti i prodotti trasformati in DTO.</summary>
    Task<IEnumerable<ProductDto>> GetProductsAsync();

    /// <summary>Recupera un prodotto specifico per ID.</summary>
    Task<ProductDto?> GetProductByIdAsync(int id);

    /// <summary>Crea un nuovo prodotto e ne inizializza lo stock.</summary>
    Task<ProductDto> CreateProductAsync(CreateProductDto productDto);

    /// <summary>Aggiorna i dettagli di un prodotto esistente.</summary>
    Task UpdateProductAsync(int id, CreateProductDto productDto);

    /// <summary>Elimina un prodotto dal catalogo.</summary>
    Task DeleteProductAsync(int id);
}