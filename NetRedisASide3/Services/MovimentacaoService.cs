using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NetRedisASide3.Models;
using NetRedisASide3.Repositories;

namespace NetRedisASide3.Services;

public class MovimentacaoService
{
    private readonly IRepository<Movimentacao> _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<MovimentacaoService> _logger;
    private const string CacheKeyPrefix = "movimentacao:";
    private const string CacheKeyAll = "movimentacoes:all";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public MovimentacaoService(
        IRepository<Movimentacao> repository,
        IDistributedCache cache,
        ILogger<MovimentacaoService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<Movimentacao>> GetAllAsync()
    {
        var cachedData = await _cache.GetStringAsync(CacheKeyAll);
        
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Retornando movimentações do cache");
            return JsonSerializer.Deserialize<IEnumerable<Movimentacao>>(cachedData) ?? Enumerable.Empty<Movimentacao>();
        }

        var movimentacoes = await _repository.GetAllAsync();
        
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration
        };
        
        await _cache.SetStringAsync(
            CacheKeyAll,
            JsonSerializer.Serialize(movimentacoes),
            options);

        _logger.LogInformation("Movimentações carregadas do banco e salvas no cache");
        
        return movimentacoes;
    }

    public async Task<Movimentacao?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}{id}";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        
        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Retornando movimentação {Id} do cache", id);
            return JsonSerializer.Deserialize<Movimentacao>(cachedData);
        }

        var movimentacao = await _repository.GetByIdAsync(id);
        
        if (movimentacao != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            };
            
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(movimentacao),
                options);

            _logger.LogInformation("Movimentação {Id} carregada do banco e salva no cache", id);
        }
        
        return movimentacao;
    }

    public async Task<Movimentacao> AddAsync(Movimentacao movimentacao)
    {
        var result = await _repository.AddAsync(movimentacao);
        
        await _cache.RemoveAsync(CacheKeyAll);
        _logger.LogInformation("Cache de listagem invalidado após criação da movimentação {Id}", result.Id);
        
        return result;
    }

    public async Task<Movimentacao> UpdateAsync(Movimentacao movimentacao)
    {
        var result = await _repository.UpdateAsync(movimentacao);
        
        var cacheKey = $"{CacheKeyPrefix}{movimentacao.Id}";
        await _cache.RemoveAsync(cacheKey);
        await _cache.RemoveAsync(CacheKeyAll);
        
        _logger.LogInformation("Cache invalidado após atualização da movimentação {Id}", movimentacao.Id);
        
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
            
            _logger.LogInformation("Cache invalidado após exclusão da movimentação {Id}", id);
        }
        
        return result;
    }
}