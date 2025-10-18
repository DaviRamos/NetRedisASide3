using Microsoft.EntityFrameworkCore;
using NetRedisASide3.Data;
using NetRedisASide3.Models;

namespace NetRedisASide3.Repositories;

public class TipoDocumentoRepository : IRepository<TipoDocumento>
{
    private readonly AppDbContext _context;

    public TipoDocumentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TipoDocumento>> GetAllAsync()
    {
        return await _context.TiposDocumento
            .AsNoTracking()
            .OrderByDescending(t => t.DataCriacao)
            .ToListAsync();
    }

    public async Task<TipoDocumento?> GetByIdAsync(int id)
    {
        return await _context.TiposDocumento
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TipoDocumento> AddAsync(TipoDocumento entity)
    {
        entity.DataCriacao = DateTime.UtcNow;
        entity.DataAtualizacao = DateTime.UtcNow;
        
        _context.TiposDocumento.Add(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }

    public async Task<TipoDocumento> UpdateAsync(TipoDocumento entity)
    {
        var existingEntity = await _context.TiposDocumento.FindAsync(entity.Id);
        if (existingEntity == null)
            throw new InvalidOperationException($"Tipo de Documento com ID {entity.Id} não encontrado.");

        existingEntity.Nome = entity.Nome;
        existingEntity.Descricao = entity.Descricao;
        existingEntity.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        return existingEntity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.TiposDocumento.FindAsync(id);
        if (entity == null)
            return false;

        _context.TiposDocumento.Remove(entity);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.TiposDocumento.AnyAsync(t => t.Id == id);
    }
}