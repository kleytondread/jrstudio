using FluentAssertions;
using Jess.Application.DTOs;
using Jess.Application.Services;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jess.Application.Tests.Services;

public class FinanceiroServiceTests
{
    private readonly IPagamentoRepository _pagamentoRepository = Substitute.For<IPagamentoRepository>();
    private readonly FinanceiroService _sut;

    public FinanceiroServiceTests()
    {
        _sut = new FinanceiroService(_pagamentoRepository, Substitute.For<ILogger<FinanceiroService>>());
    }

    [Fact]
    public async Task ListarPorProjetoAsync_RetornaPagamentosComStatusCalculado()
    {
        var pagamentos = new List<Pagamento>
        {
            new() { Id = 1, ProjetoId = 1, ValorPrevisto = 3000m, DataPrevista = DateOnly.FromDateTime(DateTime.Now).AddDays(-5) },
            new() { Id = 2, ProjetoId = 1, ValorPrevisto = 3000m, DataPrevista = DateOnly.FromDateTime(DateTime.Now), ValorRecebido = 3000m, DataRecebimento = DateOnly.FromDateTime(DateTime.Now) }
        };
        _pagamentoRepository.ListarPorProjetoAsync(1, Arg.Any<CancellationToken>()).Returns(pagamentos);

        var resultado = await _sut.ListarPorProjetoAsync(1);

        resultado.Should().HaveCount(2);
        resultado.Single(p => p.Id == 1).Status.Should().Be(StatusPagamento.Atrasado);
        resultado.Single(p => p.Id == 2).Status.Should().Be(StatusPagamento.Recebido);
    }

    [Fact]
    public async Task ListarTodosAsync_ComPagamentosDeVariosProjetos_RetornaTodosMapeados()
    {
        var pagamentos = new List<Pagamento>
        {
            new() { Id = 1, ProjetoId = 1, ValorPrevisto = 1000m, DataPrevista = DateOnly.FromDateTime(DateTime.Now) },
            new() { Id = 2, ProjetoId = 2, ValorPrevisto = 2000m, DataPrevista = DateOnly.FromDateTime(DateTime.Now) }
        };
        _pagamentoRepository.ListarTodosAsync(Arg.Any<CancellationToken>()).Returns(pagamentos);

        var resultado = await _sut.ListarTodosAsync();

        resultado.Should().HaveCount(2);
        resultado.Select(p => p.ProjetoId).Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_PersisteEAssociaAoProjeto()
    {
        var resultado = await _sut.CriarAsync(7, new SalvarPagamentoRequest
        {
            ValorPrevisto = 5000m,
            DataPrevista = new DateOnly(2026, 10, 1)
        });

        resultado.ProjetoId.Should().Be(7);
        resultado.ValorPrevisto.Should().Be(5000m);
        resultado.Status.Should().Be(StatusPagamento.Pendente);
        await _pagamentoRepository.Received(1).AdicionarAsync(
            Arg.Is<Pagamento>(p => p.ProjetoId == 7 && p.ValorPrevisto == 5000m),
            Arg.Any<CancellationToken>());
        await _pagamentoRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarAsync_PagamentoExistente_AtualizaCampos()
    {
        var pagamento = new Pagamento { Id = 3, ProjetoId = 1, ValorPrevisto = 1000m, DataPrevista = new DateOnly(2026, 1, 1) };
        _pagamentoRepository.ObterPorIdAsync(3, Arg.Any<CancellationToken>()).Returns(pagamento);

        var resultado = await _sut.AtualizarAsync(3, new SalvarPagamentoRequest { ValorPrevisto = 2000m, DataPrevista = new DateOnly(2026, 2, 1) });

        resultado.ValorPrevisto.Should().Be(2000m);
        resultado.DataPrevista.Should().Be(new DateOnly(2026, 2, 1));
        await _pagamentoRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarAsync_PagamentoInexistente_LancaKeyNotFoundException()
    {
        _pagamentoRepository.ObterPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Pagamento?)null);

        var acao = () => _sut.AtualizarAsync(99, new SalvarPagamentoRequest { ValorPrevisto = 1m, DataPrevista = new DateOnly(2026, 1, 1) });

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExcluirAsync_PagamentoExistente_RemoveEPersisteAlteracao()
    {
        var pagamento = new Pagamento { Id = 5, ProjetoId = 1, ValorPrevisto = 1000m, DataPrevista = new DateOnly(2026, 1, 1) };
        _pagamentoRepository.ObterPorIdAsync(5, Arg.Any<CancellationToken>()).Returns(pagamento);

        await _sut.ExcluirAsync(5);

        _pagamentoRepository.Received(1).Remover(pagamento);
        await _pagamentoRepository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExcluirAsync_PagamentoInexistente_LancaKeyNotFoundException()
    {
        _pagamentoRepository.ObterPorIdAsync(123, Arg.Any<CancellationToken>()).Returns((Pagamento?)null);

        var acao = () => _sut.ExcluirAsync(123);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task MarcarComoRecebidoAsync_DefineValorRecebidoIgualAoPrevistoEDataDeHoje()
    {
        var pagamento = new Pagamento { Id = 8, ProjetoId = 1, ValorPrevisto = 4000m, DataPrevista = new DateOnly(2026, 1, 1) };
        _pagamentoRepository.ObterPorIdAsync(8, Arg.Any<CancellationToken>()).Returns(pagamento);

        var resultado = await _sut.MarcarComoRecebidoAsync(8);

        resultado.ValorRecebido.Should().Be(4000m);
        resultado.DataRecebimento.Should().Be(DateOnly.FromDateTime(DateTime.Now));
        resultado.Status.Should().Be(StatusPagamento.Recebido);
    }

    [Fact]
    public async Task MarcarComoRecebidoAsync_PagamentoInexistente_LancaKeyNotFoundException()
    {
        _pagamentoRepository.ObterPorIdAsync(50, Arg.Any<CancellationToken>()).Returns((Pagamento?)null);

        var acao = () => _sut.MarcarComoRecebidoAsync(50);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ObterSaldoAsync_RetornaValorTotalMenosSomaDosRecebidos()
    {
        var pagamentos = new List<Pagamento>
        {
            new() { Id = 1, ProjetoId = 1, ValorPrevisto = 5000m, DataPrevista = new DateOnly(2026, 1, 1), ValorRecebido = 5000m },
            new() { Id = 2, ProjetoId = 1, ValorPrevisto = 5000m, DataPrevista = new DateOnly(2026, 2, 1) }
        };
        _pagamentoRepository.ListarPorProjetoAsync(1, Arg.Any<CancellationToken>()).Returns(pagamentos);

        var saldo = await _sut.ObterSaldoAsync(1, 15000m);

        saldo.Should().Be(10000m);
    }
}
