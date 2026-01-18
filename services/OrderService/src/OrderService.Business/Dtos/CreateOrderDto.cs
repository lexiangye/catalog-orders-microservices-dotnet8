using System.ComponentModel.DataAnnotations;

namespace OrderService.Business.Dtos;

/// <summary>
/// Oggetto di trasferimento dati per la creazione di un nuovo ordine.
/// Viene validato automaticamente dal framework tramite le Data Annotations.
/// </summary>
/// <param name="Lines">Elenco degli articoli (prodotti e quantità) da includere nell'ordine.</param>
public record CreateOrderDto(
    [Required] List<CreateOrderLineDto> Lines
);

/// <summary>
/// Rappresenta una singola riga di un ordine in fase di creazione.
/// </summary>
/// <param name="ProductId">L'ID univoco del prodotto nel catalogo.</param>
/// <param name="Quantity">Quantità desiderata (accettata tra 1 e 100).</param>
public record CreateOrderLineDto(
    [Required] int ProductId,
    [Range(1, 100)] int Quantity // quantità valida: almeno 1, massimo 100
);