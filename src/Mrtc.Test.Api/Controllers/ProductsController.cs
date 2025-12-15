using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mrtc.Test.Api.Models;
using Mrtc.Test.Api.Services.Interfaces;

namespace Mrtc.Test.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

    // GET: products
    [HttpGet]
    public ActionResult<IEnumerable<Product>> Get()
    {
        try
        {
            var products = _productService.GetAllProducts();
            return Ok(products);
        }
        catch (Exception ex)
        {
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
            if (product is null) return NotFound();
            return Ok(product);
        }
        catch (Exception ex)
        {
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
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
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
            return NoContent();
        }
        catch (KeyNotFoundException keyEx)
        {
            return NotFound(keyEx.Message);
        }
        catch (Exception ex)
        {
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
            return NoContent();
        }
        catch (KeyNotFoundException keyEx)
        {
            return NotFound(keyEx.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
