using Jess.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Projeto> Projetos => Set<Projeto>();

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

    public DbSet<Subtarefa> Subtarefas => Set<Subtarefa>();

    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    public DbSet<RegistroDeHoras> RegistrosDeHoras => Set<RegistroDeHoras>();

    public DbSet<SegmentoHoras> SegmentosHoras => Set<SegmentoHoras>();

    public DbSet<PausaRegistroHoras> PausasRegistroHoras => Set<PausaRegistroHoras>();

    public DbSet<Arquivo> Arquivos => Set<Arquivo>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<Configuracao> Configuracoes => Set<Configuracao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(c => c.Nome).IsRequired();
        });

        modelBuilder.Entity<Projeto>(entity =>
        {
            entity.Property(p => p.Nome).IsRequired();

            entity.HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Tarefa>(entity =>
        {
            entity.Property(t => t.Titulo).IsRequired();

            entity.HasOne(t => t.Projeto)
                .WithMany()
                .HasForeignKey(t => t.ProjetoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Subtarefas)
                .WithOne(s => s.Tarefa)
                .HasForeignKey(s => s.TarefaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Subtarefa>(entity =>
        {
            entity.Property(s => s.Descricao).IsRequired();
        });

        modelBuilder.Entity<Pagamento>(entity =>
        {
            entity.HasOne(p => p.Projeto)
                .WithMany()
                .HasForeignKey(p => p.ProjetoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RegistroDeHoras>(entity =>
        {
            entity.HasOne(r => r.Projeto)
                .WithMany()
                .HasForeignKey(r => r.ProjetoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.Segmentos)
                .WithOne(s => s.RegistroHoras)
                .HasForeignKey(s => s.RegistroHorasId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.Pausas)
                .WithOne(p => p.RegistroHoras)
                .HasForeignKey(p => p.RegistroHorasId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Arquivo>(entity =>
        {
            entity.Property(a => a.NomeArquivo).IsRequired();
            entity.Property(a => a.CaminhoArquivo).IsRequired();

            // N:N implícito — excluir um Projeto remove só a linha de ArquivoProjeto (o vínculo),
            // nunca o Arquivo em si (Seção 6.3: "excluir um Projeto não deve excluir Arquivos vinculados").
            entity.HasMany(a => a.Tags)
                .WithMany(t => t.Arquivos)
                .UsingEntity(j => j.ToTable("ArquivoTag"));

            entity.HasMany(a => a.Projetos)
                .WithMany()
                .UsingEntity(j => j.ToTable("ArquivoProjeto"));
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Property(t => t.Nome).IsRequired();
            entity.HasIndex(t => t.Nome).IsUnique();
        });
    }
}
