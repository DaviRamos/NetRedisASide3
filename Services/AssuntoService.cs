// Services/AssuntoService.cs
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NetRedisASide3.Models;
using NetRedisASide3.Repositories;

namespace NetRedisASide3.Services;

public class AssuntoService
{
    private readonly IRepository<Assunto> _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AssuntoService> _logger;
    private const string CacheKeyPrefix = "assunto:";
    private const string CacheKeyAll = "assuntos:all";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public AssuntoService(
        IRepository<Assunto> repository,
        IDistributedCache cache,
        ILogger<AssuntoService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<Assunto>> GetAllAsync()
    {
        var cachedData = await _cache.GetStringAsync(CacheKeyAll);
        
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Retornando assuntos do cache");
            return JsonSerializer.Deserialize<IEnumerable<Assunto>>(cachedData) ?? Enumerable.Empty<Assunto>();
        }

        var assuntos = await _repository.GetAllAsync();
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration
        };
        
        await _cache.SetStringAsync(
            CacheKeyAll,
            JsonSerializer.Serialize(assuntos),
            options);

        _logger.LogInformation("Assuntos carregados do banco e salvos no cache");
        
        return assuntos;
    }

    public async Task<Assunto?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}{id}";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Retornando assunto {Id} do cache", id);
            return JsonSerializer.Deserialize<Assunto>(cachedData);
        }

        var assunto = await _repository.GetByIdAsync(id);
        
        if (assunto != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            };
            
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(assunto),
                options);

            _logger.LogInformation("Assunto {Id} carregado do banco e salvo no cache", id);
        }
        
        return assunto;
    }

    public async Task<Assunto> AddAsync(Assunto assunto)
    {
        var result = await _repository.AddAsync(assunto);
        
        await _cache.RemoveAsync(CacheKeyAll);
        _logger.LogInformation("Cache de listagem invalidado após criação do assunto {Id}", result.Id);
        
        return result;
    }

    public async Task<Assunto> UpdateAsync(Assunto assunto)
    {
        var result = await _repository.UpdateAsync(assunto);
        
        var cacheKey = $"{CacheKeyPrefix}{assunto.Id}";
        await _cache.RemoveAsync(cacheKey);
        await _cache.RemoveAsync(CacheKeyAll);
        
        _logger.LogInformation("Cache invalidado após atualização do assunto {Id}", assunto.Id);
        
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _repository.DeleteAsync(id);
        
        if (result)
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            await _cache.RemoveAsync(cacheKey);
            await _cache.RemoveAsync(CacheKeyAll);
            
            _logger.LogInformation("Cache invalidado após exclusão do assunto {Id}", id);
        }
        
        return result;
    }
}