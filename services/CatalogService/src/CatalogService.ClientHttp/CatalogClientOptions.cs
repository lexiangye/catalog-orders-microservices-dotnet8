namespace CatalogService.ClientHttp;

/// <summary>
/// Opzioni di configurazione per il client HTTP del Catalogo.
/// </summary>
public class CatalogClientOptions
{
    /// <summary>
    /// URL base del CatalogService (es. "http://catalogservice:8080").
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5052";
}