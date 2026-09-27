using Jess.Entities.Entities;
using Jess.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Data.Repositories;

public class ArquivoRepository : IArquivoRepository
{
    private readonly AppDbContext _dbContext;

    public ArquivoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private IQueryable<Arquivo> ComIncludes() => _dbContext.Arquivos
        .Include(a => a.Tags)
        .Include(a => a.Projetos)
        .AsSplitQuery();

    public async Task<List<Arquivo>> ListarAsync(int? projetoId = null, CancellationToken cancellationToken = default)
    {
        var query = ComIncludes();

        if (projetoId is not null)
        {
            query = query.Where(a => a.Projetos.Any(p => p.Id == projetoId));
        }

        return await query.OrderByDescending(a => a.DataImportacao).ToListAsync(cancellationToken);
    }

    public async Task<Arquivo?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await ComIncludes().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Tag>> ListarTagsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags.OrderBy(t => t.Nome).ToListAsync(cancellationToken);
    }

    public async Task<Tag> ObterOuCriarTagAsync(string nome, CancellationToken cancellationToken = default)
    {
        var existente = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Nome == nome, cancellationToken);
        if (existente is not null)
        {
            return existente;
        }

        var tag = new Tag { Nome = nome };
        await _dbContext.Tags.AddAsync(tag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return tag;
    }

    public async Task<Projeto?> ObterProjetoAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projetos.FirstOrDefaultAsync(p => p.Id == projetoId, cancellationToken);
    }

    public async Task AdicionarAsync(Arquivo arquivo, CancellationToken cancellationToken = default)
    {
        await _dbContext.Arquivos.AddAsync(arquivo, cancellationToken);
    }

    public void Remover(Arquivo arquivo)
    {
        _dbContext.Arquivos.Remove(arquivo);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
