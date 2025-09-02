using countBy.Models;
using Microsoft.AspNetCore.Mvc;
using CountBy.Models;
using CountBy.Services;

namespace CountBy.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductsController(IProductReadService productReadService, ILogger<ProductsController> logger) : ControllerBase
{
    // GET: /Products
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDTO>))] 
    [ProducesResponseType(StatusCodes.Status204NoContent)] 
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts()
        {
            logger.LogInformation("Retrieving all products");

            try 
            {
                IEnumerable<ProductDTO> products = await productReadService.GetAllProductsAsync();

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

    // GET: /Products/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDTO>> GetAProduct(int id)
    {
        logger.LogInformation("Retrieving product with id {ID}", id);

        try 
        {
            ProductDTO? product = await productReadService.GetAProductAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        } 
        catch (Exception ex) 
        {
            logger.LogError(ex, "An error occurred while retrieving product with id {ID}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // GET: /Products/CategoryInfo
    [HttpGet("CategoryInfo")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoryDTO>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<CategoryDTO>> GetCategoryInfo()
    {
        logger.LogInformation("Retrieving Category Info");
        try
        {
            IReadOnlyCollection<CategoryDTO> products = productReadService.GetCategoryInfo();
            if (products.Count == 0)
                return NoContent();
            return Ok(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving all products");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

}
