namespace CatalogService.ClientHttp;

/// <summary>
/// Interfaccia per il client HTTP del servizio Catalogo.
/// Gestisce il recupero delle informazioni sui prodotti e le relative disponibilità.
/// </summary>
public interface ICatalogServiceClient
{
    /// <summary>
    /// Recupera i dettagli di un singolo prodotto in base al suo identificativo univoco.
    /// </summary>
    /// <param name="productId">L'ID del prodotto da cercare.</param>
    /// <returns>I dati del prodotto se trovato, altrimenti <c>null</c>.</returns>
    Task<ProductResponse?> GetProductByIdAsync(int productId);

    /// <summary>
    /// Recupera l'elenco completo di tutti i prodotti disponibili nel catalogo, inclusa la quantità in stock.
    /// </summary>
    /// <returns>Una collezione di <see cref="ProductResponse"/>. Se non ci sono prodotti, restituisce una lista vuota.</returns>
    Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
}

/// <summary>
/// Rappresenta la risposta contenente i dati del prodotto e la disponibilità a magazzino.
/// </summary>
/// <param name="Id">ID univoco del prodotto.</param>
/// <param name="Name">Nome commerciale del prodotto.</param>
/// <param name="Description">Descrizione opzionale dei dettagli del prodotto.</param>
/// <param name="Price">Prezzo unitario di vendita.</param>
/// <param name="Quantity">Quantità attualmente disponibile in stock.</param>
public record ProductResponse(int Id, string Name, string? Description, decimal Price, int Quantity);