using Jess.Entities.Entities;
using Jess.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Data.Repositories;

public class ProjetoRepository : IProjetoRepository
{
    private readonly AppDbContext _dbContext;

    public ProjetoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Projeto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projetos
            .Include(p => p.Cliente)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Projeto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projetos
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Projeto projeto, CancellationToken cancellationToken = default)
    {
        await _dbContext.Projetos.AddAsync(projeto, cancellationToken);
    }

    public void Remover(Projeto projeto)
    {
        _dbContext.Projetos.Remove(projeto);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
