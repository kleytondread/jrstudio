using FluentAssertions;
using Jess.Application.DTOs;
using Jess.Application.Services;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jess.Application.Tests.Services;

public class RastreamentoHorasServiceTests
{
    private readonly IRegistroDeHorasRepository _repository = Substitute.For<IRegistroDeHorasRepository>();
    private readonly RastreamentoHorasService _sut;

    public RastreamentoHorasServiceTests()
    {
        _sut = new RastreamentoHorasService(_repository, Substitute.For<ILogger<RastreamentoHorasService>>());
    }

    [Fact]
    public async Task IniciarAsync_SemRegistroAtivo_CriaNovoRegistroEmAndamento()
    {
        _repository.ObterEmAndamentoAsync(Arg.Any<CancellationToken>()).Returns((RegistroDeHoras?)null);

        var resultado = await _sut.IniciarAsync(7);

        resultado.ProjetoId.Should().Be(7);
        resultado.Status.Should().Be(StatusRegistroHoras.EmAndamento);
        resultado.Origem.Should().Be(OrigemRegistroHoras.Cronometro);
        await _repository.Received(1).AdicionarAsync(Arg.Any<RegistroDeHoras>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IniciarAsync_ComRegistroJaAtivo_LancaInvalidOperationException()
    {
        _repository.ObterEmAndamentoAsync(Arg.Any<CancellationToken>())
            .Returns(new RegistroDeHoras { Id = 1, ProjetoId = 3, Status = StatusRegistroHoras.EmAndamento });

        var acao = () => _sut.IniciarAsync(7);

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PausarAsync_RegistroEmAndamento_MudaParaPausadoEAbrePausa()
    {
        var registro = new RegistroDeHoras { Id = 1, ProjetoId = 1, Status = StatusRegistroHoras.EmAndamento, InicioSessao = DateTime.Now };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(registro);

        var resultado = await _sut.PausarAsync(1);

        resultado.Status.Should().Be(StatusRegistroHoras.Pausado);
        registro.Pausas.Should().ContainSingle(p => p.FimPausa == null);
    }

    [Fact]
    public async Task PausarAsync_RegistroJaPausado_LancaInvalidOperationException()
    {
        var registro = new RegistroDeHoras { Id = 1, ProjetoId = 1, Status = StatusRegistroHoras.Pausado, InicioSessao = DateTime.Now };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(registro);

        var acao = () => _sut.PausarAsync(1);

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RetomarAsync_RegistroPausado_FechaPausaEVoltaParaEmAndamento()
    {
        var registro = new RegistroDeHoras
        {
            Id = 1,
            ProjetoId = 1,
            Status = StatusRegistroHoras.Pausado,
            InicioSessao = DateTime.Now.AddMinutes(-30),
            Pausas = [new PausaRegistroHoras { InicioPausa = DateTime.Now.AddMinutes(-5) }]
        };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(registro);

        var resultado = await _sut.RetomarAsync(1);

        resultado.Status.Should().Be(StatusRegistroHoras.EmAndamento);
        registro.Pausas.Single().FimPausa.Should().NotBeNull();
    }

    [Fact]
    public async Task PararAsync_RegistroEmAndamento_CalculaDuracaoEConcluiComCategoriaENota()
    {
        var registro = new RegistroDeHoras
        {
            Id = 1,
            ProjetoId = 1,
            Status = StatusRegistroHoras.EmAndamento,
            InicioSessao = DateTime.Now.AddHours(-2)
        };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(registro);

        var resultado = await _sut.PararAsync(1, CategoriaHoras.VisitaObra, "Visita de acompanhamento");

        resultado.Status.Should().Be(StatusRegistroHoras.Concluido);
        resultado.Categoria.Should().Be(CategoriaHoras.VisitaObra);
        resultado.Nota.Should().Be("Visita de acompanhamento");
        resultado.DuracaoMinutos.Should().BeInRange(119, 121);
        resultado.FimSessao.Should().NotBeNull();
    }

    [Fact]
    public async Task PararAsync_RegistroJaConcluido_LancaInvalidOperationException()
    {
        var registro = new RegistroDeHoras { Id = 1, ProjetoId = 1, Status = StatusRegistroHoras.Concluido, InicioSessao = DateTime.Now };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(registro);

        var acao = () => _sut.PararAsync(1, null, null);

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RegistrarManualAsync_ComUmSegmento_ArmazenaCategoriaENotaNoRegistroPai()
    {
        var segmentos = new List<SegmentoInput>
        {
            new() { DuracaoMinutos = 90, Categoria = CategoriaHoras.Administrativo, Nota = "Organização" }
        };

        var resultado = await _sut.RegistrarManualAsync(4, DateTime.Now, segmentos);

        resultado.DuracaoMinutos.Should().Be(90);
        resultado.Categoria.Should().Be(CategoriaHoras.Administrativo);
        resultado.Nota.Should().Be("Organização");
        resultado.Segmentos.Should().BeEmpty();
        resultado.Origem.Should().Be(OrigemRegistroHoras.Manual);
        resultado.Status.Should().Be(StatusRegistroHoras.Concluido);
    }

    [Fact]
    public async Task RegistrarManualAsync_ComMultiplosSegmentos_SomaBateEZeraCategoriaDoPai()
    {
        var segmentos = new List<SegmentoInput>
        {
            new() { DuracaoMinutos = 60, Categoria = CategoriaHoras.ReuniaoCliente, Nota = "Briefing" },
            new() { DuracaoMinutos = 60, Categoria = CategoriaHoras.Administrativo, Nota = "Organização" }
        };

        var resultado = await _sut.RegistrarManualAsync(4, DateTime.Now, segmentos);

        resultado.DuracaoMinutos.Should().Be(120);
        resultado.Categoria.Should().BeNull();
        resultado.Nota.Should().BeNull();
        resultado.Segmentos.Should().HaveCount(2);
        resultado.Segmentos.Sum(s => s.DuracaoMinutos).Should().Be(resultado.DuracaoMinutos);
    }

    [Fact]
    public async Task RegistrarManualAsync_SemSegmentosComDuracaoValida_LancaArgumentException()
    {
        var acao = () => _sut.RegistrarManualAsync(4, DateTime.Now, [new SegmentoInput { DuracaoMinutos = 0 }]);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SalvarSegmentosAsync_DividindoSessaoExistente_RecalculaDuracaoTotalComoSomaDosTrechos()
    {
        var registro = new RegistroDeHoras
        {
            Id = 9,
            ProjetoId = 1,
            Status = StatusRegistroHoras.Concluido,
            InicioSessao = DateTime.Now.AddHours(-3),
            FimSessao = DateTime.Now,
            DuracaoMinutos = 180,
            Categoria = CategoriaHoras.Detalhamento
        };
        _repository.ObterPorIdAsync(9, Arg.Any<CancellationToken>()).Returns(registro);

        var novosSegmentos = new List<SegmentoInput>
        {
            new() { DuracaoMinutos = 100, Categoria = CategoriaHoras.Detalhamento, Nota = "Elétrico" },
            new() { DuracaoMinutos = 80, Categoria = CategoriaHoras.ReuniaoCliente, Nota = "Alinhamento" }
        };

        var resultado = await _sut.SalvarSegmentosAsync(9, novosSegmentos);

        resultado.DuracaoMinutos.Should().Be(180);
        resultado.Segmentos.Sum(s => s.DuracaoMinutos).Should().Be(resultado.DuracaoMinutos);
        resultado.Categoria.Should().BeNull();
    }

    [Fact]
    public async Task SalvarSegmentosAsync_RegistroInexistente_LancaKeyNotFoundException()
    {
        _repository.ObterPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((RegistroDeHoras?)null);

        var acao = () => _sut.SalvarSegmentosAsync(99, [new SegmentoInput { DuracaoMinutos = 30 }]);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExcluirAsync_RegistroExistente_RemoveEPersisteAlteracao()
    {
        var registro = new RegistroDeHoras { Id = 5, ProjetoId = 1, InicioSessao = DateTime.Now };
        _repository.ObterPorIdAsync(5, Arg.Any<CancellationToken>()).Returns(registro);

        await _sut.ExcluirAsync(5);

        _repository.Received(1).Remover(registro);
        await _repository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObterValorPorHoraAsync_ComRegistrosConcluidos_CalculaValorEfetivoDaHora()
    {
        var registros = new List<RegistroDeHoras>
        {
            new() { Id = 1, ProjetoId = 1, Status = StatusRegistroHoras.Concluido, DuracaoMinutos = 300, InicioSessao = DateTime.Now },
            new() { Id = 2, ProjetoId = 1, Status = StatusRegistroHoras.EmAndamento, DuracaoMinutos = 0, InicioSessao = DateTime.Now }
        };
        _repository.ListarPorProjetoAsync(1, Arg.Any<CancellationToken>()).Returns(registros);

        var valorHora = await _sut.ObterValorPorHoraAsync(1, 2500m);

        valorHora.Should().Be(500m);
    }
}
