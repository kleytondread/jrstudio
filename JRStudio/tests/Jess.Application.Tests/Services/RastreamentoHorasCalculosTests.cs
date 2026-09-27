using FluentAssertions;
using Jess.Application.Services;

namespace Jess.Application.Tests.Services;

public class RastreamentoHorasCalculosTests
{
    [Fact]
    public void CalcularDuracaoMinutos_SemPausas_RetornaDiferencaTotal()
    {
        var inicio = new DateTime(2026, 1, 1, 9, 0, 0);
        var fim = new DateTime(2026, 1, 1, 11, 30, 0);

        var duracao = RastreamentoHorasCalculos.CalcularDuracaoMinutos(inicio, fim, []);

        duracao.Should().Be(150);
    }

    [Fact]
    public void CalcularDuracaoMinutos_ComPausaFechada_DescontaDoTotal()
    {
        var inicio = new DateTime(2026, 1, 1, 9, 0, 0);
        var fim = new DateTime(2026, 1, 1, 11, 0, 0);
        var pausas = new[] { (new DateTime(2026, 1, 1, 9, 30, 0), (DateTime?)new DateTime(2026, 1, 1, 9, 45, 0)) };

        var duracao = RastreamentoHorasCalculos.CalcularDuracaoMinutos(inicio, fim, pausas);

        duracao.Should().Be(105);
    }

    [Fact]
    public void CalcularDuracaoMinutos_ComPausaAindaAberta_DescontaAteOFimDaSessao()
    {
        var inicio = new DateTime(2026, 1, 1, 9, 0, 0);
        var fim = new DateTime(2026, 1, 1, 10, 0, 0);
        var pausas = new[] { (new DateTime(2026, 1, 1, 9, 40, 0), (DateTime?)null) };

        var duracao = RastreamentoHorasCalculos.CalcularDuracaoMinutos(inicio, fim, pausas);

        duracao.Should().Be(40);
    }

    [Fact]
    public void CalcularDuracaoMinutos_ComMultiplasPausas_DescontaTodas()
    {
        var inicio = new DateTime(2026, 1, 1, 8, 0, 0);
        var fim = new DateTime(2026, 1, 1, 12, 0, 0);
        var pausas = new[]
        {
            (new DateTime(2026, 1, 1, 9, 0, 0), (DateTime?)new DateTime(2026, 1, 1, 9, 15, 0)),
            (new DateTime(2026, 1, 1, 10, 0, 0), (DateTime?)new DateTime(2026, 1, 1, 10, 30, 0))
        };

        var duracao = RastreamentoHorasCalculos.CalcularDuracaoMinutos(inicio, fim, pausas);

        duracao.Should().Be(195);
    }

    [Fact]
    public void CalcularValorPorHora_ComMinutosPositivos_RetornaValorDividido()
    {
        var valor = RastreamentoHorasCalculos.CalcularValorPorHora(15000m, 600);

        valor.Should().Be(1500m);
    }

    [Fact]
    public void CalcularValorPorHora_SemMinutosTrabalhados_RetornaNull()
    {
        var valor = RastreamentoHorasCalculos.CalcularValorPorHora(15000m, 0);

        valor.Should().BeNull();
    }
}
