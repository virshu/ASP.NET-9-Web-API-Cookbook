using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using cookbook.Models;
using cookbook.Services;

namespace cookbook.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductsController(IProductsService productsService, ILogger<ProductsController> logger)
    : ControllerBase
{
    // GET: /Products/AllAtOnce
    [HttpGet("AllAtOnce")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDTO>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts()
    {
        logger.LogInformation("Retrieving all products");

        try
        {
            IEnumerable<ProductDTO> products = await productsService.GetAllProductsAsync();

            if (!products.Any())
                return NoContent();

            return Ok(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving all products");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // GET: /Products
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDTO>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts(int pageSize, int? lastProductId = null)
    {
        if (pageSize <= 0)
        {
            return BadRequest("pageSize must be greater than 0");
        }

        PagedProductResponseDTO pagedResult = await productsService.GetPagedProductsAsync(pageSize, lastProductId);
        string? previousPageUrl = pagedResult.HasPreviousPage
            ? Url.Action("GetProducts", new { pageSize,
                lastProductId = pagedResult.Items.First().Id }) : null;
        string? nextPageUrl = pagedResult.HasNextPage
            ? Url.Action("GetProducts", new { pageSize,
                lastProductId = pagedResult.Items.Last().Id }) : null;
        var paginationMetadata = new
        {
            pagedResult.PageSize,
            pagedResult.HasPreviousPage,
            pagedResult.HasNextPage,
            pagedResult.TotalPages,
            PreviousPageUrl = pagedResult.HasPreviousPage
                ? Url.Action("GetProducts", new { pageSize, lastProductId = pagedResult.Items.First().Id }) : null,
            NextPageUrl = pagedResult.HasNextPage
                ? Url.Action("GetProducts", new { pageSize, lastProductId = pagedResult.Items.Last().Id }) : null,
            FirstPageUrl = Url.Action("GetProducts", new { pageSize }),
            LastPageUrl = Url.Action("GetProducts",
                new { pageSize, lastProductId = (pagedResult.TotalPages - 1) * pageSize })
        };

        JsonSerializerOptions options = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata, options));
        return Ok(pagedResult.Items);
    }
}
