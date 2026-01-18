using CatalogService.Repository.Entities;

namespace CatalogService.Repository.Interfaces;

/// <summary>
/// Interfaccia per la gestione della persistenza dei prodotti.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Recupera la lista completa dei prodotti con le relative informazioni di stock.
    /// </summary>
    /// <returns>Una collezione di <see cref="Product"/>.</returns>
    Task<IEnumerable<Product>> GetProductsAsync();

    /// <summary>
    /// Recupera un singolo prodotto in base al suo identificativo univoco.
    /// </summary>
    /// <param name="id">L'identificativo del prodotto.</param>
    /// <returns>Il prodotto trovato o <c>null</c> se non esistente.</returns>
    Task<Product?> GetProductByIdAsync(int id);
    
    /// <summary>
    /// Inserisce un nuovo prodotto nel database.
    /// </summary>
    /// <param name="product">L'entità prodotto da creare.</param>
    Task CreateProductAsync(Product product);

    /// <summary>
    /// Aggiorna i dati di un prodotto esistente.
    /// </summary>
    /// <param name="product">L'entità con i dati aggiornati.</param>
    Task UpdateProductAsync(Product product);

    /// <summary>
    /// Rimuove un prodotto dal sistema in base all'ID.
    /// </summary>
    /// <param name="id">L'ID del prodotto da eliminare.</param>
    Task DeleteProductAsync(int id);
}