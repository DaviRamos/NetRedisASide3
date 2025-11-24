# Exemplos Avançados - NetRedisASide3

## 📚 Integração com Ollama para IA Generativa

### Exemplo 1: Gerar Descrições Automáticas com LLM

```csharp
// Services/OllamaService.cs
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaService> _logger;
    private const string OllamaBaseUrl = "http://localhost:11434";

    public OllamaService(HttpClient httpClient, ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GenerateDescriptionAsync(string titulo)
    {
        var request = new
        {
            model = "llama2",
            prompt = $"Gere uma descrição profissional e concisa para o seguinte assunto: {titulo}",
            stream = false
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{OllamaBaseUrl}/api/generate", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(result);
        
        return jsonDoc.RootElement.GetProperty("response").GetString() ?? string.Empty;
    }

    public async Task<double[]> GenerateEmbeddingAsync(string text)
    {
        var request = new
        {
            model = "all-minilm",
            prompt = text
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{OllamaBaseUrl}/api/embeddings", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(result);
        
        var embeddingArray = jsonDoc.RootElement.GetProperty("embedding");
        var embedding = new List<double>();
        
        foreach (var item in embeddingArray.EnumerateArray())
        {
            embedding.Add(item.GetDouble());
        }
        
        return embedding.ToArray();
    }
}
```

### Uso no Endpoint:

```csharp
// Endpoints/AssuntoEndpoints.cs - Adicionar endpoint AI
group.MapPost("/generate-description", async (
    [FromBody] GenerateDescriptionRequest request,
    OllamaService ollamaService) =>
{
    var description = await ollamaService.GenerateDescriptionAsync(request.Titulo);
    return Results.Ok(new { descricao = description });
})
.WithName("GenerateAssuntoDescription")
.WithOpenApi();

public record GenerateDescriptionRequest(string Titulo);
```

---

## 🧠 Integração com Weaviate para Busca Semântica

### Exemplo 2: Busca Semântica de Documentos

```csharp
// Services/WeaviateService.cs
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class WeaviateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeaviateService> _logger;
    private const string WeaviateBaseUrl = "http://localhost:8081";

    public WeaviateService(HttpClient httpClient, ILogger<WeaviateService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> CreateSchemaAsync()
    {
        var schema = new
        {
            @class = "Assunto",
            vectorizer = "text2vec-ollama",
            moduleConfig = new
            {
                textVecOllama = new
                {
                    model = "all-minilm",
                    apiEndpoint = "http://ollama:11434"
                }
            },
            properties = new[]
            {
                new { name = "titulo", dataType = new[] { "string" } },
                new { name = "descricao", dataType = new[] { "text" } },
                new { name = "assuntoId", dataType = new[] { "int" } }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(schema),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{WeaviateBaseUrl}/v1/schema", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IndexAssuntoAsync(int id, string titulo, string descricao)
    {
        var document = new
        {
            @class = "Assunto",
            properties = new
            {
                titulo,
                descricao,
                assuntoId = id
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(document),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{WeaviateBaseUrl}/v1/objects", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<SemanticSearchResult>> SearchAsync(string query, int limit = 5)
    {
        var graphqlQuery = @$"{{
            Get {{
                Assunto(
                    nearText: {{
                        concepts: [""{query}""]
                    }}
                    limit: {limit}
                ) {{
                    titulo
                    descricao
                    assuntoId
                    _additional {{
                        distance
                        certainty
                    }}
                }}
            }}
        }}";

        var request = new { query = graphqlQuery };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{WeaviateBaseUrl}/v1/graphql", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(result);

        var results = new List<SemanticSearchResult>();
        var assuntos = jsonDoc.RootElement
            .GetProperty("data")
            .GetProperty("Get")
            .GetProperty("Assunto");

        foreach (var item in assuntos.EnumerateArray())
        {
            results.Add(new SemanticSearchResult
            {
                AssuntoId = item.GetProperty("assuntoId").GetInt32(),
                Titulo = item.GetProperty("titulo").GetString() ?? string.Empty,
                Descricao = item.GetProperty("descricao").GetString() ?? string.Empty,
                Distance = item.GetProperty("_additional").GetProperty("distance").GetDouble(),
                Certainty = item.GetProperty("_additional").GetProperty("certainty").GetDouble()
            });
        }

        return results;
    }
}

public record SemanticSearchResult
{
    public int AssuntoId { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public double Distance { get; init; }
    public double Certainty { get; init; }
}
```

