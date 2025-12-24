// Rappresenta i possibili stati di un ordine durante il suo ciclo di vita

namespace CatalogOrders.Shared.Enums;

public enum OrderStatus
{
    Pending = 0,      // Ordine appena creato, in attesa di conferma stock
    Confirmed = 1,    // Stock riservato con successo, ordine confermato
    Rejected = 2,     // Stock insufficiente, ordine rifiutato
    Cancelled = 3     // Ordine cancellato dall'utente (per compensazione)
}