using Jess.Entities.Entities;

namespace Jess.Entities.Interfaces;

public interface ITarefaRepository
{
    Task<List<Tarefa>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default);

    /// <summary>Todas as tarefas de todos os projetos — usado pelo Dashboard.</summary>
    Task<List<Tarefa>> ListarTodasAsync(CancellationToken cancellationToken = default);

    Task<Tarefa?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Tarefa tarefa, CancellationToken cancellationToken = default);

    void Remover(Tarefa tarefa);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