### Uso no Endpoint:

```csharp
// Endpoints/AssuntoEndpoints.cs - Adicionar busca semântica
group.MapGet("/search-semantic", async (
    [FromQuery] string query,
    [FromQuery] int limit,
    WeaviateService weaviateService) =>
{
    var results = await weaviateService.SearchAsync(query, limit);
    return Results.Ok(results);
})
.WithName("SearchAssuntosSemantica")
.WithOpenApi();
```

---

## 🔄 Exemplo 3: Pipeline Completo com IA

Criar um endpoint que:
1. Recebe um título
2. Gera descrição com Ollama
3. Salva no banco
4. Indexa no Weaviate
5. Invalida cache

```csharp
// Endpoints/AssuntoEndpoints.cs
group.MapPost("/ai-enhanced", async (
    [FromBody] CreateAssuntoAIRequest request,
    AssuntoService assuntoService,
    OllamaService ollamaService,
    WeaviateService weaviateService,
    IValidator<Assunto> validator) =>
{
    try
    {
        // 1. Gerar descrição com IA
        var descricaoGerada = await ollamaService.GenerateDescriptionAsync(request.Nome);

        // 2. Criar modelo
        var assunto = new Assunto
        {
            Nome = request.Nome,
            Descricao = string.IsNullOrEmpty(request.Descricao) 
                ? descricaoGerada 
                : request.Descricao
        };

        // 3. Validar
        var validationResult = await validator.ValidateAsync(assunto);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        // 4. Salvar no banco (invalida cache automaticamente)
        var created = await assuntoService.AddAsync(assunto);

        // 5. Indexar no Weaviate para busca semântica
        await weaviateService.IndexAssuntoAsync(
            created.Id,
            created.Nome,
            created.Descricao);

        return Results.Created($"/api/assuntos/{created.Id}", new
        {
            assunto = created,
            descricaoGeradaPorIA = string.IsNullOrEmpty(request.Descricao)
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Erro ao criar assunto com IA",
            detail: ex.Message,
            statusCode: 500);
    }
})
.WithName("CreateAssuntoAIEnhanced")
.WithOpenApi();

public record CreateAssuntoAIRequest(string Nome, string? Descricao = null);
```

---

## 📊 Exemplo 4: Analytics com Cache e Agregações

```csharp
// Services/AnalyticsService.cs
public class AnalyticsService
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache;
    private const string StatsCacheKey = "analytics:stats";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

    public AnalyticsService(AppDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<SystemStats> GetSystemStatsAsync()
    {
        // Tentar obter do cache
        var cachedData = await _cache.GetStringAsync(StatsCacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<SystemStats>(cachedData)!;
        }

        // Calcular stats
        var stats = new SystemStats
        {
            TotalAssuntos = await _context.Assuntos.CountAsync(),
            TotalMovimentacoes = await _context.Movimentacoes.CountAsync(),
            TotalTiposDocumento = await _context.TiposDocumento.CountAsync(),
            AssuntosMaisRecentes = await _context.Assuntos
                .OrderByDescending(a => a.DataCriacao)
                .Take(5)
                .Select(a => a.Nome)
                .ToListAsync(),
            DataUltimaAtualizacao = DateTime.UtcNow
        };

        // Salvar no cache
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration
        };

        await _cache.SetStringAsync(
            StatsCacheKey,
            JsonSerializer.Serialize(stats),
            options);

        return stats;
    }

    public async Task InvalidateStatsCache()
    {
        await _cache.RemoveAsync(StatsCacheKey);
    }
}

public record SystemStats
{
    public int TotalAssuntos { get; init; }
    public int TotalMovimentacoes { get; init; }
    public int TotalTiposDocumento { get; init; }
    public List<string> AssuntosMaisRecentes { get; init; } = new();
    public DateTime DataUltimaAtualizacao { get; init; }
}
```

