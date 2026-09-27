using Jess.Application.Interfaces;
using Jess.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jess.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjetoService, ProjetoService>();
        services.AddScoped<ITarefaService, TarefaService>();
        services.AddScoped<IFinanceiroService, FinanceiroService>();
        services.AddScoped<IRastreamentoHorasService, RastreamentoHorasService>();
        services.AddScoped<IArquivoService, ArquivoService>();
        services.AddScoped<IConfiguracaoService, ConfiguracaoService>();
        services.AddSingleton<CronometroStateService>();
        services.AddSingleton<NotificacaoService>();
        services.AddScoped<ConfirmState>();

        return services;
    }
}
