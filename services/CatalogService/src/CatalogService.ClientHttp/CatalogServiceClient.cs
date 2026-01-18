using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CatalogService.ClientHttp;

/// <summary>
/// Implementazione del client HTTP per interagire con le API del CatalogService.
/// Include la gestione degli errori e il logging integrato.
/// </summary>
/// <param name="httpClient">L'istanza di <see cref="HttpClient"/> configurata (solitamente tramite dependency injection).</param>
/// <param name="logger">Logger per tracciare eventuali errori di comunicazione.</param>
public class CatalogServiceClient(HttpClient httpClient, ILogger<CatalogServiceClient> logger) 
    : ICatalogServiceClient
{
    /// <inheritdoc />
    public async Task<ProductResponse?> GetProductByIdAsync(int productId)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ProductResponse>($"api/products/{productId}");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Errore durante il recupero del prodotto {ProductId}", productId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<ProductResponse>>("api/products") 
                   ?? [];
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Errore durante il recupero della lista prodotti");
            return [];
        }
    }
}