### Endpoint de Analytics:

```csharp
// Program.cs - Registrar serviço
builder.Services.AddScoped<AnalyticsService>();

// Endpoints/AnalyticsEndpoints.cs
public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics")
            .WithTags("Analytics")
            .RequireAuthorization();

        group.MapGet("/stats", async (AnalyticsService service) =>
        {
            var stats = await service.GetSystemStatsAsync();
            return Results.Ok(stats);
        })
        .WithName("GetSystemStats")
        .WithOpenApi();
    }
}
```

---

## 🔐 Exemplo 5: Auditoria e Logging Avançado

```csharp
// Services/AuditService.cs
public class AuditService
{
    private readonly ILogger<AuditService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        ILogger<AuditService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public void LogOperation(string operation, string entityType, int? entityId, object? data = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User?.FindFirst("sub")?.Value ?? "anonymous";
        var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";

        _logger.LogInformation(
            "Audit: Operation={Operation}, Entity={EntityType}, EntityId={EntityId}, " +
            "UserId={UserId}, IP={IpAddress}, Data={Data}",
            operation,
            entityType,
            entityId,
            userId,
            ipAddress,
            JsonSerializer.Serialize(data));
    }
}
```

### Integrar no Service:

```csharp
// Services/AssuntoService.cs - Adicionar auditoria
public class AssuntoService
{
    private readonly IRepository<Assunto> _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AssuntoService> _logger;
    private readonly AuditService _auditService; // ← NOVO

    public AssuntoService(
        IRepository<Assunto> repository,
        IDistributedCache cache,
        ILogger<AssuntoService> logger,
        AuditService auditService) // ← NOVO
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
        _auditService = auditService; // ← NOVO
    }

    public async Task<Assunto> AddAsync(Assunto assunto)
    {
        var result = await _repository.AddAsync(assunto);
        await _cache.RemoveAsync(CacheKeyAll);
        
        // ← NOVO: Auditoria
        _auditService.LogOperation("CREATE", "Assunto", result.Id, new { result.Nome });
        
        _logger.LogInformation("Cache de listagem invalidado após criação do assunto {Id}", result.Id);
        return result;
    }

    // Aplicar em Update e Delete também...
}
```

---

## ⚡ Exemplo 6: Rate Limiting Personalizado

```csharp
// Middleware/RateLimitMiddleware.cs
using System.Collections.Concurrent;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, (DateTime, int)> _requests = new();
    private const int MaxRequests = 100;
    private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(1);

    public RateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var identifier = context.User?.FindFirst("sub")?.Value 
            ?? context.Connection.RemoteIpAddress?.ToString() 
            ?? "unknown";

        var now = DateTime.UtcNow;
        
        if (_requests.TryGetValue(identifier, out var requestInfo))
        {
            var (lastRequest, count) = requestInfo;
            
            if (now - lastRequest < TimeWindow)
            {
                if (count >= MaxRequests)
                {
                    context.Response.StatusCode = 429; // Too Many Requests
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Rate limit exceeded",
                        message = $"Maximum {MaxRequests} requests per minute"
                    });
                    return;
                }
                
                _requests[identifier] = (lastRequest, count + 1);
            }
            else
            {
                _requests[identifier] = (now, 1);
            }
        }
        else
        {
            _requests[identifier] = (now, 1);
        }

        await _next(context);
    }
}

// Program.cs - Registrar middleware
app.UseMiddleware<RateLimitMiddleware>();
```

---

## 🎯 Exemplo 7: Background Jobs com Hosted Service

