namespace CatalogOrders.Shared.Enums;

/// <summary>
/// Tipi di eventi che possono essere pubblicati su Kafka
/// </summary>
public enum EventType
{
    OrderCreated,
    OrderCancelled,
    StockReserved,
    StockReservationFailed,
    StockReleased
}