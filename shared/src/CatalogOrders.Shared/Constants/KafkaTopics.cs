// Centralizza i nomi dei topic Kafka per evitare errori di battitura

namespace CatalogOrders.Shared.Constants;

/// <summary>
/// Nomi dei topic Kafka (evita stringhe magiche nel codice)
/// </summary>
public static class KafkaTopics
{
    // Topic per eventi del ciclo di vita degli ordini
    public const string OrderCreated = "order-created";
    public const string OrderCancelled = "order-cancelled";
    
    // Topic per eventi relativi allo stock
    public const string StockReserved = "stock-reserved";
    public const string StockReservationFailed = "stock-reservation-failed";
    public const string StockReleased = "stock-released";
}