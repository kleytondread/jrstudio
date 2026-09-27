using Jess.Application.DTOs;
using Jess.Application.Interfaces;
using Jess.Application.Services;
using Jess.Entities.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jess.Application.Tests.Services;

public class NotificacaoServiceTests
{
    private readonly ITarefaService _tarefaService = Substitute.For<ITarefaService>();
    private readonly IToastService _toastService = Substitute.For<IToastService>();
    private readonly NotificacaoService _sut;

    public NotificacaoServiceTests()
    {
        var scope = Substitute.For<IServiceScope>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(ITarefaService)).Returns(_tarefaService);
        scope.ServiceProvider.Returns(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        _sut = new NotificacaoService(scopeFactory, _toastService, Substitute.For<ILogger<NotificacaoService>>());
    }

    private static TarefaDto CriarTarefa(int id, ColunaTarefa coluna, DateTime? prazo) => new()
    {
        Id = id,
        Titulo = $"Tarefa {id}",
        Coluna = coluna,
        Prazo = prazo,
        DataCriacao = DateTime.Now
    };

    [Fact]
    public async Task VerificarAsync_TarefaComPrazoHojeENaoConcluida_Notifica()
    {
        var tarefa = CriarTarefa(1, ColunaTarefa.AFazer, DateTime.Today);
        _tarefaService.ListarTodasAsync(Arg.Any<CancellationToken>()).Returns([tarefa]);

        await _sut.VerificarAsync();

        _toastService.Received(1).Mostrar("Tarefa 1", Arg.Any<string>());
    }

    [Fact]
    public async Task VerificarAsync_TarefaConcluidaComPrazoHoje_NaoNotifica()
    {
        var tarefa = CriarTarefa(2, ColunaTarefa.Concluido, DateTime.Today);
        _tarefaService.ListarTodasAsync(Arg.Any<CancellationToken>()).Returns([tarefa]);

        await _sut.VerificarAsync();

        _toastService.DidNotReceive().Mostrar(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task VerificarAsync_TarefaSemPrazo_NaoNotifica()
    {
        var tarefa = CriarTarefa(3, ColunaTarefa.AFazer, null);
        _tarefaService.ListarTodasAsync(Arg.Any<CancellationToken>()).Returns([tarefa]);

        await _sut.VerificarAsync();

        _toastService.DidNotReceive().Mostrar(Arg.Any<string>(), Arg.Any<string>());
    }

    [Theory]
    [MemberData(nameof(DiasForaDeHoje))]
    public async Task VerificarAsync_TarefaComPrazoForaDeHoje_NaoNotifica(DateTime prazo)
    {
        var tarefa = CriarTarefa(4, ColunaTarefa.Fazendo, prazo);
        _tarefaService.ListarTodasAsync(Arg.Any<CancellationToken>()).Returns([tarefa]);

        await _sut.VerificarAsync();

        _toastService.DidNotReceive().Mostrar(Arg.Any<string>(), Arg.Any<string>());
    }

    public static IEnumerable<object[]> DiasForaDeHoje()
    {
        yield return [DateTime.Today.AddDays(-1)];
        yield return [DateTime.Today.AddDays(1)];
    }

    [Fact]
    public async Task VerificarAsync_ChamadoDuasVezesNoMesmoDia_NotificaSoUmaVez()
    {
        var tarefa = CriarTarefa(5, ColunaTarefa.AFazer, DateTime.Today);
        _tarefaService.ListarTodasAsync(Arg.Any<CancellationToken>()).Returns([tarefa]);

        await _sut.VerificarAsync();
        await _sut.VerificarAsync();

        _toastService.Received(1).Mostrar("Tarefa 5", Arg.Any<string>());
    }
}
