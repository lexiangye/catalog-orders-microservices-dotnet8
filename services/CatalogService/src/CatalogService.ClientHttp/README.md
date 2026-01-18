# CatalogService.ClientHttp

Il client ufficiale .NET per interagire con il microservizio **CatalogService**.  
Questo pacchetto fornisce un'astrazione semplice e resiliente per recuperare informazioni sui prodotti e le relative disponibilità di magazzino.

## Caratteristiche principali

- **Typed Client**: Interfaccia `ICatalogServiceClient` pronta per la Dependency Injection.
- **Resilienza integrata**: Include politiche di **Retry** e **Circuit Breaker** tramite `Microsoft.Extensions.Http.Resilience`.
- **Modello semplificato**: Espone `ProductResponse` che appiattisce la relazione Prodotto/Stock per una facile fruizione.

## Installazione

Puoi installare il pacchetto tramite NuGet Package Manager o CLI:

```bash
dotnet add package CatalogService.ClientHttp
```

## Configurazione

Per registrare il client nella tua applicazione .NET (es. in `Program.cs` di un altro microservizio), usa il metodo di estensione fornito:

```csharp
using CatalogService.ClientHttp;

var builder = WebApplication.CreateBuilder(args);

// Registrazione del client con configurazione della resilienza
builder.Services.AddCatalogServiceClient(options =>
{
    options.BaseUrl = builder.Configuration["CatalogService:BaseUrl"] ?? "http://localhost:5052";
});
```

## Esempio di utilizzo

Una volta registrato, puoi iniettare `ICatalogServiceClient` in qualsiasi controller o servizio:

```csharp
public class MyOrderService(ICatalogServiceClient catalogClient)
{
    public async Task ProcessOrder(int productId)
    {
        // Il client gestisce automaticamente i retry in caso di errori temporanei di rete
        var product = await catalogClient.GetProductByIdAsync(productId);

        if (product != null && product.Quantity > 0)
        {
            // Procedi con la logica...
            Console.WriteLine($"Prodotto {product.Name} disponibile a {product.Price}€");
        }
    }
}
```

## Gestione della resilienza

Il client è configurato per proteggere il sistema da fallimenti a cascata:

- **Retry**: tenta nuovamente la richiesta in caso di timeout o errori 5xx.
- **Circuit Breaker**: se il `CatalogService` è offline, il client smette temporaneamente di inviare richieste per non sovraccaricare il sistema, lanciando un'eccezione rapida.

---

Sviluppato da **Lexiang Ye** — **Università di Parma**
