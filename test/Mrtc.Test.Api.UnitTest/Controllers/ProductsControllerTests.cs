using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

using Mrtc.Test.Api.Controllers;
using Mrtc.Test.Api.Models;
using Mrtc.Test.Api.Services.Interfaces;

namespace Mrtc.Test.Api.UnitTest.Controllers
{
    public class ProductsControllerTests
    {
        private class StubProductService : IProductService
        {
            public Func<IEnumerable<Product>>? OnGetAll;
            public Func<int, Product?>? OnGetById;
            public Action<Product>? OnAdd;
            public Action<int, Product>? OnUpdate;
            public Action<int>? OnDelete;

            public IEnumerable<Product> GetAllProducts() => OnGetAll != null ? OnGetAll() : new List<Product>();
            public Product? GetProductById(int id) => OnGetById != null ? OnGetById(id) : null;
            public void AddProduct(Product product) => OnAdd?.Invoke(product);
            public void UpdateProduct(int id, Product product) => OnUpdate?.Invoke(id, product);
            public void DeleteProduct(int id) => OnDelete?.Invoke(id);
        }

        private static ProductsController CreateController(IProductService svc)
        {
            // provide a logger instance to satisfy the controller constructor
            var logger = NullLogger<ProductsController>.Instance;
            var controller = new ProductsController(svc, logger);
            // Ensure model state is clear for tests that rely on validation passing
            controller.ModelState.Clear();
            return controller;
        }

        [Fact]
        public void Get_ReturnsOkWithProducts()
        {
            var svc = new StubProductService
            {
                OnGetAll = () => new List<Product> { new Product { Id = 1, Title = "A" }, new Product { Id = 2, Title = "B" } }
            };

            var controller = CreateController(svc);

            var result = controller.Get();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsAssignableFrom<IEnumerable<Product>>(ok.Value);
            Assert.Collection(items,
                p => Assert.Equal("A", p.Title),
                p => Assert.Equal("B", p.Title)
            );
        }

        [Fact]
        public void Get_ById_ReturnsOk_WhenFound()
        {
            var svc = new StubProductService { OnGetById = id => new Product { Id = id, Title = "A" } };
            var controller = CreateController(svc);

            var result = controller.Get(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var product = Assert.IsType<Product>(ok.Value);
            Assert.Equal(1, product.Id);
        }

        [Fact]
        public void Get_ById_ReturnsNotFound_WhenMissing()
        {
            var svc = new StubProductService { OnGetById = id => null };
            var controller = CreateController(svc);

            var result = controller.Get(5);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Post_NullPayload_ReturnsBadRequest()
        {
            var svc = new StubProductService();
            var controller = CreateController(svc);

            var result = controller.Post(null);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Post_Valid_ReturnsCreated()
        {
            var svc = new StubProductService
            {
                OnAdd = p => p.Id = 42
            };
            var controller = CreateController(svc);
            var prod = new Product { Title = "New", Price = 10 };

            var result = controller.Post(prod);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<Product>(created.Value);
            Assert.Equal(42, returned.Id);
        }

        [Fact]
        public void Put_NullPayload_ReturnsBadRequest()
        {
            var svc = new StubProductService();
            var controller = CreateController(svc);

            var result = controller.Put(1, null);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Put_Valid_ReturnsNoContent()
        {
            var svc = new StubProductService { OnUpdate = (id, p) => { /* ok */ } };
            var controller = CreateController(svc);
            var prod = new Product { Title = "U" };

            var result = controller.Put(1, prod);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Put_NonExisting_ReturnsNotFound()
        {
            var svc = new StubProductService { OnUpdate = (id, p) => throw new KeyNotFoundException("missing") };
            var controller = CreateController(svc);
            var prod = new Product { Title = "U" };

            var result = controller.Put(99, prod);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("missing", notFound.Value?.ToString());
        }

        [Fact]
        public void Delete_Existing_ReturnsNoContent()
        {
            var svc = new StubProductService { OnDelete = id => { /* ok */ } };
            var controller = CreateController(svc);

            var result = controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_NonExisting_ReturnsNotFound()
        {
            var svc = new StubProductService { OnDelete = id => throw new KeyNotFoundException("not found") };
            var controller = CreateController(svc);

            var result = controller.Delete(5);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFound.Value?.ToString());
        }

        [Fact]
        public void Get_WhenServiceThrows_ReturnsInternalServer()
        {
            var svc = new StubProductService { OnGetAll = () => throw new Exception("boom-get") };
            var controller = CreateController(svc);

            var result = controller.Get();
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("boom-get", obj.Value);
        }

        [Fact]
        public void GetById_WhenServiceThrows_ReturnsInternalServer()
        {
            var svc = new StubProductService { OnGetById = id => throw new Exception("boom-get-id") };
            var controller = CreateController(svc);

            var result = controller.Get(1);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("boom-get-id", obj.Value);
        }

        [Fact]
        public void Post_WhenServiceThrows_ReturnsInternalServer()
        {
            var svc = new StubProductService { OnAdd = p => throw new Exception("boom-add") };
            var controller = CreateController(svc);
            var prod = new Product { Title = "X", Price = 1 };

            var result = controller.Post(prod);
            var obj = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("boom-add", obj.Value);
        }

        [Fact]
        public void Put_WhenServiceThrows_ReturnsInternalServer()
        {
            var svc = new StubProductService { OnUpdate = (id, p) => throw new Exception("boom-update") };
            var controller = CreateController(svc);
            var prod = new Product { Title = "X", Price = 1 };

            var result = controller.Put(1, prod);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("boom-update", obj.Value);
        }

        [Fact]
        public void Delete_WhenServiceThrows_ReturnsInternalServer()
        {
            var svc = new StubProductService { OnDelete = id => throw new Exception("boom-delete") };
            var controller = CreateController(svc);

            var result = controller.Delete(1);
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("boom-delete", obj.Value);
        }
    }
}
