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

    /// <summary>
    /// Retrieves a collection of all available products.
    /// </summary>
    /// <returns>An <see cref="ActionResult{T}"/> containing an enumerable collection of <see cref="Product"/> objects with HTTP
    /// status code 200 (OK) if successful; otherwise, a 500 (Internal Server Error) response if an error occurs.</returns>
    /// <response code="200">Returns the list of products</response>
    /// <response code="500">If any exception</response>

    // GET: products
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<Product>> Get()
    {
        try
        {
            var products = _productService.GetAllProducts();
            logger.LogDebug("Retrieved {ProductCount} {@Products}", products.Count(), products);
            return Ok(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the product with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product to retrieve.</param>
    /// <returns>An <see cref="ActionResult{Product}"/> containing the product if found; otherwise, a 404 Not Found response if
    /// the product does not exist, or a 500 Internal Server Error response if an unexpected error occurs.</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">If no product is found</response>
    /// <response code="500">If any exception</response>

    // GET products/5
    [HttpGet("{id}", Name = "Get")]    
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            logger.LogDebug("Retrieved product with id {ProductId}: {@Product}", id, product);
            return Ok(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving product with id {ProductId}", id);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Creates a new product with the specified details.
    /// </summary>
    /// <remarks>Requires authentication using Basic Authentication. The created product can be retrieved
    /// using the URI provided in the Location header of the response.</remarks>
    /// <param name="product">The product to create. The request body must contain a valid product object. Cannot be null.</param>
    /// <returns>A 201 Created response containing the created product if successful; a 400 Bad Request response if the product
    /// is null or the model state is invalid; or a 500 Internal Server Error response if an unexpected error occurs.</returns>
    /// <response code="201">Returns when product is created</response>
    /// <response code="400">If product payload is not valid</response>
    /// <response code="500">If any exception</response>

    // POST products
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            logger.LogDebug("Added new {@Product}", product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding new {@Product}", product);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Updates the product with the specified identifier using the provided product data.
    /// </summary>
    /// <remarks>The request requires authentication using Basic authentication. The product payload must be
    /// valid and not null. If the specified product does not exist, a 404 Not Found response is returned.</remarks>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="product">The updated product data to apply. The request body must contain a valid product object.</param>
    /// <returns>A 204 No Content response if the update is successful; a 400 Bad Request response if the input is invalid; a 404
    /// Not Found response if a product with the specified identifier does not exist; or a 500 Internal Server Error
    /// response if an unexpected error occurs.</returns>
    /// <response code="204">Returns when product is updated</response>
    /// <response code="400">If product payload is not valid</response>
    /// <response code="404">If product data source is not found</response>
    /// <response code="500">If any exception</response>

    // PUT products/5
    [HttpPut("{id}", Name = "Put")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            logger.LogDebug("Updated product with id {ProductId} to {@Product}", id, product);
            return NoContent();
        }
        catch (KeyNotFoundException keyEx)
        {
            logger.LogCritical(keyEx, "Product file was not found.");
            return NotFound(keyEx.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product with id {ProductId} and payload {@Product}", id, product);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Deletes the product with the specified identifier.
    /// </summary>
    /// <remarks>Requires authentication using Basic Authentication. Only authenticated users are authorized
    /// to perform this operation.</remarks>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>A 204 No Content response if the product was successfully deleted; a 404 Not Found response if the product does
    /// not exist; or a 500 Internal Server Error response if an unexpected error occurs.</returns>
    /// <response code="204">Returns when product is deleted</response>
    /// <response code="404">If product data source is not found</response>
    /// <response code="500">If any exception</response>

    // DELETE products/5
    [HttpDelete("{id}", Name = "Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
