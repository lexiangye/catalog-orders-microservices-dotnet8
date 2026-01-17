using Microsoft.AspNetCore.Mvc;
using OrderService.Business.Dtos;
using OrderService.Business.Interfaces;

namespace OrderService.WebApi.Controllers;

[ApiController] // abilita validazione automatica dei DTO + risposte 400 se ModelState non è valido
[Route("api/[controller]")] // -> /api/orders
public class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>
    /// Recupera tutti gli ordini
    /// </summary>
    [HttpGet] // GET /api/orders
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        // Chiede al layer Business la lista ordini (Repository + mapping a DTO)
        var orders = await orderService.GetOrdersAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Recupera un ordine tramite ID
    /// </summary>
    [HttpGet("{id:int}")] // GET /api/orders/{id}
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        // Recupera ordine dal Business
        var order = await orderService.GetOrderByIdAsync(id);

        // Se non esiste, ritorna 404
        if (order is null)
            return NotFound(new { message = $"Order with ID {id} not found" });

        return Ok(order);
    }

    /// <summary>
    /// Crea un nuovo ordine (stato iniziale: Pending)
    /// </summary>
    [HttpPost] // POST /api/orders
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            // Delego al Business:
            // - valida/recupera prodotti dal Catalog (HTTP)
            // - salva ordine su DB (Pending)
            // - pubblica evento Kafka OrderCreated (inizio saga)
            var created = await orderService.CreateOrderAsync(dto);

            // 201 Created + header Location che punta a GET /api/orders/{id}
            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            // Se il Business segnala input non valido (es. prodotto non trovato), ritorna 400
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancella un ordine (attiva compensazione saga)
    /// </summary>
    [HttpDelete("{id:int}")] // DELETE /api/orders/{id}
    public async Task<IActionResult> CancelOrder(int id)
    {
        // Controllo esistenza: se non c'è, 404
        var existing = await orderService.GetOrderByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Order with ID {id} not found" });

        // Chiede al Business di cancellare:
        // - aggiorna stato nel DB
        // - pubblica OrderCancelled (compensazione: release stock in Catalog)
        await orderService.CancelOrderAsync(id);

        return NoContent(); // 204: operazione eseguita, nessun body
    }
}
