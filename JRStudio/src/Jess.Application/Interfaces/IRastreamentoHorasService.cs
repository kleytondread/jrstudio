using Jess.Application.DTOs;
using Jess.Entities.Enums;

namespace Jess.Application.Interfaces;

public interface IRastreamentoHorasService
{
    Task<RegistroHorasDto?> ObterEmAndamentoAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegistroHorasDto>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default);

    Task<RegistroHorasDto> IniciarAsync(int projetoId, CancellationToken cancellationToken = default);

    Task<RegistroHorasDto> PausarAsync(int id, CancellationToken cancellationToken = default);

    Task<RegistroHorasDto> RetomarAsync(int id, CancellationToken cancellationToken = default);

    Task<RegistroHorasDto> PararAsync(int id, CategoriaHoras? categoria, string? nota, CancellationToken cancellationToken = default);

    Task<RegistroHorasDto> RegistrarManualAsync(int projetoId, DateTime inicioSessao, List<SegmentoInput> segmentos, CancellationToken cancellationToken = default);

    Task<RegistroHorasDto> SalvarSegmentosAsync(int id, List<SegmentoInput> segmentos, CancellationToken cancellationToken = default);

    Task ExcluirAsync(int id, CancellationToken cancellationToken = default);

    Task<decimal?> ObterValorPorHoraAsync(int projetoId, decimal valorTotalAcordado, CancellationToken cancellationToken = default);
}
