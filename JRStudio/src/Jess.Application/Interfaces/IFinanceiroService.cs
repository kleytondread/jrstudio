using Jess.Application.DTOs;

namespace Jess.Application.Interfaces;

public interface IFinanceiroService
{
    Task<IReadOnlyList<PagamentoDto>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default);

    /// <summary>Todos os pagamentos de todos os projetos — usado pelo Dashboard.</summary>
    Task<IReadOnlyList<PagamentoDto>> ListarTodosAsync(CancellationToken cancellationToken = default);

    Task<PagamentoDto> CriarAsync(int projetoId, SalvarPagamentoRequest request, CancellationToken cancellationToken = default);

    Task<PagamentoDto> AtualizarAsync(int id, SalvarPagamentoRequest request, CancellationToken cancellationToken = default);

    Task ExcluirAsync(int id, CancellationToken cancellationToken = default);

    Task<PagamentoDto> MarcarComoRecebidoAsync(int id, CancellationToken cancellationToken = default);

    Task<decimal> ObterSaldoAsync(int projetoId, decimal valorTotalAcordado, CancellationToken cancellationToken = default);
}
