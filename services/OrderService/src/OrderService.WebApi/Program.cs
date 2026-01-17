
using CatalogService.ClientHttp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using OrderService.Business.Interfaces;
using OrderService.Business.Services;
using OrderService.Repository.Data;
using OrderService.Repository.Interfaces;
using OrderService.Repository.Repositories;
using OrderService.WebApi.Kafka;

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
builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CatalogService:BaseUrl"] ?? "http://localhost:5052");
})
.AddStandardResilienceHandler();

// === KAFKA ===
builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
builder.Services.AddHostedService<StockEventsConsumer>();

// ==========================================
// 2. COSTRUZIONE DELL'APP
// ==========================================
var app = builder.Build();

// ==========================================
// 3. PIPELINE HTTP
// ==========================================
app.UseSwagger();
app.UseSwaggerUI();
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