```csharp
// Services/CacheWarmupService.cs
public class CacheWarmupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheWarmupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public CacheWarmupService(
        IServiceProvider serviceProvider,
        ILogger<CacheWarmupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cache Warmup Service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await WarmupCacheAsync();
                _logger.LogInformation("Cache warmup completado. Próxima execução em {Interval}", _interval);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante cache warmup");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task WarmupCacheAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        
        var assuntoService = scope.ServiceProvider.GetRequiredService<AssuntoService>();
        var movimentacaoService = scope.ServiceProvider.GetRequiredService<MovimentacaoService>();
        var tipoDocumentoService = scope.ServiceProvider.GetRequiredService<TipoDocumentoService>();

        // Pré-carregar dados no cache
        await assuntoService.GetAllAsync();
        await movimentacaoService.GetAllAsync();
        await tipoDocumentoService.GetAllAsync();

        _logger.LogInformation("Cache aquecido com sucesso");
    }
}

// Program.cs - Registrar hosted service
builder.Services.AddHostedService<CacheWarmupService>();
```

---

## 📧 Exemplo 8: Notificações em Tempo Real com SignalR

```csharp
// Hubs/NotificationHub.cs
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    public async Task SendNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }

    public async Task NotifyEntityCreated(string entityType, int id)
    {
        await Clients.All.SendAsync("EntityCreated", new
        {
            Type = entityType,
            Id = id,
            Timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}

// Program.cs - Registrar SignalR
builder.Services.AddSignalR();

// Mapear hub
app.MapHub<NotificationHub>("/hubs/notifications");

// Services/AssuntoService.cs - Integrar com SignalR
public class AssuntoService
{
    private readonly IRepository<Assunto> _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AssuntoService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext; // ← NOVO

    public AssuntoService(
        IRepository<Assunto> repository,
        IDistributedCache cache,
        ILogger<AssuntoService> logger,
        IHubContext<NotificationHub> hubContext) // ← NOVO
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<Assunto> AddAsync(Assunto assunto)
    {
        var result = await _repository.AddAsync(assunto);
        await _cache.RemoveAsync(CacheKeyAll);
        
        // ← NOVO: Notificar via SignalR
        await _hubContext.Clients.All.SendAsync("EntityCreated", new
        {
            Type = "Assunto",
            Id = result.Id,
            Nome = result.Nome,
            Timestamp = DateTime.UtcNow
        });
        
        _logger.LogInformation("Assunto {Id} criado e notificação enviada", result.Id);
        return result;
    }
}
```

### Cliente JavaScript para SignalR:

```html
<!-- wwwroot/index.html -->
<!DOCTYPE html>
<html>
<head>
    <title>NetRedisASide3 - Notifications</title>
    <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.0/dist/browser/signalr.min.js"></script>
</head>
<body>
    <h1>Real-time Notifications</h1>
    <ul id="notifications"></ul>

    <script>
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/notifications")
            .configureLogging(signalR.LogLevel.Information)
            .build();

        connection.on("EntityCreated", (data) => {
            const li = document.createElement("li");
            li.textContent = `${data.type} #${data.id} criado: ${data.nome} às ${data.timestamp}`;
            document.getElementById("notifications").appendChild(li);
        });

        connection.start()
            .then(() => console.log("Connected to SignalR"))
            .catch(err => console.error(err));
    </script>
</body>
</html>
```

---

## 🔄 Exemplo 9: Retry Policy com Polly

```csharp
// Program.cs - Adicionar pacote
// dotnet add package Microsoft.Extensions.Http.Polly

// Configurar HttpClient com retry policy
builder.Services.AddHttpClient<OllamaService>()
    .AddTransientHttpErrorPolicy(policyBuilder => 
        policyBuilder.WaitAndRetryAsync(
            3, 
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} após {timespan.TotalSeconds}s");
            }));

builder.Services.AddHttpClient<WeaviateService>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)));
```

---

## 📦 Exemplo 10: Export de Dados (CSV/Excel)

```csharp
// Services/ExportService.cs
using System.Text;

