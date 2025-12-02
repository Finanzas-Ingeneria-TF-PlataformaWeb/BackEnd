using Microsoft.EntityFrameworkCore;
using MiVivienda.Api.Data;
using MiVivienda.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext con SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=mivivienda.db"));

// CORS (de momento abierto para probar desde cualquier origen)
const string CorsPolicy = "CorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .AllowAnyOrigin()   // luego lo restringimos, por ahora nos ayuda a ver si el problema era CORS
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Servicios de dominio
builder.Services.AddScoped<LoanCalculatorService>();

var app = builder.Build();

// Swagger (dev y prod)
app.UseSwagger();
app.UseSwaggerUI();

// Redirección a HTTPS (no hace daño en Render)
app.UseHttpsRedirection();

// CORS SIEMPRE antes de MapControllers
app.UseCors(CorsPolicy);

// Archivos estáticos (por si usas wwwroot)
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();