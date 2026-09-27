using FluentAssertions;
using Jess.Application.Services;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Entities.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jess.Application.Tests.Services;

public class ArquivoServiceTests
{
    private readonly IArquivoRepository _repository = Substitute.For<IArquivoRepository>();
    private readonly IFileStorageService _fileStorage = Substitute.For<IFileStorageService>();
    private readonly ArquivoService _sut;

    public ArquivoServiceTests()
    {
        _sut = new ArquivoService(_repository, _fileStorage, Substitute.For<ILogger<ArquivoService>>());
    }

    [Fact]
    public async Task ImportarAsync_ComTagsEProjetos_CriaArquivoVinculadoComTipoInferidoPelaExtensao()
    {
        _fileStorage.Armazenar(Arg.Any<string>()).Returns(("2026/09/15/foto.jpg", 2048L));
        _repository.ObterOuCriarTagAsync("madeira", Arg.Any<CancellationToken>()).Returns(new Tag { Id = 1, Nome = "madeira" });
        _repository.ObterProjetoAsync(9, Arg.Any<CancellationToken>()).Returns(new Projeto { Id = 9, Nome = "Casa Marina" });

        var resultado = await _sut.ImportarAsync("C:\\origem\\foto.jpg", projetoIds: [9], tags: ["madeira"]);

        resultado.NomeArquivo.Should().Be("foto.jpg");
        resultado.TipoArquivo.Should().Be(TipoArquivo.Imagem);
        resultado.CaminhoRelativo.Should().Be("2026/09/15/foto.jpg");
        resultado.TamanhoBytes.Should().Be(2048L);
        resultado.Tags.Should().Contain("madeira");
        resultado.ProjetoIds.Should().Contain(9);
        await _repository.Received(1).AdicionarAsync(Arg.Any<Arquivo>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("relatorio.pdf", TipoArquivo.PDF)]
    [InlineData("memorial.docx", TipoArquivo.Documento)]
    [InlineData("planilha.xlsx", TipoArquivo.Outro)]
    public async Task ImportarAsync_InfereTipoPelaExtensao(string nomeArquivo, TipoArquivo tipoEsperado)
    {
        _fileStorage.Armazenar(Arg.Any<string>()).Returns(($"2026/09/15/{nomeArquivo}", 100L));

        var resultado = await _sut.ImportarAsync($"C:\\origem\\{nomeArquivo}");

        resultado.TipoArquivo.Should().Be(tipoEsperado);
    }

    [Fact]
    public async Task VincularProjetoAsync_ProjetoAindaNaoVinculado_Adiciona()
    {
        var arquivo = new Arquivo { Id = 1, NomeArquivo = "a.jpg", CaminhoArquivo = "x" };
        var projeto = new Projeto { Id = 5, Nome = "Loft" };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(arquivo);
        _repository.ObterProjetoAsync(5, Arg.Any<CancellationToken>()).Returns(projeto);

        await _sut.VincularProjetoAsync(1, 5);

        arquivo.Projetos.Should().ContainSingle(p => p.Id == 5);
        await _repository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VincularProjetoAsync_ProjetoJaVinculado_NaoDuplicaNemSalva()
    {
        var projeto = new Projeto { Id = 5, Nome = "Loft" };
        var arquivo = new Arquivo { Id = 1, NomeArquivo = "a.jpg", CaminhoArquivo = "x", Projetos = [projeto] };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(arquivo);

        await _sut.VincularProjetoAsync(1, 5);

        arquivo.Projetos.Should().ContainSingle();
        await _repository.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DesvincularProjetoAsync_RemoveApenasOVinculo_NuncaChamaRemoverDoArquivo()
    {
        var projeto = new Projeto { Id = 5, Nome = "Loft" };
        var arquivo = new Arquivo { Id = 1, NomeArquivo = "a.jpg", CaminhoArquivo = "x", Projetos = [projeto] };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(arquivo);

        await _sut.DesvincularProjetoAsync(1, 5);

        arquivo.Projetos.Should().BeEmpty();
        _repository.DidNotReceive().Remover(Arg.Any<Arquivo>());
        await _repository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarTagsAsync_SubstituiAsTagsExistentesPelasNovas()
    {
        var tagAntiga = new Tag { Id = 1, Nome = "antiga" };
        var arquivo = new Arquivo { Id = 1, NomeArquivo = "a.jpg", CaminhoArquivo = "x", Tags = [tagAntiga] };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(arquivo);
        _repository.ObterOuCriarTagAsync("nova", Arg.Any<CancellationToken>()).Returns(new Tag { Id = 2, Nome = "nova" });

        await _sut.AtualizarTagsAsync(1, ["nova"]);

        arquivo.Tags.Should().ContainSingle(t => t.Nome == "nova");
    }

    [Fact]
    public async Task AlternarFavoritoAsync_InverteOValorAtual()
    {
        var arquivo = new Arquivo { Id = 1, NomeArquivo = "a.jpg", CaminhoArquivo = "x", Favorito = false };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(arquivo);

        await _sut.AlternarFavoritoAsync(1);

        arquivo.Favorito.Should().BeTrue();
    }

    [Fact]
    public async Task ExcluirAsync_RemoveRegistroEArquivoFisico()
    {
        var arquivo = new Arquivo { Id = 1, NomeArquivo = "a.jpg", CaminhoArquivo = "2026/09/15/a.jpg" };
        _repository.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(arquivo);

        await _sut.ExcluirAsync(1);

        _repository.Received(1).Remover(arquivo);
        await _repository.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        _fileStorage.Received(1).Remover("2026/09/15/a.jpg");
    }

    [Fact]
    public async Task ExcluirAsync_ArquivoInexistente_LancaKeyNotFoundException()
    {
        _repository.ObterPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Arquivo?)null);

        var acao = () => _sut.ExcluirAsync(99);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }
}
