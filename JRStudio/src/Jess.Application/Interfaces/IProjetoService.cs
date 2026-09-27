using Jess.Application.DTOs;
using Jess.Entities.Enums;

namespace Jess.Application.Interfaces;

public interface IProjetoService
{
    Task<IReadOnlyList<ProjetoDto>> ListarAsync(CancellationToken cancellationToken = default);

    Task<ProjetoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProjetoDto> CriarAsync(SalvarProjetoRequest request, CancellationToken cancellationToken = default);

    Task<ProjetoDto> AtualizarAsync(int id, SalvarProjetoRequest request, CancellationToken cancellationToken = default);

    Task ExcluirAsync(int id, CancellationToken cancellationToken = default);

    Task<ProjetoDto> AlterarStatusAsync(int id, StatusProjeto novoStatus, CancellationToken cancellationToken = default);
}
