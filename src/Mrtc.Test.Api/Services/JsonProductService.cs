using System.Text.Json;

using Microsoft.Extensions.Options;

using Mrtc.Test.Api.Models;
using Mrtc.Test.Api.Services.Interfaces;

namespace Mrtc.Test.Api.Services;

public class JsonProductService : IProductService
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;    

    public JsonProductService(IWebHostEnvironment env, IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions> jsonOptions)
    {
        ArgumentNullException.ThrowIfNull(env);
        _jsonOptions = jsonOptions.Value.SerializerOptions;
        _filePath = Path.Combine(env.ContentRootPath, "test_products.json");
    }

    public IEnumerable<Product> GetAllProducts()
    {
        var payload = ReadPayload();
        return payload?.Products ?? [];
    }

    public Product? GetProductById(int id)
    {
        return ReadPayload()?.Products?.Find(p => p.Id == id);
    }

    public void AddProduct(Product product)
    {
        var payload = ReadPayload() ?? new ProductsPayload { Products = [] };
        var products = payload.Products ?? [];

        var newId = products.Count != 0 ? products.Max(p => p.Id) + 1 : 1;
        product.Id = newId;
        products.Add(product);
        payload.Products = products;

        WritePayload(payload);
    }

    public void UpdateProduct(int id, Product product)
    {
        var payload = ReadPayload() ?? new ProductsPayload { Products = [] };
        var products = payload.Products ?? [];

        var index = products.FindIndex(p => p.Id == id);
        if (index == -1)
            throw new KeyNotFoundException($"Product with id {id} not found.");

        product.Id = id;
        products[index] = product;
        payload.Products = products;

        WritePayload(payload);
    }

    public void DeleteProduct(int id)
    {
        var payload = ReadPayload() ?? new ProductsPayload { Products = [] };
        var products = payload.Products ?? [];

        var index = products.FindIndex(p => p.Id == id);
        if (index == -1)
            throw new KeyNotFoundException($"Product with id {id} not found.");

        products.RemoveAt(index);
        payload.Products = products;

        WritePayload(payload);
    }

    private ProductsPayload? ReadPayload()
    {
        if (!File.Exists(_filePath))
        {
            throw new Exception("Products file not found.");
        }

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<ProductsPayload>(json, _jsonOptions);
    }

    private void WritePayload(ProductsPayload payload)
    {
        var outJson = JsonSerializer.Serialize(payload, _jsonOptions);
        File.WriteAllText(_filePath, outJson);
    }    
}
