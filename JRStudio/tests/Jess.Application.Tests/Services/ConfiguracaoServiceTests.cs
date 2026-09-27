using FluentAssertions;
using Jess.Application.Services;
using Jess.Entities.Entities;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jess.Application.Tests.Services;

public class ConfiguracaoServiceTests
{
    private readonly IConfiguracaoRepository _repository = Substitute.For<IConfiguracaoRepository>();
    private readonly ConfiguracaoService _sut;

    public ConfiguracaoServiceTests()
    {
        _sut = new ConfiguracaoService(_repository, Substitute.For<ILogger<ConfiguracaoService>>());
    }

    [Fact]
    public async Task ObterAsync_RetornaONomeArmazenado()
    {
        _repository.ObterOuCriarAsync(Arg.Any<CancellationToken>()).Returns(new Configuracao { Id = 1, NomeArquiteto = "Jéssica" });

        var resultado = await _sut.ObterAsync();

        resultado.NomeArquiteto.Should().Be("Jéssica");
    }

    [Fact]
    public async Task AtualizarNomeArquitetoAsync_ComNomeValido_SalvaComTrim()
    {
        var configuracao = new Configuracao { Id = 1 };
        _repository.ObterOuCriarAsync(Arg.Any<CancellationToken>()).Returns(configuracao);

        var resultado = await _sut.AtualizarNomeArquitetoAsync("  Jéssica  ");

        resultado.NomeArquiteto.Should().Be("Jéssica");
        configuracao.NomeArquiteto.Should().Be("Jéssica");
        await _repository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AtualizarNomeArquitetoAsync_ComValorVazio_SalvaComoNulo(string? valor)
    {
        var configuracao = new Configuracao { Id = 1, NomeArquiteto = "Antigo" };
        _repository.ObterOuCriarAsync(Arg.Any<CancellationToken>()).Returns(configuracao);

        var resultado = await _sut.AtualizarNomeArquitetoAsync(valor);

        resultado.NomeArquiteto.Should().BeNull();
    }
}
