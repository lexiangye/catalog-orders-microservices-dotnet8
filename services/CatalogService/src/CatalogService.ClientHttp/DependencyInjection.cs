using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace CatalogService.ClientHttp;

/// <summary>
/// Metodi di estensione per registrare il client HTTP del Catalogo.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Aggiunge il client HTTP del CatalogService con la gestione della resilienza integrata.
    /// Include automaticamente: Retry, Circuit Breaker, e Timeout.
    /// </summary>
    /// <param name="services">La collection dei servizi.</param>
    /// <param name="configureOptions">Azione per configurare l'URL base.</param>
    /// <returns>La collection dei servizi per il chaining.</returns>
    public static IServiceCollection AddCatalogServiceClient(
        this IServiceCollection services,
        Action<CatalogClientOptions> configureOptions)
    {
        // Legge le opzioni dall'utente
        var options = new CatalogClientOptions();
        configureOptions(options);

        // Registra l'HttpClient tipizzato con la pipeline di resilienza
        services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddStandardResilienceHandler(); // Retry + Circuit Breaker + Timeout

        return services;
    }
}