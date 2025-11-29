using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MiVivienda.Api.Data;
using MiVivienda.Api.Models;

namespace MiVivienda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public PropertiesController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Property>>> GetAll()
    {
        var properties = await _context.Properties
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(properties);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Property>> GetById(int id)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property == null) return NotFound();
        return Ok(property);
    }

    [HttpPost]
    public async Task<ActionResult<Property>> Create(Property property)
    {
        property.Id = 0;
        property.CreatedAt = DateTime.UtcNow;

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = property.Id }, property);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Property>> Update(int id, Property input)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property == null) return NotFound();

        property.Code = input.Code;
        property.Name = input.Name;
        property.Address = input.Address;
        property.District = input.District;
        property.Price = input.Price;
        property.Currency = input.Currency;
        property.AreaM2 = input.AreaM2;
        property.Bedrooms = input.Bedrooms;
        property.Bathrooms = input.Bathrooms;
        property.ImageUrl = input.ImageUrl;

        await _context.SaveChangesAsync();
        return Ok(property);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property == null) return NotFound();

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // NUEVO: endpoint para subir imagen de una unidad
    [HttpPost("{id:int}/image")]
    public async Task<ActionResult> UploadImage(int id, IFormFile file)
    {
        // 1. Buscar la propiedad
        var property = await _context.Properties.FindAsync(id);
        if (property == null)
        {
            return NotFound("Unidad inmobiliaria no encontrada.");
        }

        // 2. Validar archivo
        if (file == null || file.Length == 0)
        {
            return BadRequest("No se recibió ningún archivo.");
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("El archivo debe ser una imagen.");
        }

        const long maxSize = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxSize)
        {
            return BadRequest("La imagen es demasiado pesada. Máximo 5 MB.");
        }

        // 3. Ruta física a wwwroot/images/properties
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadsFolder = Path.Combine(webRoot, "images", "properties");
        Directory.CreateDirectory(uploadsFolder);

        // 4. Nombre de archivo único
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        // 5. Guardar archivo en disco
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 6. Guardar URL relativa en la entidad
        var relativeUrl = $"/images/properties/{fileName}";
        property.ImageUrl = relativeUrl;

        await _context.SaveChangesAsync();

        // 7. Devolver la URL al front
        return Ok(new { imageUrl = relativeUrl });
    }
}
