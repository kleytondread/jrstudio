using Jess.Entities.Entities;
using Jess.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Data.Repositories;

public class PagamentoRepository : IPagamentoRepository
{
    private readonly AppDbContext _dbContext;

    public PagamentoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Pagamento>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Pagamentos
            .Where(p => p.ProjetoId == projetoId)
            .OrderBy(p => p.DataPrevista)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Pagamento>> ListarTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Pagamentos
            .OrderBy(p => p.DataPrevista)
            .ToListAsync(cancellationToken);
    }

    public async Task<Pagamento?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Pagamentos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Pagamento pagamento, CancellationToken cancellationToken = default)
    {
        await _dbContext.Pagamentos.AddAsync(pagamento, cancellationToken);
    }

    public void Remover(Pagamento pagamento)
    {
        _dbContext.Pagamentos.Remove(pagamento);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
