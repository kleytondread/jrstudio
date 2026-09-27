using Jess.Application.DTOs;
using Jess.Entities.Enums;

namespace Jess.Application.Interfaces;

public interface ITarefaService
{
    Task<IReadOnlyList<TarefaDto>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default);

    /// <summary>Todas as tarefas de todos os projetos — usado pelo Dashboard.</summary>
    Task<IReadOnlyList<TarefaDto>> ListarTodasAsync(CancellationToken cancellationToken = default);

    Task<TarefaDto> CriarRapidaAsync(CriarTarefaRapidaRequest request, CancellationToken cancellationToken = default);

    Task<TarefaDto> AtualizarAsync(int id, SalvarTarefaRequest request, CancellationToken cancellationToken = default);

    /// <summary>Move o card para outra coluna do quadro Kanban (sem precisar do formulário completo).</summary>
    Task<TarefaDto> MoverAsync(int id, ColunaTarefa novaColuna, CancellationToken cancellationToken = default);

    /// <summary>Troca só a prioridade (usado pelo seletor rápido no próprio card).</summary>
    Task<TarefaDto> AlterarPrioridadeAsync(int id, PrioridadeTarefa prioridade, CancellationToken cancellationToken = default);

    Task ExcluirAsync(int id, CancellationToken cancellationToken = default);
}
