using Jess.Entities.Entities;

namespace Jess.Entities.Interfaces;

public interface IPagamentoRepository
{
    Task<List<Pagamento>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default);

    /// <summary>Todos os pagamentos de todos os projetos — usado pelo Dashboard.</summary>
    Task<List<Pagamento>> ListarTodosAsync(CancellationToken cancellationToken = default);

    Task<Pagamento?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Pagamento pagamento, CancellationToken cancellationToken = default);

    void Remover(Pagamento pagamento);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
