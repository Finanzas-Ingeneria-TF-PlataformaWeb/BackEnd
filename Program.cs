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

// CORS para el front en 5173 / 5174
const string AllowedOrigins = "_allowedOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowedOrigins, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174"
                // Cuando tengas el front desplegado, agrega aquí:
                // "https://tu-frontend.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Servicios de dominio
builder.Services.AddScoped<LoanCalculatorService>();

var app = builder.Build();

// Habilitar Swagger SIEMPRE (Dev y Producción/Render)
app.UseSwagger();
app.UseSwaggerUI();

// CORS
app.UseCors(AllowedOrigins);

// Servir archivos estáticos desde wwwroot
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