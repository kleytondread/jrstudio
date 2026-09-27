using Jess.Application.DTOs;
using Jess.Application.Interfaces;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jess.Application.Services;

public class RastreamentoHorasService : IRastreamentoHorasService
{
    private readonly IRegistroDeHorasRepository _repository;
    private readonly ILogger<RastreamentoHorasService> _logger;

    public RastreamentoHorasService(IRegistroDeHorasRepository repository, ILogger<RastreamentoHorasService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<RegistroHorasDto?> ObterEmAndamentoAsync(CancellationToken cancellationToken = default)
    {
        var registro = await _repository.ObterEmAndamentoAsync(cancellationToken);
        return registro is null ? null : MapToDto(registro);
    }

    public async Task<IReadOnlyList<RegistroHorasDto>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        var registros = await _repository.ListarPorProjetoAsync(projetoId, cancellationToken);
        return registros.Select(MapToDto).ToList();
    }

    public async Task<RegistroHorasDto> IniciarAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        var ativo = await _repository.ObterEmAndamentoAsync(cancellationToken);
        if (ativo is not null)
        {
            throw new InvalidOperationException("Já existe um cronômetro ativo. Pare-o antes de iniciar um novo.");
        }

        var registro = new RegistroDeHoras
        {
            ProjetoId = projetoId,
            InicioSessao = DateTime.Now,
            Origem = OrigemRegistroHoras.Cronometro,
            Status = StatusRegistroHoras.EmAndamento
        };

        await _repository.AdicionarAsync(registro, cancellationToken);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Cronômetro iniciado (registro {RegistroId}) no projeto {ProjetoId}", registro.Id, projetoId);

