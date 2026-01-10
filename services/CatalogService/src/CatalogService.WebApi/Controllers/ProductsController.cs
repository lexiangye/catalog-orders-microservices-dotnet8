using CatalogService.Business.Dtos;
using CatalogService.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ICatalogService catalogService) : ControllerBase
{
    /// <summary>
    /// Recupera tutti i prodotti
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await catalogService.GetProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Recupera un prodotto tramite ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await catalogService.GetProductByIdAsync(id);
        
        if (product is null)
            return NotFound(new { message = $"Product with ID {id} not found" });
        
        return Ok(product);
    }

    /// <summary>
    /// Crea un nuovo prodotto
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        var created = await catalogService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
    }

    /// <summary>
    /// Aggiorna un prodotto esistente
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductDto dto)
    {
        var existing = await catalogService.GetProductByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Product with ID {id} not found" });
        
        await catalogService.UpdateProductAsync(id, dto);
        return NoContent();
    }

    /// <summary>
    /// Elimina un prodotto
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var existing = await catalogService.GetProductByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Product with ID {id} not found" });
        
        await catalogService.DeleteProductAsync(id);
        return NoContent();
    }
}