using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Mrtc.Test.Api.Models;
using Xunit;

namespace Mrtc.Test.Api.UnitTest.Models
{
    public class ProductTests
    {
        private static string CreateTempDirWithJson(string json)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "test_products.json"), json);
            return tempDir;
        }

        private class SimpleServiceProvider : IServiceProvider
        {
            private readonly object _service;
            public SimpleServiceProvider(object service) => _service = service;
            public object? GetService(Type serviceType) => serviceType == typeof(IWebHostEnvironment) ? _service : null;
        }

        [Fact]
        public void DataAnnotations_TitleRequired_Fails()
        {
            var p = new Product { Title = null!, Price = 1 };
            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(p);
            var valid = Validator.TryValidateObject(p, ctx, results, validateAllProperties: true);
            Assert.False(valid);
            Assert.Contains(results, r => r.MemberNames != null && r.MemberNames.Contains(nameof(Product.Title)));
        }

        [Fact]
        public void DataAnnotations_PriceRange_FailsForNegative()
        {
            var p = new Product { Title = "T", Price = -5 };
            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(p);
            var valid = Validator.TryValidateObject(p, ctx, results, validateAllProperties: true);
            Assert.False(valid);
            Assert.Contains(results, r => r.MemberNames != null && r.MemberNames.Contains(nameof(Product.Price)));
        }

        [Fact]
        public void DataAnnotations_PriceZero_AllowsZero()
        {
            var p = new Product { Title = "T", Price = 0m };
            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(p);
            var valid = Validator.TryValidateObject(p, ctx, results, validateAllProperties: true);
            Assert.True(valid);
            Assert.DoesNotContain(results, r => r.MemberNames != null && r.MemberNames.Contains(nameof(Product.Price)));
        }

        [Fact]
        public void DataAnnotations_DescriptionTooLong_Fails()
        {
            var longDesc = new string('x', 101);
            var p = new Product { Title = "T", Price = 1, Description = longDesc };
            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(p);
            var valid = Validator.TryValidateObject(p, ctx, results, validateAllProperties: true);
            Assert.False(valid);
            Assert.Contains(results, r => r.MemberNames != null && r.MemberNames.Contains(nameof(Product.Description)));
        }

        [Fact]
        public void Validate_NoEnvironment_ReturnsNoErrors()
        {
            var p = new Product { Title = "T", Price = 1 };
            var ctx = new ValidationContext(p); // no service provider -> env null
            var results = p.Validate(ctx).ToList();
            Assert.Empty(results);
        }

        [Fact]
        public void Validate_FileMissing_ReturnsNoErrors()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var env = new TestWebHostEnvironment { ContentRootPath = tempDir };
                var p = new Product { Title = "T", Price = 1 };
                var ctx = new ValidationContext(p, new SimpleServiceProvider(env), null);
                var results = p.Validate(ctx).ToList();
                Assert.Empty(results);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void Validate_DuplicateFound_ReturnsValidationResult()
        {
            var existing = new ProductsPayload { Products = new List<Product> { new Product { Id = 2, Title = "A ", Brand = "X" } } };
            var json = JsonSerializer.Serialize(existing, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var dir = CreateTempDirWithJson(json);
            try
            {
                var env = new TestWebHostEnvironment { ContentRootPath = dir };
                var p = new Product { Id = 3, Title = " A", Brand = "x", Price = 1 };
                var ctx = new ValidationContext(p, new SimpleServiceProvider(env), null);
                var results = p.Validate(ctx).ToList();
                Assert.Single(results);
                var vr = results[0];
                Assert.Equal("A product with the same title and brand already exists.", vr.ErrorMessage);
                Assert.Contains(nameof(Product.Title), vr.MemberNames);
                Assert.Contains(nameof(Product.Brand), vr.MemberNames);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Validate_SameId_NoDuplicate()
        {
            var existing = new ProductsPayload { Products = new List<Product> { new Product { Id = 5, Title = "Same", Brand = "B" } } };
            var json = JsonSerializer.Serialize(existing, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var dir = CreateTempDirWithJson(json);
            try
            {
                var env = new TestWebHostEnvironment { ContentRootPath = dir };
                var p = new Product { Id = 5, Title = "Same", Brand = "B", Price = 1 };
                var ctx = new ValidationContext(p, new SimpleServiceProvider(env), null);
                var results = p.Validate(ctx).ToList();
                Assert.Empty(results);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Validate_MalformedJson_NoErrors()
        {
            var dir = CreateTempDirWithJson("{ bad json }");
            try
            {
                var env = new TestWebHostEnvironment { ContentRootPath = dir };
                var p = new Product { Title = "T", Price = 1 };
                var ctx = new ValidationContext(p, new SimpleServiceProvider(env), null);
                var results = p.Validate(ctx).ToList();
                Assert.Empty(results);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
