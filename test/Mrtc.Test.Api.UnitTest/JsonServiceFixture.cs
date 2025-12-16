using Microsoft.Extensions.Options;

using Mrtc.Test.Api.Services;

namespace Mrtc.Test.Api.UnitTest
{
    public sealed class JsonServiceFixture : IDisposable
    {
        private readonly List<string> _dirs = new();

        public (JsonProductService service, string dir) CreateServiceWithJson(string json)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var filePath = Path.Combine(tempDir, "test_products.json");
            File.WriteAllText(filePath, json);

            var env = new TestWebHostEnvironment { ContentRootPath = tempDir };
            var jsOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();
            jsOptions.SerializerOptions.PropertyNameCaseInsensitive = true;
            var jsonOptions = Options.Create(jsOptions);

            var service = new JsonProductService(env, jsonOptions);
            _dirs.Add(tempDir);
            return (service, tempDir);
        }

        public (JsonProductService service, string dir) CreateServiceWithoutFile()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var env = new TestWebHostEnvironment { ContentRootPath = tempDir };
            var jsOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();
            jsOptions.SerializerOptions.PropertyNameCaseInsensitive = true;
            var jsonOptions = Options.Create(jsOptions);

            var service = new JsonProductService(env, jsonOptions);
            _dirs.Add(tempDir);
            return (service, tempDir);
        }

        public void Dispose()
        {
            foreach (var d in _dirs)
            {
                try { if (Directory.Exists(d)) Directory.Delete(d, true); } catch { }
            }
        }
    }
}
