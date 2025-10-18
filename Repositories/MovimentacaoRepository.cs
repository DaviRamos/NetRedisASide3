using Microsoft.EntityFrameworkCore;
using NetRedisASide3.Data;
using NetRedisASide3.Models;

namespace NetRedisASide3.Repositories;

public class MovimentacaoRepository : IRepository<Movimentacao>
{
    private readonly AppDbContext _context;

    public MovimentacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Movimentacao>> GetAllAsync()
    {
        return await _context.Movimentacoes
            .AsNoTracking()
            .OrderByDescending(m => m.DataCriacao)
            .ToListAsync();
    }

    public async Task<Movimentacao?> GetByIdAsync(int id)
    {
        return await _context.Movimentacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Movimentacao> AddAsync(Movimentacao entity)
    {
        entity.DataCriacao = DateTime.UtcNow;
        entity.DataAtualizacao = DateTime.UtcNow;
        
        _context.Movimentacoes.Add(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }

    public async Task<Movimentacao> UpdateAsync(Movimentacao entity)
    {
        var existingEntity = await _context.Movimentacoes.FindAsync(entity.Id);
        if (existingEntity == null)
            throw new InvalidOperationException($"Movimentação com ID {entity.Id} não encontrada.");

        existingEntity.Nome = entity.Nome;
        existingEntity.Descricao = entity.Descricao;
        existingEntity.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        return existingEntity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Movimentacoes.FindAsync(id);
        if (entity == null)
            return false;

        _context.Movimentacoes.Remove(entity);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Movimentacoes.AnyAsync(m => m.Id == id);
    }
}