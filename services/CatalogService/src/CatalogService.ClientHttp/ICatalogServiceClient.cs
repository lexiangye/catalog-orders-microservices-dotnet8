namespace CatalogService.ClientHttp;

public interface ICatalogServiceClient
{
    Task<ProductResponse?> GetProductByIdAsync(int productId);
    Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
}

public record ProductResponse(int Id, string Name, string? Description, decimal Price, int Quantity);