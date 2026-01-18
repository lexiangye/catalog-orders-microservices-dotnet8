using System.Text.Json;
using CatalogOrders.Shared.Constants;
using CatalogOrders.Shared.Enums;
using CatalogOrders.Shared.Events;
using Confluent.Kafka;
using OrderService.Business.Interfaces;

namespace OrderService.WebApi.Kafka;

/// <summary>
/// Servizio responsabile della pubblicazione di eventi di dominio sul bus di messaggi Apache Kafka.
/// Implementa il pattern Outbox tramite invio asincrono per la coreografia della saga.
/// </summary>
public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    // Producer Kafka che invia messaggi (key/value)
    private readonly IProducer<string, string> _producer;

    // Logger per tracciare pubblicazioni e problemi
    private readonly ILogger<KafkaEventPublisher> _logger;

    // Opzioni JSON: camelCase per avere JSON “standard” (es. eventId, createdAt...)
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;

        // Legge BootstrapServers da appsettings (Kafka:BootstrapServers)
        // Se non presente, fallback a localhost:9092
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            Acks = Acks.All,              // aspetta conferma da tutti i replica leader (più affidabile)
            EnableIdempotence = true      // riduce duplicati in caso di retry
        };

        // Costruisce il producer Kafka
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    /// <summary>
    /// Pubblica l'evento di creazione di un nuovo ordine.
    /// </summary>
    /// <param name="evt">Dati dell'evento <see cref="OrderCreatedEvent"/>.</param>
    public async Task PublishOrderCreatedAsync(OrderCreatedEvent evt)
    {
        // Incapsula l’evento in un envelope con metadata (EventId, timestamp, type...)
        var envelope = EventEnvelope<OrderCreatedEvent>.Create(evt, EventType.OrderCreated);

        // Pubblica sul topic dedicato
        await PublishAsync(KafkaTopics.OrderCreated, envelope);
    }

    /// <summary>
    /// Pubblica l'evento di cancellazione di un ordine per attivare la compensazione lato catalogo.
    /// </summary>
    /// <param name="evt">Dati dell'evento <see cref="OrderCancelledEvent"/>.</param>
    public async Task PublishOrderCancelledAsync(OrderCancelledEvent evt)
    {
        var envelope = EventEnvelope<OrderCancelledEvent>.Create(evt, EventType.OrderCancelled);
        await PublishAsync(KafkaTopics.OrderCancelled, envelope);
    }

    /// <summary>
    /// Metodo generico privato per la serializzazione e l'invio fisico del messaggio su Kafka.
    /// </summary>
    /// <typeparam name="T">Tipo del payload dell'evento.</typeparam>
    /// <param name="topic">Il topic di destinazione.</param>
    /// <param name="envelope">L'involucro contenente l'evento e i metadati.</param>
    private async Task PublishAsync<T>(string topic, EventEnvelope<T> envelope) where T : class
    {
        // Serializza envelope + payload in JSON
        var json = JsonSerializer.Serialize(envelope, _jsonOptions);

        // Messaggio Kafka: chiave = EventId (utile per ordering/partitioning), valore = JSON
        var message = new Message<string, string>
        {
            Key = envelope.EventId.ToString(),
            Value = json
        };

        // Invia su Kafka (ProduceAsync attende ack dal broker)
        var result = await _producer.ProduceAsync(topic, message);

        // Log informativo con topic e partition
        _logger.LogInformation("📤 Published {EventType} to {Topic} [partition {Partition}]",
            envelope.EventType, topic, result.Partition.Value);
    }

    // Libera risorse native del producer (connessioni, buffer ecc.)
    public void Dispose() => _producer?.Dispose();
}
