using Microsoft.EntityFrameworkCore;
using NetRedisASide3.Data;
using NetRedisASide3.Models;

namespace NetRedisASide3.Repositories;

public class AssuntoRepository : IRepository<Assunto>
{
    private readonly AppDbContext _context;

    public AssuntoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Assunto>> GetAllAsync()
    {
        return await _context.Assuntos
            .AsNoTracking()
            .OrderByDescending(a => a.DataCriacao)
            .ToListAsync();
    }

    public async Task<Assunto?> GetByIdAsync(int id)
    {
        return await _context.Assuntos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Assunto> AddAsync(Assunto entity)
    {
        entity.DataCriacao = DateTime.UtcNow;
        entity.DataAtualizacao = DateTime.UtcNow;
        
        _context.Assuntos.Add(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }

    public async Task<Assunto> UpdateAsync(Assunto entity)
    {
        var existingEntity = await _context.Assuntos.FindAsync(entity.Id);
        if (existingEntity == null)
            throw new InvalidOperationException($"Assunto com ID {entity.Id} não encontrado.");

        existingEntity.Nome = entity.Nome;
        existingEntity.Descricao = entity.Descricao;
        existingEntity.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        return existingEntity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Assuntos.FindAsync(id);
        if (entity == null)
            return false;

        _context.Assuntos.Remove(entity);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Assuntos.AnyAsync(a => a.Id == id);
    }
}