using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mrtc.Test.Api.Models;
using Mrtc.Test.Api.Services.Interfaces;

namespace Mrtc.Test.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController(IProductService productService, ILogger<ProductsController> logger) : ControllerBase
{
    private readonly IProductService _productService = productService;

    // GET: products
    [HttpGet]
    public ActionResult<IEnumerable<Product>> Get()
    {
        try
        {
            var products = _productService.GetAllProducts();
            logger.LogDebug("Retrieved {ProductCount} {@products}", products.Count(), products);
            return Ok(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, ex.Message);
        }
    }

    // GET products/5
    [HttpGet("{id}")]
    public ActionResult<Product> Get(int id)
    {
        try
        {
            var product = _productService.GetProductById(id);
            if (product is null)
            {
                logger.LogDebug("Product with id {ProductId} not found", id);
                return NotFound();
            }
            logger.LogDebug("Retrieved product with id {ProductId}: {@product}", id, product);
            return Ok(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving product with id {ProductId}", id);
            return StatusCode(500, ex.Message);
        }
    }

    // POST products
    [HttpPost]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
    public ActionResult<Product> Post([FromBody] Product product)
    {
        if (product is null)
            return BadRequest("Product payload is required.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            _productService.AddProduct(product);
            logger.LogDebug("Added new {@product}", product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding new {@product}", product);
            return StatusCode(500, ex.Message);
        }
    }

    // PUT products/5
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
    public ActionResult Put(int id, [FromBody] Product product)
    {
        if (product is null)
            return BadRequest("Product payload is required.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            _productService.UpdateProduct(id, product);
            logger.LogDebug("Updated product with id {ProductId} to {@product}", id, product);
            return NoContent();
        }
        catch (KeyNotFoundException keyEx)
        {
            logger.LogCritical(keyEx, "Product file was not found.");
            return NotFound(keyEx.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product with id {ProductId} and payload {@product}", id, product);
            return StatusCode(500, ex.Message);
        }
    }

    // DELETE products/5
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
    public ActionResult Delete(int id)
    {
        try
        {
            _productService.DeleteProduct(id);
            logger.LogDebug("Deleted product with id {ProductId}", id);
            return NoContent();
        }
        catch (KeyNotFoundException keyEx)
        {
            logger.LogCritical(keyEx, "Product file was not found.");
            return NotFound(keyEx.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product with id {ProductId}", id);
            return StatusCode(500, ex.Message);
        }
    }
}
