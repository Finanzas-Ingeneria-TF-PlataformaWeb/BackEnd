namespace MiVivienda.Api.Models;

public class Property
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string District { get; set; } = null!;

    public decimal Price { get; set; }
    public string Currency { get; set; } = "PEN";

    public decimal AreaM2 { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string PropertyType { get; set; } = default!; // "apartment", "house", etc.

}