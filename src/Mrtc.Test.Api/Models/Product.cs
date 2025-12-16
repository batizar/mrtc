using System.ComponentModel.DataAnnotations;
using System.Text.Json;

using Destructurama.Attributed;

namespace Mrtc.Test.Api.Models;

public class Product : IValidatableObject
{
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public int? Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    public required string Title { get; set; }

    [StringLength(100, ErrorMessage = "Description length can't be more than 100.")]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public decimal? Rating { get; set; }

    public int? Stock { get; set; }

    public string? Brand { get; set; }

    public string? Category { get; set; }

    [NotLogged]
    public string? Thumbnail { get; set; }

    [NotLogged]
    public List<string>? Images { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var env = validationContext.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
        if (env == null)
        {
            yield break;
        }

        var filePath = Path.Combine(env.ContentRootPath, "test_products.json");
        if (!File.Exists(filePath))
        {
            yield break;
        }

        var isDuplicateFound = false;

        try
        {
            var json = File.ReadAllText(filePath);
            var payload = JsonSerializer.Deserialize<ProductsPayload>(json, CachedJsonSerializerOptions);

            var titleNormalized = (Title ?? string.Empty).Trim();
            var brandNormalized = (Brand ?? string.Empty).Trim();

            isDuplicateFound = payload?.Products?.Any(p => string.Equals(p.Title.Trim(), titleNormalized, StringComparison.OrdinalIgnoreCase)
               && string.Equals((p.Brand ?? string.Empty).Trim(), brandNormalized, StringComparison.OrdinalIgnoreCase)
               && p.Id != Id) ?? false;           
        }
        catch (JsonException)
        {
            yield break;
        }
        catch
        {
            yield break;
        }

        if (isDuplicateFound)
        {
            yield return new ValidationResult("A product with the same title and brand already exists.", [nameof(Title), nameof(Brand)]);
        }
    }
}
