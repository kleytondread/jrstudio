using System.IO.Compression;
using FluentAssertions;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Infrastructure.Backup;
using Jess.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jess.Infrastructure.Tests.Backup;

public class BackupServiceTests : IDisposable
{
    private readonly string _root;
    private readonly ILogger<BackupService> _logger = Substitute.For<ILogger<BackupService>>();

    public BackupServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"JRStudioBackupTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        // Os AppDbContext criados nos testes (SQLite) deixam conexões em pool com o arquivo aberto
        // mesmo depois do DisposeAsync — sem isso, a limpeza da pasta abaixo falha com "arquivo em uso".
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private string Caminho(params string[] partes) => Path.Combine(_root, Path.Combine(partes));

    private static async Task CriarBancoComProjetoAsync(string dbPath, string nomeProjeto)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={dbPath}").Options;
        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        var cliente = new Cliente { Nome = "Cliente Teste", DataCadastro = DateTime.Now };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        context.Projetos.Add(new Projeto
        {
            Nome = nomeProjeto,
            ClienteId = cliente.Id,
            DataInicio = DateOnly.FromDateTime(DateTime.Now),
            Status = StatusProjeto.EmAndamento,
            ValorTotalAcordado = 1000m,
            DataCriacao = DateTime.Now
        });
        await context.SaveChangesAsync();
    }

    private static async Task<List<string>> ListarNomesProjetosAsync(string dbPath)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={dbPath}").Options;
        await using var context = new AppDbContext(options);
        return await context.Projetos.Select(p => p.Nome).ToListAsync();
    }

    [Fact]
    public async Task CriarERestaurarBackupCompleto_ResultaNoMesmoEstadoDeDadosEArquivos()
    {
        var dbOrigem = Caminho("origem", "dados.db");
        var arquivosOrigem = Caminho("origem", "Arquivos");
        Directory.CreateDirectory(Path.GetDirectoryName(dbOrigem)!);
        Directory.CreateDirectory(arquivosOrigem);
        await File.WriteAllTextAsync(Path.Combine(arquivosOrigem, "textura.txt"), "conteudo");

        await CriarBancoComProjetoAsync(dbOrigem, "Casa Marina");

        var servicoOrigem = new BackupService(dbOrigem, arquivosOrigem, _logger);
        var zipPath = Caminho("backup-completo.zip");
        await servicoOrigem.CriarBackupAsync(TipoBackup.Completo, zipPath);

        var dbDestino = Caminho("destino", "dados.db");
        var arquivosDestino = Caminho("destino", "Arquivos");
        var servicoDestino = new BackupService(dbDestino, arquivosDestino, _logger);

        var tipoRestaurado = await servicoDestino.RestaurarBackupAsync(zipPath);

        tipoRestaurado.Should().Be(TipoBackup.Completo);
        (await ListarNomesProjetosAsync(dbDestino)).Should().Contain("Casa Marina");
        File.Exists(Path.Combine(arquivosDestino, "textura.txt")).Should().BeTrue();
    }

    [Fact]
    public async Task CriarERestaurarBackupSoDados_RestauraOBancoSemCopiarArquivos()
    {
        var dbOrigem = Caminho("origem2", "dados.db");
        var arquivosOrigem = Caminho("origem2", "Arquivos");
        Directory.CreateDirectory(Path.GetDirectoryName(dbOrigem)!);
        Directory.CreateDirectory(arquivosOrigem);
        await File.WriteAllTextAsync(Path.Combine(arquivosOrigem, "imagem.txt"), "conteudo");

        await CriarBancoComProjetoAsync(dbOrigem, "Loft Central");

        var servicoOrigem = new BackupService(dbOrigem, arquivosOrigem, _logger);
        var zipPath = Caminho("backup-dados.zip");
        await servicoOrigem.CriarBackupAsync(TipoBackup.SoDados, zipPath);

        var dbDestino = Caminho("destino2", "dados.db");
        var arquivosDestino = Caminho("destino2", "Arquivos");
        var servicoDestino = new BackupService(dbDestino, arquivosDestino, _logger);

        var tipoRestaurado = await servicoDestino.RestaurarBackupAsync(zipPath);

        tipoRestaurado.Should().Be(TipoBackup.SoDados);
        (await ListarNomesProjetosAsync(dbDestino)).Should().Contain("Loft Central");
        Directory.Exists(arquivosDestino).Should().BeFalse();
    }

    [Fact]
    public async Task RestaurarBackup_ZipSemBancoDeDados_LancaInvalidOperationException()
    {
        var pastaVazia = Caminho("pasta-vazia");
        Directory.CreateDirectory(pastaVazia);
        var zipInvalido = Caminho("invalido.zip");
        ZipFile.CreateFromDirectory(pastaVazia, zipInvalido);

        var servico = new BackupService(Caminho("destino3", "dados.db"), Caminho("destino3", "Arquivos"), _logger);

        var acao = () => servico.RestaurarBackupAsync(zipInvalido);

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }
}
