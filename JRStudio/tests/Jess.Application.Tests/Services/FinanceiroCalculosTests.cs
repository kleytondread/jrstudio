using FluentAssertions;
using Jess.Application.Services;
using Jess.Entities.Enums;

namespace Jess.Application.Tests.Services;

public class FinanceiroCalculosTests
{
    [Fact]
    public void CalcularStatus_ComValorRecebidoPreenchido_RetornaRecebido()
    {
        var status = FinanceiroCalculos.CalcularStatus(1500m, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 1));

        status.Should().Be(StatusPagamento.Recebido);
    }

    [Fact]
    public void CalcularStatus_SemValorRecebidoEDataPrevistaNoPassado_RetornaAtrasado()
    {
        var status = FinanceiroCalculos.CalcularStatus(null, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 1));

        status.Should().Be(StatusPagamento.Atrasado);
    }

    [Fact]
    public void CalcularStatus_SemValorRecebidoEDataPrevistaNoFuturo_RetornaPendente()
    {
        var status = FinanceiroCalculos.CalcularStatus(null, new DateOnly(2026, 12, 1), new DateOnly(2026, 6, 1));

        status.Should().Be(StatusPagamento.Pendente);
    }

    [Fact]
    public void CalcularStatus_SemValorRecebidoEDataPrevistaHoje_RetornaPendente()
    {
        var hoje = new DateOnly(2026, 6, 1);

        var status = FinanceiroCalculos.CalcularStatus(null, hoje, hoje);

        status.Should().Be(StatusPagamento.Pendente);
    }

    [Fact]
    public void CalcularSaldo_ComPagamentosParciais_RetornaValorRestante()
    {
        var saldo = FinanceiroCalculos.CalcularSaldo(15000m, [3000m, 3000m, null]);

        saldo.Should().Be(9000m);
    }

    [Fact]
    public void CalcularSaldo_SemPagamentosRecebidos_RetornaValorTotal()
    {
        var saldo = FinanceiroCalculos.CalcularSaldo(15000m, [null, null]);

        saldo.Should().Be(15000m);
    }
}