public class ExportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ExportService> _logger;

    public ExportService(AppDbContext context, ILogger<ExportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<byte[]> ExportAssuntosToCsvAsync()
    {
        var assuntos = await _context.Assuntos
            .OrderBy(a => a.Nome)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Id,Nome,Descricao,DataCriacao,DataAtualizacao");

        foreach (var assunto in assuntos)
        {
            csv.AppendLine($"{assunto.Id}," +
                          $"\"{assunto.Nome}\"," +
                          $"\"{assunto.Descricao}\"," +
                          $"{assunto.DataCriacao:yyyy-MM-dd HH:mm:ss}," +
                          $"{assunto.DataAtualizacao:yyyy-MM-dd HH:mm:ss}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportAssuntosToJsonAsync()
    {
        var assuntos = await _context.Assuntos
            .OrderBy(a => a.Nome)
            .ToListAsync();

        var json = JsonSerializer.Serialize(assuntos, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return Encoding.UTF8.GetBytes(json);
    }
}

// Endpoints/ExportEndpoints.cs
public static class ExportEndpoints
{
    public static void MapExportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/export")
            .WithTags("Export")
            .RequireAuthorization();

        group.MapGet("/assuntos/csv", async (ExportService service) =>
        {
            var csv = await service.ExportAssuntosToCsvAsync();
            return Results.File(
                csv,
                "text/csv",
                $"assuntos_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        })
        .WithName("ExportAssuntosCsv")
        .WithOpenApi();

        group.MapGet("/assuntos/json", async (ExportService service) =>
        {
            var json = await service.ExportAssuntosToJsonAsync();
            return Results.File(
                json,
                "application/json",
                $"assuntos_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        })
        .WithName("ExportAssuntosJson")
        .WithOpenApi();
    }
}
```

---

## 🔍 Exemplo 11: Busca Avançada com Filtros

```csharp
// Models/AssuntoFilter.cs
public class AssuntoFilter
{
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public DateTime? DataCriacaoInicio { get; set; }
    public DateTime? DataCriacaoFim { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? OrderBy { get; set; } = "Nome";
    public bool Descending { get; set; } = false;
}

// Services/AssuntoService.cs - Adicionar método de busca
public async Task<PagedResult<Assunto>> SearchAsync(AssuntoFilter filter)
{
    var query = _context.Assuntos.AsNoTracking().AsQueryable();

    // Aplicar filtros
    if (!string.IsNullOrEmpty(filter.Nome))
    {
        query = query.Where(a => a.Nome.Contains(filter.Nome));
    }

    if (!string.IsNullOrEmpty(filter.Descricao))
    {
        query = query.Where(a => a.Descricao.Contains(filter.Descricao));
    }

    if (filter.DataCriacaoInicio.HasValue)
    {
        query = query.Where(a => a.DataCriacao >= filter.DataCriacaoInicio.Value);
    }

    if (filter.DataCriacaoFim.HasValue)
    {
        query = query.Where(a => a.DataCriacao <= filter.DataCriacaoFim.Value);
    }

    // Total de registros
    var totalItems = await query.CountAsync();

    // Ordenação
    query = filter.OrderBy?.ToLower() switch
    {
        "nome" => filter.Descending 
            ? query.OrderByDescending(a => a.Nome) 
            : query.OrderBy(a => a.Nome),
        "datacriacao" => filter.Descending 
            ? query.OrderByDescending(a => a.DataCriacao) 
            : query.OrderBy(a => a.DataCriacao),
        _ => query.OrderBy(a => a.Nome)
    };

    // Paginação
    var items = await query
        .Skip((filter.PageNumber - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .ToListAsync();

    return new PagedResult<Assunto>
    {
        Items = items,
        TotalItems = totalItems,
        PageNumber = filter.PageNumber,
        PageSize = filter.PageSize,
        TotalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize)
    };
}

public record PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalItems { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

// Endpoints/AssuntoEndpoints.cs - Adicionar busca
group.MapPost("/search", async (
    [FromBody] AssuntoFilter filter,
    AssuntoService service) =>
{
    var result = await service.SearchAsync(filter);
    return Results.Ok(result);
})
.WithName("SearchAssuntos")
.WithOpenApi();
```

---

## 🎨 Exemplo 12: Response Compression

```csharp
// Program.cs - Adicionar compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.SmallestSize;
});

// Usar middleware
app.UseResponseCompression();
```

---

## 🔐 Exemplo 13: API Versioning

```csharp
// Program.cs - Adicionar pacote
// dotnet add package Asp.Versioning.Http

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// Endpoints/AssuntoEndpoints.cs - Versionar endpoints
public static void MapAssuntoEndpointsV1(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/v1/assuntos")
        .WithTags("Assuntos V1")
        .RequireAuthorization();

    // Endpoints da versão 1...
}

public static void MapAssuntoEndpointsV2(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/v2/assuntos")
        .WithTags("Assuntos V2")
        .RequireAuthorization();

    // Endpoints da versão 2 com melhorias...
}
```

---

## 📊 Exemplo 14: Métricas com Prometheus

```csharp
// Program.cs - Adicionar pacote
// dotnet add package prometheus-net.AspNetCore

using Prometheus;

// Criar métricas customizadas
var requestCounter = Metrics.CreateCounter(
    "api_requests_total",
    "Total de requisições da API",
    new CounterConfiguration
    {
        LabelNames = new[] { "method", "endpoint", "status" }
    });

var requestDuration = Metrics.CreateHistogram(
    "api_request_duration_seconds",
    "Duração das requisições da API",
    new HistogramConfiguration
    {
        LabelNames = new[] { "method", "endpoint" }
    });

// Middleware para coletar métricas
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "/";
    var method = context.Request.Method;

    using (requestDuration.WithLabels(method, path).NewTimer())
    {
        await next();
    }

    requestCounter.WithLabels(
        method,
        path,
        context.Response.StatusCode.ToString()
    ).Inc();
});

// Expor métricas
app.MapMetrics();
```

---

## 🧪 Exemplo 15: Testes de Integração

```csharp
// Tests/IntegrationTests/AssuntoEndpointsTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

public class AssuntoEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AssuntoEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/api/assuntos");

        // Assert
        response.EnsureSuccessStatusCode();
        var assuntos = await response.Content.ReadFromJsonAsync<List<Assunto>>();
        Assert.NotNull(assuntos);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAssunto()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var newAssunto = new
        {
            nome = "Test Assunto",
            descricao = "Test Description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/assuntos", newAssunto);

        // Assert
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<Assunto>();
        Assert.NotNull(created);
        Assert.Equal("Test Assunto", created.Nome);
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var tokenResponse = await _client.PostAsync(
            "http://localhost:8080/realms/netredisaside3/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "netredisaside3-api",
                ["client_secret"] = "netredisaside3-secret-change-in-production",
                ["username"] = "admin",
                ["password"] = "admin123"
            }));

        var tokenData = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenData?.AccessToken ?? string.Empty;
    }

    private record TokenResponse(string AccessToken);
}
```

---

## 🎯 Resumo dos Exemplos

| # | Exemplo | Tecnologia | Complexidade |
|---|---------|------------|--------------|
| 1 | Geração de IA | Ollama | ⭐⭐⭐ |
| 2 | Busca Semântica | Weaviate | ⭐⭐⭐⭐ |
| 3 | Pipeline Completo | Ollama + Weaviate | ⭐⭐⭐⭐⭐ |
| 4 | Analytics | Cache + Agregações | ⭐⭐⭐ |
| 5 | Auditoria | Logging | ⭐⭐ |
| 6 | Rate Limiting | Middleware | ⭐⭐⭐ |
| 7 | Background Jobs | Hosted Service | ⭐⭐⭐ |
| 8 | Real-time | SignalR | ⭐⭐⭐⭐ |
| 9 | Retry Policy | Polly | ⭐⭐ |
| 10 | Export Dados | CSV/JSON | ⭐⭐ |
| 11 | Busca Avançada | Filtros + Paginação | ⭐⭐⭐ |
| 12 | Compression | Brotli/Gzip | ⭐ |
| 13 | Versioning | API Versioning | ⭐⭐ |
| 14 | Métricas | Prometheus | ⭐⭐⭐ |
| 15 | Testes | xUnit | ⭐⭐⭐ |

---

## 📚 Próximos Passos

1. Escolha os exemplos relevantes para seu caso de uso
2. Implemente progressivamente
3. Teste cada integração isoladamente
4. Monitore performance e logs
5. Documente suas personalizações

**💡 Dica:** Comece pelos exemplos mais simples e vá aumentando a complexidade conforme ganha confiança!