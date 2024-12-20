using fast_authenticator.context;
using fast_auth.service;
using Microsoft.EntityFrameworkCore;
using fast_auth.util;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:80");

// Ajouter les services nécessaires à l'application

// Enregistrer MyDbContext pour l'injection de dépendances
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Enregistrer AppService comme service injectable
builder.Services.AddScoped<AppService>();

// Ajouter les contrôleurs
builder.Services.AddControllers(options =>
{
    options.Filters.Add<TokenValidationFilter>();
});

// Ajouter la configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()  // Permet toutes les origines (ajuster selon ton besoin)
               .AllowAnyMethod()  // Permet toutes les méthodes (GET, POST, etc.)
               .AllowAnyHeader(); // Permet tous les headers
    });
});

// Configurer Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Fast_Auth API", Version = "v1" });
});

var app = builder.Build();

// Configurer le pipeline des requêtes HTTP
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

// Appliquer la politique CORS
app.UseCors("AllowAll"); // Applique la politique CORS définie ci-dessus

app.UseAuthorization();

app.MapControllers();

app.Run();
