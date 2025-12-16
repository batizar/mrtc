using System.Text.Json;

using Mrtc.Test.Api.Models;

namespace Mrtc.Test.Api.UnitTest.Services
{
    public class JsonProductServiceTests(JsonServiceFixture fixture) : IClassFixture<JsonServiceFixture>
    {
        [Fact]
        public void GetAllProducts_ReturnsAll()
        {
            var sample = "{\"products\":[{\"id\":1,\"title\":\"A\",\"price\":1},{\"id\":2,\"title\":\"B\",\"price\":2}]}";
            var (service, dir) = fixture.CreateServiceWithJson(sample);

            var all = service.GetAllProducts().ToList();
            Assert.Equal(2, all.Count);
            Assert.Contains(all, p => p.Id == 1 && p.Title == "A");
            Assert.Contains(all, p => p.Id == 2 && p.Title == "B");
        }

        [Fact]
        public void GetProductById_ReturnsProduct_WhenExists()
        {
            var sample = "{\"products\":[{\"id\":1,\"title\":\"A\",\"price\":1}]}";
            var (service, dir) = fixture.CreateServiceWithJson(sample);

            var p = service.GetProductById(1);
            Assert.NotNull(p);
            Assert.Equal("A", p.Title);
        }

        [Fact]
        public void AddProduct_AssignsIdAndPersists()
        {
            var sample = "{\"products\":[{\"id\":1,\"title\":\"A\",\"price\":1},{\"id\":2,\"title\":\"B\",\"price\":2}]}";
            var (service, dir) = fixture.CreateServiceWithJson(sample);

            var newProduct = new Product { Title = "C", Price = 3 };
            service.AddProduct(newProduct);

            var file = Path.Combine(dir, "test_products.json");
            var json = File.ReadAllText(file);
            var payload = JsonSerializer.Deserialize<ProductsPayload>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(payload?.Products);
            Assert.Contains(payload.Products, p => p.Title == "C" && p.Id == 3);
        }

        [Fact]
        public void UpdateProduct_UpdatesExisting()
        {
            var sample = "{\"products\":[{\"id\":1,\"title\":\"A\",\"price\":1}]}";
            var (service, dir) = fixture.CreateServiceWithJson(sample);

            var updated = new Product { Title = "A-updated", Price = 9 };
            service.UpdateProduct(1, updated);

            var file = Path.Combine(dir, "test_products.json");
            var json = File.ReadAllText(file);
            var payload = JsonSerializer.Deserialize<ProductsPayload>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(payload?.Products);
            var p = payload.Products.FirstOrDefault();
            Assert.NotNull(p);
            Assert.Equal(1, p.Id);
            Assert.Equal("A-updated", p.Title);
        }

        [Fact]
        public void UpdateProduct_NonExisting_Throws()
        {
            var sample = "{\"products\":[]}";
            var (service, dir) = fixture.CreateServiceWithJson(sample);

            var updated = new Product { Title = "X", Price = 1 };
            Assert.Throws<KeyNotFoundException>(() => service.UpdateProduct(99, updated));
        }

        [Fact]
        public void DeleteProduct_RemovesExisting()
        {
            var sample = "{\"products\":[{\"id\":1,\"title\":\"A\",\"price\":1},{\"id\":2,\"title\":\"B\",\"price\":2}]}";
            var (service, dir) = fixture.CreateServiceWithJson(sample);

            service.DeleteProduct(1);
            var file = Path.Combine(dir, "test_products.json");
            var json = File.ReadAllText(file);
            var payload = JsonSerializer.Deserialize<ProductsPayload>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.DoesNotContain(payload.Products, p => p.Id == 1);
        }

        [Fact]
        public void DeleteProduct_NonExisting_Throws()
        {
            var sample = "{\"products\":[]}";
            var (service, dir) = fixture.CreateServiceWithJson(sample);

            Assert.Throws<KeyNotFoundException>(() => service.DeleteProduct(5));
        }

        [Fact]
        public void GetAllProducts_FileMissing_ThrowsException()
        {
            var (service, dir) = fixture.CreateServiceWithoutFile();
            Assert.Throws<FileNotFoundException>(() => service.GetAllProducts().ToList());
        }

        [Fact]
        public void GetProductById_FileMissing_ThrowsException()
        {
            var (service, dir) = fixture.CreateServiceWithoutFile();
            Assert.Throws<FileNotFoundException>(() => service.GetProductById(1));
        }

        [Fact]
        public void GetAllProducts_MalformedFile_ThrowsJsonException()
        {
            var malformed = "{ products: [ }";
            var (service, dir) = fixture.CreateServiceWithJson(malformed);
            Assert.Throws<JsonException>(() => service.GetAllProducts().ToList());
        }

        [Fact]
        public void GetProductById_MalformedFile_ThrowsJsonException()
        {
            var malformed = "{ products: [ }";
            var (service, dir) = fixture.CreateServiceWithJson(malformed);
            Assert.Throws<JsonException>(() => service.GetProductById(1));
        }
    }
}
