using Jess.Application.DTOs;

namespace Jess.Application.Interfaces;

public interface IArquivoService
{
    Task<IReadOnlyList<ArquivoDto>> ListarAsync(int? projetoId = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListarTagsAsync(CancellationToken cancellationToken = default);

    Task<ArquivoDto> ImportarAsync(
        string caminhoOrigemAbsoluto,
        IReadOnlyCollection<int>? projetoIds = null,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default);

    Task VincularProjetoAsync(int arquivoId, int projetoId, CancellationToken cancellationToken = default);

    Task DesvincularProjetoAsync(int arquivoId, int projetoId, CancellationToken cancellationToken = default);

    Task AtualizarTagsAsync(int arquivoId, IReadOnlyCollection<string> tags, CancellationToken cancellationToken = default);

    Task AlternarFavoritoAsync(int arquivoId, CancellationToken cancellationToken = default);

    Task ExcluirAsync(int arquivoId, CancellationToken cancellationToken = default);
}
