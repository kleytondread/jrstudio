using Jess.Application.DTOs;

namespace Jess.Application.Interfaces;

public interface IConfiguracaoService
{
    Task<ConfiguracaoDto> ObterAsync(CancellationToken cancellationToken = default);

    Task<ConfiguracaoDto> AtualizarNomeArquitetoAsync(string? nomeArquiteto, CancellationToken cancellationToken = default);
}
