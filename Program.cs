using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using NetRedisASide3.Data;
using NetRedisASide3.Models;
using NetRedisASide3.Repositories;
using NetRedisASide3.Services;
using NetRedisASide3.Validators;
using NetRedisASide3.Endpoints;
using NetRedisASide3.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configuração de Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configuração do PostgreSQL com User Secrets
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection")
    ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configuração do Redis (Cache Distribuído)
var redisConnection = builder.Configuration["RedisConnection"] ??  throw new InvalidOperationException("Connection string 'RedisConnection' not found.");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "NetRedisASide3:";
});


// Configuração do Keycloak (Autenticação JWT)
var keycloakSettings = builder.Configuration.GetSection("Keycloak").Get<KeycloakSettings>();
builder.Services.Configure<KeycloakSettings>(builder.Configuration.GetSection("Keycloak"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakSettings?.Authority;
        options.Audience = keycloakSettings?.Audience;
        options.MetadataAddress = keycloakSettings?.MetadataAddress ?? string.Empty;
        options.RequireHttpsMetadata = keycloakSettings?.RequireHttpsMetadata ?? false;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = keycloakSettings?.ValidateAudience ?? true,
            ValidateIssuer = keycloakSettings?.ValidateIssuer ?? true,
            ValidateLifetime = keycloakSettings?.ValidateLifetime ?? true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Registro de Repositórios
builder.Services.AddScoped<IRepository<Assunto>, AssuntoRepository>();
builder.Services.AddScoped<IRepository<Movimentacao>, MovimentacaoRepository>();
builder.Services.AddScoped<IRepository<TipoDocumento>, TipoDocumentoRepository>();

// Registro de Services
builder.Services.AddScoped<AssuntoService>();
builder.Services.AddScoped<MovimentacaoService>();
builder.Services.AddScoped<TipoDocumentoService>();

// Registro de Validators
builder.Services.AddScoped<IValidator<Assunto>, AssuntoValidator>();
builder.Services.AddScoped<IValidator<Movimentacao>, MovimentacaoValidator>();
builder.Services.AddScoped<IValidator<TipoDocumento>, TipoDocumentoValidator>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString ?? string.Empty,
        name: "postgres",
        tags: new[] { "database", "postgresql" })
    .AddRedis(
        redisConnection,
        name: "redis",
        tags: new[] { "cache", "redis" })
    .AddUrlGroup(
        new Uri(keycloakSettings?.Authority ?? "http://localhost:8080"),
        name: "keycloak",
        tags: new[] { "identity", "keycloak" })
    .AddUrlGroup(
        new Uri("http://localhost:11434/api/tags"),
        name: "ollama",
        tags: new[] { "ai", "ollama" })
    .AddUrlGroup(
        new Uri("http://localhost:8081/v1/.well-known/ready"),
        name: "weaviate",
        tags: new[] { "vector-db", "weaviate" });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "NetRedisASide3 API",
        Version = "v1",
        Description = "API para gestão de Assuntos, Movimentações e Tipos de Documento com cache distribuído Redis",
        Contact = new()
        {
            Name = "Equipe de Desenvolvimento",
            Email = "dev@netredisaside3.com"
        }
    });

    // Configuração de segurança JWT no Swagger
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu token}"
    });

    options.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();
/*
// Aplicar migrations automaticamente em desenvolvimento
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
*/


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "NetRedisASide3 API v1");
    options.RoutePrefix = "swagger";
});


//app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Health Checks Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("database") || check.Tags.Contains("cache")
});
app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false
});

// Mapeamento de Endpoints
app.MapAssuntoEndpoints();
app.MapMovimentacaoEndpoints();
app.MapTipoDocumentoEndpoints();

// Endpoint raiz
app.MapGet("/", () => Results.Ok(new
{
    application = "NetRedisASide3",
    version = "1.0.0",
    status = "running",
    documentation = "/swagger",
    health = "/health"
})).WithTags("Root").AllowAnonymous();


app.Run();
