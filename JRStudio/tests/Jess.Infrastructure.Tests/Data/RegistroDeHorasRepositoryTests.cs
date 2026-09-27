using FluentAssertions;
using Jess.Entities.Entities;
using Jess.Entities.Enums;
using Jess.Infrastructure.Data;
using Jess.Infrastructure.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Jess.Infrastructure.Tests.Data;

/// <summary>
/// Cobre um bug real de produção: o cronômetro é iniciado/parado por um <c>AppDbContext</c> separado
/// (criado via <c>IServiceScopeFactory</c> pelo <c>CronometroStateService</c> — ver Guia_Implementacao_IA.md,
/// "Riscos técnicos conhecidos"), enquanto a tela de Horas lê pelo <c>AppDbContext</c> de vida longa da
/// sessão do BlazorWebView. Sem <c>AsNoTracking()</c>, a releitura na tela devolvia a entidade antiga já
/// presa no identity map do seu DbContext, mesmo depois do outro DbContext gravar "Concluído" no banco.
/// Um repositório mockado não pega esse tipo de bug — por isso o teste bate num SQLite real.
/// </summary>
public class RegistroDeHorasRepositoryTests : IDisposable
{
    private readonly string _dbPath;

    public RegistroDeHorasRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"JRStudioRegistroHorasTests-{Guid.NewGuid():N}.db");
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
    public async Task ListarPorProjetoAsync_AposEscritaPorOutroDbContext_ReflecteOStatusAtualizado()
    {
        int registroId;
        int projetoId;
        await using (var setup = CriarContexto())
        {
            await setup.Database.MigrateAsync();

            var cliente = new Cliente { Nome = "Cliente Teste", DataCadastro = DateTime.Now };
            setup.Clientes.Add(cliente);
            await setup.SaveChangesAsync();

            var projeto = new Projeto
            {
                Nome = "Casa Marina",
                ClienteId = cliente.Id,
                DataInicio = DateOnly.FromDateTime(DateTime.Now),
                Status = StatusProjeto.EmAndamento,
                ValorTotalAcordado = 1000m,
                DataCriacao = DateTime.Now
            };
            setup.Projetos.Add(projeto);
            await setup.SaveChangesAsync();
            projetoId = projeto.Id;
        }

        await using var contextoDaTela = CriarContexto();
        var repositorioDaTela = new RegistroDeHorasRepository(contextoDaTela);

        var registro = new RegistroDeHoras
        {
            ProjetoId = projetoId,
            InicioSessao = DateTime.Now,
            Origem = OrigemRegistroHoras.Cronometro,
            Status = StatusRegistroHoras.EmAndamento
        };
        await repositorioDaTela.AdicionarAsync(registro);
        await repositorioDaTela.SalvarAlteracoesAsync();
        registroId = registro.Id;

        // Simula a tela de Horas exibindo o cronômetro em andamento — carrega e rastreia a entidade
        // no mesmo DbContext de vida longa que ficará aberto durante toda a sessão.
        var antesDeParar = await repositorioDaTela.ListarPorProjetoAsync(projetoId);
        antesDeParar.Single().Status.Should().Be(StatusRegistroHoras.EmAndamento);

        // Simula o CronometroStateService parando o cronômetro por um DbContext separado (escopo próprio).
        await using (var contextoDoCronometro = CriarContexto())
        {
            var repositorioDoCronometro = new RegistroDeHorasRepository(contextoDoCronometro);
            var ativo = await repositorioDoCronometro.ObterPorIdAsync(registroId);
            ativo!.Status = StatusRegistroHoras.Concluido;
            ativo.FimSessao = DateTime.Now;
            ativo.DuracaoMinutos = 42;
            ativo.Categoria = CategoriaHoras.Detalhamento;
            await repositorioDoCronometro.SalvarAlteracoesAsync();
        }

        // A tela releitura pelo MESMO repositório/DbContext de antes — precisa ver o dado novo.
        var depoisDeParar = await repositorioDaTela.ListarPorProjetoAsync(projetoId);

        depoisDeParar.Single().Status.Should().Be(StatusRegistroHoras.Concluido,
            "a releitura não pode devolver a entidade antiga presa no change tracker do DbContext de vida longa");
        depoisDeParar.Single().DuracaoMinutos.Should().Be(42);
    }
}
