// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using NetRedisASide3.Models;

namespace NetRedisASide3.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Assunto> Assuntos { get; set; }
    public DbSet<Movimentacao> Movimentacoes { get; set; }
    public DbSet<TipoDocumento> TiposDocumento { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Assunto
        modelBuilder.Entity<Assunto>(entity =>
        {
            entity.ToTable("assuntos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.DataCriacao).IsRequired();
            entity.Property(e => e.DataAtualizacao).IsRequired();
            entity.HasIndex(e => e.Nome);
        });

        // Configuração da entidade Movimentacao
        modelBuilder.Entity<Movimentacao>(entity =>
        {
            entity.ToTable("movimentacoes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.DataCriacao).IsRequired();
            entity.Property(e => e.DataAtualizacao).IsRequired();
            entity.HasIndex(e => e.Nome);
        });

        // Configuração da entidade TipoDocumento
        modelBuilder.Entity<TipoDocumento>(entity =>
        {
            entity.ToTable("tipos_documento");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(1000);
            entity.Property(e => e.DataCriacao).IsRequired();
            entity.Property(e => e.DataAtualizacao).IsRequired();
            entity.HasIndex(e => e.Nome);
        });
    }
}