        return MapToDto(registro);
    }

    public async Task<RegistroHorasDto> PausarAsync(int id, CancellationToken cancellationToken = default)
    {
        var registro = await ObterOuFalharAsync(id, cancellationToken);

        if (registro.Status != StatusRegistroHoras.EmAndamento)
        {
            throw new InvalidOperationException("Só é possível pausar um registro em andamento.");
        }

        registro.Status = StatusRegistroHoras.Pausado;
        registro.Pausas.Add(new PausaRegistroHoras { InicioPausa = DateTime.Now });

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return MapToDto(registro);
    }

    public async Task<RegistroHorasDto> RetomarAsync(int id, CancellationToken cancellationToken = default)
    {
        var registro = await ObterOuFalharAsync(id, cancellationToken);

        if (registro.Status != StatusRegistroHoras.Pausado)
        {
            throw new InvalidOperationException("Só é possível retomar um registro pausado.");
        }

        var pausaAberta = registro.Pausas.First(p => p.FimPausa is null);
        pausaAberta.FimPausa = DateTime.Now;
        registro.Status = StatusRegistroHoras.EmAndamento;

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return MapToDto(registro);
    }

    public async Task<RegistroHorasDto> PararAsync(int id, CategoriaHoras? categoria, string? nota, CancellationToken cancellationToken = default)
    {
        var registro = await ObterOuFalharAsync(id, cancellationToken);

        if (registro.Status == StatusRegistroHoras.Concluido)
        {
            throw new InvalidOperationException("Esse registro já foi concluído.");
        }

        var fim = DateTime.Now;
        var pausaAberta = registro.Pausas.FirstOrDefault(p => p.FimPausa is null);
        if (pausaAberta is not null)
        {
            pausaAberta.FimPausa = fim;
        }

        registro.FimSessao = fim;
        registro.Status = StatusRegistroHoras.Concluido;
        registro.DuracaoMinutos = RastreamentoHorasCalculos.CalcularDuracaoMinutos(
            registro.InicioSessao, fim, registro.Pausas.Select(p => (p.InicioPausa, p.FimPausa)));
        registro.Categoria = categoria;
        registro.Nota = nota;

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Cronômetro parado (registro {RegistroId}), duração {DuracaoMinutos} min", id, registro.DuracaoMinutos);

        return MapToDto(registro);
    }

    public async Task<RegistroHorasDto> RegistrarManualAsync(int projetoId, DateTime inicioSessao, List<SegmentoInput> segmentos, CancellationToken cancellationToken = default)
    {
        ValidarSegmentos(segmentos);

        var registro = new RegistroDeHoras
        {
            ProjetoId = projetoId,
            InicioSessao = inicioSessao,
            FimSessao = inicioSessao,
            Origem = OrigemRegistroHoras.Manual,
            Status = StatusRegistroHoras.Concluido
        };

        AplicarSegmentos(registro, segmentos);

        await _repository.AdicionarAsync(registro, cancellationToken);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Registro manual de horas criado (registro {RegistroId}) no projeto {ProjetoId}", registro.Id, projetoId);

        return MapToDto(registro);
    }

    public async Task<RegistroHorasDto> SalvarSegmentosAsync(int id, List<SegmentoInput> segmentos, CancellationToken cancellationToken = default)
    {
        ValidarSegmentos(segmentos);

        var registro = await ObterOuFalharAsync(id, cancellationToken);
        AplicarSegmentos(registro, segmentos);

        await _repository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Registro de horas {RegistroId} atualizado com {QuantidadeTrechos} trecho(s)", id, segmentos.Count);

        return MapToDto(registro);
    }

    public async Task ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        var registro = await ObterOuFalharAsync(id, cancellationToken);

        _repository.Remover(registro);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Registro de horas {RegistroId} excluído", id);
    }

    public async Task<decimal?> ObterValorPorHoraAsync(int projetoId, decimal valorTotalAcordado, CancellationToken cancellationToken = default)
    {
        var registros = await _repository.ListarPorProjetoAsync(projetoId, cancellationToken);
        var totalMinutos = registros
            .Where(r => r.Status == StatusRegistroHoras.Concluido)
            .Sum(r => r.DuracaoMinutos);

        return RastreamentoHorasCalculos.CalcularValorPorHora(valorTotalAcordado, totalMinutos);
    }

    private static void ValidarSegmentos(List<SegmentoInput> segmentos)
    {
        if (segmentos.Count == 0 || segmentos.All(s => s.DuracaoMinutos <= 0))
        {
            throw new ArgumentException("Informe ao menos um trecho com duração maior que zero.");
        }
    }

    private static void AplicarSegmentos(RegistroDeHoras registro, List<SegmentoInput> segmentos)
    {
        registro.Segmentos.Clear();
        registro.DuracaoMinutos = segmentos.Sum(s => s.DuracaoMinutos);

        if (segmentos.Count == 1)
        {
            registro.Categoria = segmentos[0].Categoria;
            registro.Nota = segmentos[0].Nota;
            return;
        }

        registro.Categoria = null;
        registro.Nota = null;

        for (var i = 0; i < segmentos.Count; i++)
        {
            registro.Segmentos.Add(new SegmentoHoras
            {
                DuracaoMinutos = segmentos[i].DuracaoMinutos,
                Categoria = segmentos[i].Categoria,
                Nota = segmentos[i].Nota,
                Ordem = i
            });
        }
    }

    private async Task<RegistroDeHoras> ObterOuFalharAsync(int id, CancellationToken cancellationToken) =>
        await _repository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Registro de horas {id} não encontrado.");

    private static RegistroHorasDto MapToDto(RegistroDeHoras registro) => new()
    {
        Id = registro.Id,
        ProjetoId = registro.ProjetoId,
        InicioSessao = registro.InicioSessao,
        FimSessao = registro.FimSessao,
        DuracaoMinutos = registro.DuracaoMinutos,
        Origem = registro.Origem,
        Status = registro.Status,
        Categoria = registro.Categoria,
        Nota = registro.Nota,
        PausaAbertaEm = registro.Pausas.FirstOrDefault(p => p.FimPausa is null)?.InicioPausa,
        TempoPausadoFechado = registro.Pausas
            .Where(p => p.FimPausa is not null)
            .Aggregate(TimeSpan.Zero, (soma, p) => soma + (p.FimPausa!.Value - p.InicioPausa)),
        Segmentos = registro.Segmentos
            .OrderBy(s => s.Ordem)
            .Select(s => new SegmentoHorasDto
            {
                DuracaoMinutos = s.DuracaoMinutos,
                Categoria = s.Categoria,
                Nota = s.Nota,
                Ordem = s.Ordem
            })
            .ToList()
    };
}
