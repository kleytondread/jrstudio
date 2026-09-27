using FluentAssertions;
using Jess.Application.DTOs;
using Jess.Application.Services;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jess.Application.Tests.Services;

public class ProjetoServiceTests
{
    private readonly IProjetoRepository _projetoRepository = Substitute.For<IProjetoRepository>();
    private readonly IClienteRepository _clienteRepository = Substitute.For<IClienteRepository>();
    private readonly ProjetoService _sut;

    public ProjetoServiceTests()
    {
        _sut = new ProjetoService(_projetoRepository, _clienteRepository, Substitute.For<ILogger<ProjetoService>>());
    }

    private static Cliente CriarCliente(int id = 1, string nome = "Fernanda Souza") =>
        new() { Id = id, Nome = nome, DataCadastro = DateTime.Now };

    private static SalvarProjetoRequest CriarRequestValido(StatusProjeto status = StatusProjeto.Orcamento) => new()
    {
        Nome = "Casa Marina",
        ClienteNome = "Fernanda Souza",
        Endereco = "Rua das Palmeiras, 120",
        DataInicio = new DateOnly(2026, 1, 10),
        PrazoEstimado = new DateOnly(2026, 11, 30),
        ValorTotalAcordado = 15000m,
        Status = status
    };

    [Fact]
    public async Task ListarAsync_ComProjetosCadastrados_RetornaTodosMapeadosComNomeDoCliente()
    {
        var cliente = CriarCliente();
        var projetos = new List<Projeto>
        {
            new() { Id = 1, Nome = "Casa Marina", Cliente = cliente, ClienteId = cliente.Id, DataInicio = new DateOnly(2026, 1, 10) }
        };
        _projetoRepository.ListarAsync(Arg.Any<CancellationToken>()).Returns(projetos);

        var resultado = await _sut.ListarAsync();

        resultado.Should().ContainSingle();
        resultado[0].Nome.Should().Be("Casa Marina");
        resultado[0].ClienteNome.Should().Be("Fernanda Souza");
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_ResolveClientePorNomeEPersisteProjeto()
    {
        var cliente = CriarCliente();
        _clienteRepository.ObterOuCriarPorNomeAsync("Fernanda Souza", Arg.Any<CancellationToken>()).Returns(cliente);

        var resultado = await _sut.CriarAsync(CriarRequestValido());

        resultado.Nome.Should().Be("Casa Marina");
        resultado.ClienteNome.Should().Be("Fernanda Souza");
        resultado.Status.Should().Be(StatusProjeto.Orcamento);
        resultado.DataConclusao.Should().BeNull();
        await _projetoRepository.Received(1).AdicionarAsync(
            Arg.Is<Projeto>(p => p.Nome == "Casa Marina" && p.ClienteId == cliente.Id),
            Arg.Any<CancellationToken>());
        await _projetoRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_ComStatusConcluido_JaDefineDataConclusao()
    {
        _clienteRepository.ObterOuCriarPorNomeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(CriarCliente());

        var resultado = await _sut.CriarAsync(CriarRequestValido(StatusProjeto.Concluido));

        resultado.DataConclusao.Should().NotBeNull();
    }

    [Fact]
    public async Task AtualizarAsync_ProjetoExistente_AtualizaCamposEMantemId()
    {
        var cliente = CriarCliente();
        var projeto = new Projeto { Id = 5, Nome = "Nome antigo", Cliente = cliente, ClienteId = cliente.Id, DataInicio = new DateOnly(2026, 1, 1) };
        _projetoRepository.ObterPorIdAsync(5, Arg.Any<CancellationToken>()).Returns(projeto);
        _clienteRepository.ObterOuCriarPorNomeAsync("Fernanda Souza", Arg.Any<CancellationToken>()).Returns(cliente);

        var resultado = await _sut.AtualizarAsync(5, CriarRequestValido());

        resultado.Id.Should().Be(5);
        resultado.Nome.Should().Be("Casa Marina");
        await _projetoRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarAsync_ProjetoInexistente_LancaKeyNotFoundException()
    {
        _projetoRepository.ObterPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Projeto?)null);

        var acao = () => _sut.AtualizarAsync(99, CriarRequestValido());

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExcluirAsync_ProjetoExistente_RemoveEPersisteAlteracao()
    {
        var projeto = new Projeto { Id = 7, Nome = "Studio 21", DataInicio = new DateOnly(2026, 1, 1) };
        _projetoRepository.ObterPorIdAsync(7, Arg.Any<CancellationToken>()).Returns(projeto);

        await _sut.ExcluirAsync(7);

        _projetoRepository.Received(1).Remover(projeto);
        await _projetoRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExcluirAsync_ProjetoInexistente_LancaKeyNotFoundException()
    {
        _projetoRepository.ObterPorIdAsync(123, Arg.Any<CancellationToken>()).Returns((Projeto?)null);

        var acao = () => _sut.ExcluirAsync(123);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AlterarStatusAsync_ParaConcluido_DefineDataConclusao()
    {
        var projeto = new Projeto { Id = 3, Nome = "Loft Central", Status = StatusProjeto.EmAndamento, DataInicio = new DateOnly(2026, 1, 1) };
        _projetoRepository.ObterPorIdAsync(3, Arg.Any<CancellationToken>()).Returns(projeto);

        var resultado = await _sut.AlterarStatusAsync(3, StatusProjeto.Concluido);

        resultado.Status.Should().Be(StatusProjeto.Concluido);
        resultado.DataConclusao.Should().NotBeNull();
    }

    [Fact]
    public async Task AlterarStatusAsync_DeConcluidoParaOutroStatus_LimpaDataConclusao()
    {
        var projeto = new Projeto
        {
            Id = 4,
            Nome = "Residência Aroeira",
            Status = StatusProjeto.Concluido,
            DataConclusao = DateTime.Now,
            DataInicio = new DateOnly(2026, 1, 1)
        };
        _projetoRepository.ObterPorIdAsync(4, Arg.Any<CancellationToken>()).Returns(projeto);

        var resultado = await _sut.AlterarStatusAsync(4, StatusProjeto.Pausado);

        resultado.Status.Should().Be(StatusProjeto.Pausado);
        resultado.DataConclusao.Should().BeNull();
    }
}
