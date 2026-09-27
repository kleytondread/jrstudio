using Jess.Entities.Entities;
using Jess.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Data.Repositories;

public class TarefaRepository : ITarefaRepository
{
    private readonly AppDbContext _dbContext;

    public TarefaRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Tarefa>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tarefas
            .Where(t => t.ProjetoId == projetoId)
            .OrderBy(t => t.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Tarefa>> ListarTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tarefas
            .OrderBy(t => t.Prazo)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tarefa?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tarefas.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Tarefa tarefa, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tarefas.AddAsync(tarefa, cancellationToken);
    }

    public void Remover(Tarefa tarefa)
    {
        _dbContext.Tarefas.Remove(tarefa);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
