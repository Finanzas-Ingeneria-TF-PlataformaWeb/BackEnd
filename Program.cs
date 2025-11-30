using Microsoft.EntityFrameworkCore;
using MiVivienda.Api.Data;
using MiVivienda.Api.Services;
// Es probable que necesites agregar esto si usas Npgsql
// using Npgsql.EntityFrameworkCore.PostgreSQL; 

var builder = WebApplication.CreateBuilder(args);

// Controllers y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🟢 DbContext con POSTGRESQL (Para Render) 🟢
// El framework buscará la variable de entorno 'ConnectionStrings__DefaultConnection'
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        // Obtiene la cadena de la configuración (appsettings.json o Environment Variable)
        builder.Configuration.GetConnectionString("DefaultConnection")
        // Como alternativa de desarrollo local, si la cadena no existe, usa una cadena local
        ?? "Host=localhost;Database=local_db;Username=postgres;Password=yourpassword"));

// ... (El resto del código se mantiene igual)

const string AllowedOrigins = "_allowedOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowedOrigins, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Servicios de dominio
builder.Services.AddScoped<LoanCalculatorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS
app.UseCors(AllowedOrigins);

// NUEVO: servir archivos estáticos desde wwwroot
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Asegúrate de que tus migraciones son válidas para PostgreSQL.
    db.Database.Migrate(); 
}

app.Run();
