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

// === CORS ===
const string CorsPolicy = "CorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "https://frontend-fina.vercel.app",
                "https://frontend-fina-e6z3e6e7o-saebryxns-projects.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Servicios de dominio
builder.Services.AddScoped<LoanCalculatorService>();

var app = builder.Build();

// Swagger siempre
app.UseSwagger();
app.UseSwaggerUI();

// CORS antes de los controladores
app.UseCors(CorsPolicy);

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Migraciones automáticas
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();