using System.Text.Json;
using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Enums;
using CatalogOrders.Shared.Events;
using CatalogService.Business.Interfaces;
using Confluent.Kafka;

namespace CatalogService.WebApi.Kafka;

/// <summary>
/// Implementazione concreta del publisher Kafka. 
/// Si occupa di inviare eventi di dominio verso il broker Kafka per la comunicazione asincrona.
/// </summary>
public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        
        // Configurazione del Producer
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            // Garantisce che il messaggio sia scritto su tutte le repliche (massima persistenza)
            Acks = Acks.All,
            // Previene messaggi duplicati in caso di retry di rete
            EnableIdempotence = true
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    /// <summary>
    /// Notifica che lo stock è stato riservato con successo per un ordine.
    /// </summary>
    public async Task PublishStockReservedAsync(StockReservedEvent evt)
    {
        var envelope = EventEnvelope<StockReservedEvent>.Create(evt, EventType.StockReserved);
        await PublishAsync(KafkaTopics.StockReserved, envelope);
    }

    /// <summary>
    /// Notifica il fallimento della prenotazione dello stock (es. magazzino insufficiente).
    /// </summary>
    public async Task PublishStockReservationFailedAsync(StockReservationFailedEvent evt)
    {
        var envelope = EventEnvelope<StockReservationFailedEvent>.Create(evt, EventType.StockReservationFailed);
        await PublishAsync(KafkaTopics.StockReservationFailed, envelope);
    }

    /// <summary>
    /// Notifica il rilascio dello stock precedentemente riservato (es. a seguito di annullamento ordine).
    /// </summary>
    public async Task PublishStockReleasedAsync(StockReleasedEvent evt)
    {
        var envelope = EventEnvelope<StockReleasedEvent>.Create(evt, EventType.StockReleased);
        await PublishAsync(KafkaTopics.StockReleased, envelope);
    }

    /// <summary>
    /// Metodo core per la serializzazione e l'invio del messaggio a Kafka.
    /// </summary>
    /// <typeparam name="T">Il tipo di payload dell'evento.</typeparam>
    /// <param name="topic">Il topic Kafka di destinazione.</param>
    /// <param name="envelope">L'envelope contenente metadati e payload.</param>
    private async Task PublishAsync<T>(string topic, EventEnvelope<T> envelope) where T : class
    {
        var json = JsonSerializer.Serialize(envelope, _jsonOptions);
        
        var message = new Message<string, string>
        {
            // Usiamo l'ID dell'evento come chiave per mantenere l'ordine dei messaggi
            // legati alla stessa entità all'interno della stessa partizione.
            Key = envelope.EventId.ToString(),
            Value = json
        };

        // Invio asincrono al broker
        var result = await _producer.ProduceAsync(topic, message);
        
        _logger.LogInformation("Published {EventType} to {Topic} [partition {Partition}]",
            envelope.EventType, topic, result.Partition.Value);
    }

    /// <summary>
    /// Rilascia le risorse del producer Kafka (chiude le connessioni pendenti).
    /// </summary>
    public void Dispose() => _producer?.Dispose();
}