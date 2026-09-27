using Jess.Entities.Entities;

namespace Jess.Entities.Interfaces;

public interface IProjetoRepository
{
    Task<List<Projeto>> ListarAsync(CancellationToken cancellationToken = default);

    Task<Projeto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Projeto projeto, CancellationToken cancellationToken = default);

    void Remover(Projeto projeto);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
