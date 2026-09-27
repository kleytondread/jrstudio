using Jess.Entities.Interfaces;
using Jess.Infrastructure.Backup;
using Jess.Infrastructure.Data;
using Jess.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jess.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        AppPaths.EnsureFoldersExist();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={AppPaths.DatabaseFilePath}"));

        services.AddScoped<IProjetoRepository, ProjetoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ITarefaRepository, TarefaRepository>();
        services.AddScoped<IPagamentoRepository, PagamentoRepository>();
        services.AddScoped<IRegistroDeHorasRepository, RegistroDeHorasRepository>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IArquivoRepository, ArquivoRepository>();
        services.AddScoped<IConfiguracaoRepository, ConfiguracaoRepository>();
        services.AddSingleton<IFileStorageService, FileSystem.FileStorageService>();

        return services;
    }
}
