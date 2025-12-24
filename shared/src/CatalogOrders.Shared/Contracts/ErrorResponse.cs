// Formato standardizzato per restituire errori dalle API
// Rende gli errori consistenti tra CatalogService e OrderService

namespace CatalogOrders.Shared.Contracts;

public class ErrorResponse
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    
    // Constructor per errori semplici
    public ErrorResponse() { }
    
    public ErrorResponse(string errorCode, string message)
    {
        ErrorCode = errorCode;
        Message = message;
    }
}

// Quando viene usato: Quando qualcosa va storto (prodotto non trovato, stock insufficiente)