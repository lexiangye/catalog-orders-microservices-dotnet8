using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace CatalogService.ClientHttp;

public class CatalogServiceClient(HttpClient httpClient, ILogger<CatalogServiceClient> logger) 
    : ICatalogServiceClient
{
    public async Task<ProductResponse?> GetProductByIdAsync(int productId)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ProductResponse>($"api/products/{productId}");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to get product {ProductId}", productId);
            return null;
        }
    }

    public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<ProductResponse>>("api/products") 
                   ?? [];
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Failed to get products");
            return [];
        }
    }
}