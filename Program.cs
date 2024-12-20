using fast_authenticator.context;
using fast_auth.service;
using Microsoft.EntityFrameworkCore;
using fast_auth.util;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:80");

// Add services to the container.

// Enregistrer MyDbContext pour l'injection de dépendances
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Enregistrer AppService comme service injectable
builder.Services.AddScoped<AppService>();

// Ajouter les contrôleurs
//builder.Services.AddControllers();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<TokenValidationFilter>();
});

// Configurer Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Fast_Auth API", Version = "v1" });
});

var app = builder.Build();

// Configure le pipeline des requêtes HTTP
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fast_Auth API v1");
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
