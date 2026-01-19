using CatalogService.ClientHttp;
using Microsoft.EntityFrameworkCore;
using OrderService.Business.Interfaces;
using OrderService.Business.Services;
using OrderService.Repository.Data;
using OrderService.Repository.Interfaces;
using OrderService.Repository.Repositories;
using OrderService.WebApi.Messaging;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURAZIONE DEI SERVIZI (DI)
// ==========================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === DATABASE ===
var connectionString = builder.Configuration.GetConnectionString("OrderConnection");
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// === REPOSITORY ===
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// === BUSINESS SERVICES ===
builder.Services.AddScoped<IOrderService, OrderService.Business.Services.OrderService>();
builder.Services.AddScoped<IStockEventHandler, StockEventHandler>();

// === HTTP CLIENT con Circuit Breaker (CatalogService) ===
builder.Services.AddCatalogServiceClient(options =>
{
    options.BaseUrl = builder.Configuration["CatalogService:BaseUrl"] ?? "http://localhost:5052";
});

// === CAP (Transactional Outbox e Inbox + Kafka) ===
builder.Services.AddCap(options =>
{
    // Configura MySQL come storage per l'Outbox
    // CAP creerà automaticamente le tabelle `cap.published` e `cap.received`
    options.UseMySql(connectionString!);
    
    // Configura Kafka come message broker
    options.UseKafka(kafka =>
    {
        kafka.Servers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
    });
    
    // Dashboard CAP per monitoraggio (accessibile su /cap)
    options.UseDashboard(d => d.PathMatch = "/cap");
    
    // Nome del gruppo consumer per questo servizio
    options.DefaultGroupName = "order-service";
    
    // Retry policy per messaggi falliti
    options.FailedRetryCount = 5;
    options.FailedRetryInterval = 30;
});

// === CAP Event Publisher ===
builder.Services.AddScoped<IEventPublisher, CapEventPublisher>();

// === CAP Subscriber per eventi Stock ===
builder.Services.AddTransient<CapStockEventsSubscriber>();

// ==========================================
// 2. COSTRUZIONE DELL'APP
// ==========================================
var app = builder.Build();

// ==========================================
// 3. PIPELINE HTTP
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();
app.MapControllers();

// ==========================================
// 4. MIGRATION AUTOMATICA
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<OrderDbContext>();
        context.Database.Migrate();
        Console.WriteLine("✅ OrderService database migration applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred while migrating the database: {ex.Message}");
    }
}

app.Run();
