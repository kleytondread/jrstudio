using FluentAssertions;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Tests.Data;

/// <summary>
/// Cobre a regra de negócio da Seção 6.3: excluir um Projeto não deve excluir os Arquivos
/// vinculados a ele, só o vínculo em ArquivoProjeto. Como isso depende do comportamento real de
/// cascade delete do EF Core/SQLite (não dá pra verificar isso com um repositório mockado), o teste
/// bate num banco SQLite real, criado num arquivo temporário.
/// </summary>
public class ArquivoProjetoCascadeTests : IDisposable
{
    private readonly string _dbPath;

    public ArquivoProjetoCascadeTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"JRStudioArquivoCascadeTests-{Guid.NewGuid():N}.db");
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
    public async Task ExcluirProjeto_NaoExcluiArquivoVinculado_RemoveApenasOVinculo()
    {
        int arquivoId;
        int projetoId;

        await using (var context = CriarContexto())
        {
            await context.Database.MigrateAsync();

            var cliente = new Cliente { Nome = "Cliente Teste", DataCadastro = DateTime.Now };
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            var projeto = new Projeto
            {
                Nome = "Casa Marina",
                ClienteId = cliente.Id,
                DataInicio = DateOnly.FromDateTime(DateTime.Now),
                Status = StatusProjeto.EmAndamento,
                ValorTotalAcordado = 1000m,
                DataCriacao = DateTime.Now
            };

            var arquivo = new Arquivo
            {
                NomeArquivo = "textura.jpg",
                CaminhoArquivo = "2026/09/15/textura.jpg",
                TipoArquivo = TipoArquivo.Imagem,
                TamanhoBytes = 1024,
                DataImportacao = DateTime.Now,
                Projetos = [projeto]
            };

            context.Projetos.Add(projeto);
            context.Arquivos.Add(arquivo);
            await context.SaveChangesAsync();

            arquivoId = arquivo.Id;
            projetoId = projeto.Id;
        }

        await using (var context = CriarContexto())
        {
            var projeto = await context.Projetos.FindAsync(projetoId);
            context.Projetos.Remove(projeto!);
            await context.SaveChangesAsync();
        }

        await using (var context = CriarContexto())
        {
            var arquivo = await context.Arquivos.Include(a => a.Projetos).FirstOrDefaultAsync(a => a.Id == arquivoId);

            arquivo.Should().NotBeNull("excluir o projeto não deve excluir o arquivo vinculado");
            arquivo!.Projetos.Should().BeEmpty("o vínculo com o projeto excluído deve ter sido removido");

            (await context.Projetos.FindAsync(projetoId)).Should().BeNull();
        }
    }
}
