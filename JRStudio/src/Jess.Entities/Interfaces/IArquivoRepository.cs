using Jess.Entities.Entities;

namespace Jess.Entities.Interfaces;

public interface IArquivoRepository
{
    Task<List<Arquivo>> ListarAsync(int? projetoId = null, CancellationToken cancellationToken = default);

    Task<Arquivo?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<Tag>> ListarTagsAsync(CancellationToken cancellationToken = default);

    Task<Tag> ObterOuCriarTagAsync(string nome, CancellationToken cancellationToken = default);

    Task<Projeto?> ObterProjetoAsync(int projetoId, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Arquivo arquivo, CancellationToken cancellationToken = default);

    void Remover(Arquivo arquivo);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
