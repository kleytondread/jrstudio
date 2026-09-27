using FluentAssertions;
using Jess.Infrastructure.Data;
using Jess.Infrastructure.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Tests.Data;

/// <summary>
/// Reportado pela usuária: salvar o nome em Configurações não refletia na saudação do Dashboard.
/// Bate num SQLite real pra confirmar (ou descartar) um problema real de persistência — um
/// repositório mockado não provaria nada aqui, já que o bug relatado é justamente "não gravou".
/// </summary>
public class ConfiguracaoRepositoryTests : IDisposable
{
    private readonly string _dbPath;

    public ConfiguracaoRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"JRStudioConfiguracaoTests-{Guid.NewGuid():N}.db");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    private AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={_dbPath}").Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ObterOuCriarAsync_DepoisDeAtualizarNomeEDeSalvar_PersisteNaTabela()
    {
        await using (var setup = CriarContexto())
        {
            await setup.Database.MigrateAsync();
        }

        // Simula o Dashboard carregando a saudação antes de qualquer edição — cria a linha única.
        await using (var contextoDashboard = CriarContexto())
        {
            var repositorio = new ConfiguracaoRepository(contextoDashboard);
            var configuracao = await repositorio.ObterOuCriarAsync();
            configuracao.NomeArquiteto.Should().BeNull();
        }

        // Simula a tela de Configurações salvando o nome.
        await using (var contextoConfiguracoes = CriarContexto())
        {
            var repositorio = new ConfiguracaoRepository(contextoConfiguracoes);
            var configuracao = await repositorio.ObterOuCriarAsync();
            configuracao.NomeArquiteto = "Jéssica";
            await repositorio.SalvarAlteracoesAsync();
        }

        // Uma terceira leitura, por um DbContext novo, confirma que gravou de verdade no arquivo .db.
        await using var contextoVerificacao = CriarContexto();
        var repositorioVerificacao = new ConfiguracaoRepository(contextoVerificacao);
        var resultado = await repositorioVerificacao.ObterOuCriarAsync();

        resultado.NomeArquiteto.Should().Be("Jéssica");
    }
}
