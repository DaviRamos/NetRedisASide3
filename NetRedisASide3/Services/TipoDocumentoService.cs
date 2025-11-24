using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NetRedisASide3.Models;
using NetRedisASide3.Repositories;

namespace NetRedisASide3.Services;

public class TipoDocumentoService
{
    private readonly IRepository<TipoDocumento> _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TipoDocumentoService> _logger;
    private const string CacheKeyPrefix = "tipodocumento:";
    private const string CacheKeyAll = "tiposdocumento:all";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public TipoDocumentoService(
        IRepository<TipoDocumento> repository,
        IDistributedCache cache,
        ILogger<TipoDocumentoService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<TipoDocumento>> GetAllAsync()
    {
        var cachedData = await _cache.GetStringAsync(CacheKeyAll);
        
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Retornando tipos de documento do cache");
            return JsonSerializer.Deserialize<IEnumerable<TipoDocumento>>(cachedData) ?? Enumerable.Empty<TipoDocumento>();
        }

        var tipos = await _repository.GetAllAsync();
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration
        };
        
        await _cache.SetStringAsync(
            CacheKeyAll,
            JsonSerializer.Serialize(tipos),
            options);

        _logger.LogInformation("Tipos de documento carregados do banco e salvos no cache");
        
        return tipos;
    }

    public async Task<TipoDocumento?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}{id}";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Retornando tipo de documento {Id} do cache", id);
            return JsonSerializer.Deserialize<TipoDocumento>(cachedData);
        }

        var tipo = await _repository.GetByIdAsync(id);
        
        if (tipo != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            };
            
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(tipo),
                options);

            _logger.LogInformation("Tipo de documento {Id} carregado do banco e salvo no cache", id);
        }
        
        return tipo;
    }

    public async Task<TipoDocumento> AddAsync(TipoDocumento tipo)
    {
        var result = await _repository.AddAsync(tipo);
        
        await _cache.RemoveAsync(CacheKeyAll);
        _logger.LogInformation("Cache de listagem invalidado após criação do tipo de documento {Id}", result.Id);
        
        return result;
    }

    public async Task<TipoDocumento> UpdateAsync(TipoDocumento tipo)
    {
        var result = await _repository.UpdateAsync(tipo);
        
        var cacheKey = $"{CacheKeyPrefix}{tipo.Id}";
        await _cache.RemoveAsync(cacheKey);
        await _cache.RemoveAsync(CacheKeyAll);
        
        _logger.LogInformation("Cache invalidado após atualização do tipo de documento {Id}", tipo.Id);
        
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
            
            _logger.LogInformation("Cache invalidado após exclusão do tipo de documento {Id}", id);
        }
        
        return result;
    }
}