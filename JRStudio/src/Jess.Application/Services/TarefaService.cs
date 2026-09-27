using Jess.Application.DTOs;
using Jess.Application.Interfaces;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jess.Application.Services;

public class TarefaService : ITarefaService
{
    private readonly ITarefaRepository _tarefaRepository;
    private readonly ILogger<TarefaService> _logger;

    public TarefaService(ITarefaRepository tarefaRepository, ILogger<TarefaService> logger)
    {
        _tarefaRepository = tarefaRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TarefaDto>> ListarPorProjetoAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        var tarefas = await _tarefaRepository.ListarPorProjetoAsync(projetoId, cancellationToken);
        return tarefas.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<TarefaDto>> ListarTodasAsync(CancellationToken cancellationToken = default)
    {
        var tarefas = await _tarefaRepository.ListarTodasAsync(cancellationToken);
        return tarefas.Select(MapToDto).ToList();
    }

    public async Task<TarefaDto> CriarRapidaAsync(CriarTarefaRapidaRequest request, CancellationToken cancellationToken = default)
    {
        var tarefa = new Tarefa
        {
            Titulo = request.Titulo,
            ProjetoId = request.ProjetoId,
            Fase = request.Fase,
            Coluna = request.Coluna,
            Prioridade = PrioridadeTarefa.Media,
            DataCriacao = DateTime.Now,
            DataConclusao = request.Coluna == ColunaTarefa.Concluido ? DateTime.Now : null
        };

        await _tarefaRepository.AdicionarAsync(tarefa, cancellationToken);
        await _tarefaRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Tarefa {TarefaId} criada no projeto {ProjetoId}, fase {Fase}", tarefa.Id, tarefa.ProjetoId, tarefa.Fase);

        return MapToDto(tarefa);
    }

    public async Task<TarefaDto> AtualizarAsync(int id, SalvarTarefaRequest request, CancellationToken cancellationToken = default)
    {
        var tarefa = await _tarefaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Tarefa {id} não encontrada.");

        tarefa.Titulo = request.Titulo;
        tarefa.Descricao = request.Descricao;
        tarefa.Prioridade = request.Prioridade;
        tarefa.Prazo = request.Prazo;
        tarefa.RegraRecorrencia = request.RegraRecorrencia;
        AplicarColuna(tarefa, request.Coluna);

        await _tarefaRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Tarefa {TarefaId} atualizada", tarefa.Id);

        return MapToDto(tarefa);
    }

    public async Task<TarefaDto> MoverAsync(int id, ColunaTarefa novaColuna, CancellationToken cancellationToken = default)
    {
        var tarefa = await _tarefaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Tarefa {id} não encontrada.");

        AplicarColuna(tarefa, novaColuna);
        await _tarefaRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Tarefa {TarefaId} movida para a coluna {Coluna}", id, novaColuna);

        return MapToDto(tarefa);
    }

    public async Task<TarefaDto> AlterarPrioridadeAsync(int id, PrioridadeTarefa prioridade, CancellationToken cancellationToken = default)
    {
        var tarefa = await _tarefaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Tarefa {id} não encontrada.");

        tarefa.Prioridade = prioridade;
        await _tarefaRepository.SalvarAlteracoesAsync(cancellationToken);

        return MapToDto(tarefa);
    }

    public async Task ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        var tarefa = await _tarefaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Tarefa {id} não encontrada.");

        _tarefaRepository.Remover(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync(cancellationToken);

        _logger.LogInformation("Tarefa {TarefaId} excluída ({Titulo})", id, tarefa.Titulo);
    }

    private static void AplicarColuna(Tarefa tarefa, ColunaTarefa novaColuna)
    {
        if (novaColuna == ColunaTarefa.Concluido && tarefa.Coluna != ColunaTarefa.Concluido)
        {
            tarefa.DataConclusao = DateTime.Now;
        }
        else if (novaColuna != ColunaTarefa.Concluido)
        {
            tarefa.DataConclusao = null;
        }

        tarefa.Coluna = novaColuna;
    }

    private static TarefaDto MapToDto(Tarefa tarefa) => new()
    {
        Id = tarefa.Id,
        Titulo = tarefa.Titulo,
        Descricao = tarefa.Descricao,
        ProjetoId = tarefa.ProjetoId,
        Fase = tarefa.Fase,
        Prioridade = tarefa.Prioridade,
        Prazo = tarefa.Prazo,
        Coluna = tarefa.Coluna,
        RegraRecorrencia = tarefa.RegraRecorrencia,
        DataCriacao = tarefa.DataCriacao,
        DataConclusao = tarefa.DataConclusao
    };
}
