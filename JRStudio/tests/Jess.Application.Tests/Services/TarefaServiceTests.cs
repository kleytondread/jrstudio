using FluentAssertions;
using Jess.Application.DTOs;
using Jess.Application.Services;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jess.Application.Tests.Services;

public class TarefaServiceTests
{
    private readonly ITarefaRepository _tarefaRepository = Substitute.For<ITarefaRepository>();
    private readonly TarefaService _sut;

    public TarefaServiceTests()
    {
        _sut = new TarefaService(_tarefaRepository, Substitute.For<ILogger<TarefaService>>());
    }

    [Fact]
    public async Task ListarPorProjetoAsync_ComTarefasCadastradas_RetornaTodasMapeadas()
    {
        var tarefas = new List<Tarefa>
        {
            new() { Id = 1, Titulo = "Levantamento métrico", ProjetoId = 1, Fase = FaseProjeto.PreProjeto, Coluna = ColunaTarefa.Concluido, DataCriacao = DateTime.Now }
        };
        _tarefaRepository.ListarPorProjetoAsync(1, Arg.Any<CancellationToken>()).Returns(tarefas);

        var resultado = await _sut.ListarPorProjetoAsync(1);

        resultado.Should().ContainSingle();
        resultado[0].Titulo.Should().Be("Levantamento métrico");
        resultado[0].Fase.Should().Be(FaseProjeto.PreProjeto);
    }

    [Fact]
    public async Task ListarTodasAsync_ComTarefasDeVariosProjetos_RetornaTodasMapeadas()
    {
        var tarefas = new List<Tarefa>
        {
            new() { Id = 1, Titulo = "Tarefa A", ProjetoId = 1, Coluna = ColunaTarefa.AFazer, DataCriacao = DateTime.Now },
            new() { Id = 2, Titulo = "Tarefa B", ProjetoId = 2, Coluna = ColunaTarefa.Fazendo, DataCriacao = DateTime.Now }
        };
        _tarefaRepository.ListarTodasAsync(Arg.Any<CancellationToken>()).Returns(tarefas);

        var resultado = await _sut.ListarTodasAsync();

        resultado.Should().HaveCount(2);
        resultado.Select(t => t.ProjetoId).Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public async Task CriarRapidaAsync_ComDadosValidos_PersisteComFaseEColunaInformadas()
    {
        var request = new CriarTarefaRapidaRequest
        {
            ProjetoId = 5,
            Fase = FaseProjeto.Anteprojeto,
            Coluna = ColunaTarefa.AFazer,
            Titulo = "Selecionar revestimento do banheiro"
        };

        var resultado = await _sut.CriarRapidaAsync(request);

        resultado.Titulo.Should().Be("Selecionar revestimento do banheiro");
        resultado.ProjetoId.Should().Be(5);
        resultado.Fase.Should().Be(FaseProjeto.Anteprojeto);
        resultado.Coluna.Should().Be(ColunaTarefa.AFazer);
        resultado.DataConclusao.Should().BeNull();
        await _tarefaRepository.Received(1).AdicionarAsync(
            Arg.Is<Tarefa>(t => t.Titulo == "Selecionar revestimento do banheiro" && t.ProjetoId == 5),
            Arg.Any<CancellationToken>());
        await _tarefaRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarRapidaAsync_DiretoNaColunaConcluido_JaDefineDataConclusao()
    {
        var request = new CriarTarefaRapidaRequest
        {
            ProjetoId = 1,
            Fase = FaseProjeto.PreProjeto,
            Coluna = ColunaTarefa.Concluido,
            Titulo = "Tarefa já concluída"
        };

        var resultado = await _sut.CriarRapidaAsync(request);

        resultado.DataConclusao.Should().NotBeNull();
    }

    [Fact]
    public async Task AtualizarAsync_TarefaExistente_AtualizaCampos()
    {
        var tarefa = new Tarefa { Id = 2, Titulo = "Antigo", ProjetoId = 1, Fase = FaseProjeto.EstudoPreliminar, Coluna = ColunaTarefa.AFazer, DataCriacao = DateTime.Now };
        _tarefaRepository.ObterPorIdAsync(2, Arg.Any<CancellationToken>()).Returns(tarefa);

        var resultado = await _sut.AtualizarAsync(2, new SalvarTarefaRequest
        {
            Titulo = "Novo título",
            Prioridade = PrioridadeTarefa.Alta,
            Coluna = ColunaTarefa.Fazendo
        });

        resultado.Titulo.Should().Be("Novo título");
        resultado.Prioridade.Should().Be(PrioridadeTarefa.Alta);
        resultado.Coluna.Should().Be(ColunaTarefa.Fazendo);
        await _tarefaRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarAsync_TarefaInexistente_LancaKeyNotFoundException()
    {
        _tarefaRepository.ObterPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Tarefa?)null);

        var acao = () => _sut.AtualizarAsync(99, new SalvarTarefaRequest { Titulo = "X" });

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AtualizarAsync_MovendoParaConcluido_DefineDataConclusao()
    {
        var tarefa = new Tarefa { Id = 3, Titulo = "T", ProjetoId = 1, Fase = FaseProjeto.Anteprojeto, Coluna = ColunaTarefa.Fazendo, DataCriacao = DateTime.Now };
        _tarefaRepository.ObterPorIdAsync(3, Arg.Any<CancellationToken>()).Returns(tarefa);

        var resultado = await _sut.AtualizarAsync(3, new SalvarTarefaRequest { Titulo = "T", Coluna = ColunaTarefa.Concluido });

        resultado.DataConclusao.Should().NotBeNull();
    }

    [Fact]
    public async Task AtualizarAsync_MovendoDeConcluidoParaOutraColuna_LimpaDataConclusao()
    {
        var tarefa = new Tarefa
        {
            Id = 4,
            Titulo = "T",
            ProjetoId = 1,
            Fase = FaseProjeto.Anteprojeto,
            Coluna = ColunaTarefa.Concluido,
            DataConclusao = DateTime.Now,
            DataCriacao = DateTime.Now
        };
        _tarefaRepository.ObterPorIdAsync(4, Arg.Any<CancellationToken>()).Returns(tarefa);

        var resultado = await _sut.AtualizarAsync(4, new SalvarTarefaRequest { Titulo = "T", Coluna = ColunaTarefa.AFazer });

        resultado.DataConclusao.Should().BeNull();
    }

    [Fact]
    public async Task ExcluirAsync_TarefaExistente_RemoveEPersisteAlteracao()
    {
        var tarefa = new Tarefa { Id = 6, Titulo = "Excluir", DataCriacao = DateTime.Now };
        _tarefaRepository.ObterPorIdAsync(6, Arg.Any<CancellationToken>()).Returns(tarefa);

        await _sut.ExcluirAsync(6);

        _tarefaRepository.Received(1).Remover(tarefa);
        await _tarefaRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExcluirAsync_TarefaInexistente_LancaKeyNotFoundException()
    {
        _tarefaRepository.ObterPorIdAsync(123, Arg.Any<CancellationToken>()).Returns((Tarefa?)null);

        var acao = () => _sut.ExcluirAsync(123);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }
}
