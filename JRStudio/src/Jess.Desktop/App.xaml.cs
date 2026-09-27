using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Jess.Application;
using Jess.Infrastructure;
using Jess.Infrastructure.Data;
using Jess.Infrastructure.Logging;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Jess.Desktop;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Log.Logger = LoggingSetup.Configure(new LoggerConfiguration()).CreateLogger();

        AplicarTemaDaBarraDeTitulo();
        RegistrarAppUserModelId();

        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        Log.Information("Aplicativo J.R Studio iniciando");

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(services =>
            {
                services.AddWpfBlazorWebView();
#if DEBUG
                services.AddBlazorWebViewDeveloperTools();
#endif
                services.AddInfrastructure();
                services.AddApplication();
                services.AddSingleton<Jess.Application.Interfaces.IFileDialogService, FileDialogService>();
                services.AddSingleton<Jess.Application.Interfaces.IAppLifecycleService, AppLifecycleService>();
                services.AddSingleton<Jess.Application.Interfaces.IToastService, WindowsToastService>();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<TrayIconService>();
            })
            .Build();

        using (var scope = _host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
            Log.Information("Banco de dados migrado com sucesso");
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        _host.Services.GetRequiredService<TrayIconService>().Iniciar();
        _host.Services.GetRequiredService<Jess.Application.Services.NotificacaoService>().Iniciar();

        Log.Information("Aplicativo J.R Studio iniciado com sucesso");
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("Aplicativo J.R Studio encerrando");

        if (_host is not null)
        {
            await PausarCronometroAtivoAsync();

            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);

        Log.CloseAndFlush();
    }

    /// <summary>
    /// Sem isso, fechar o app com um cronômetro rodando deixava o registro como "Em andamento" no
    /// banco; ao reabrir, <see cref="Jess.Application.Services.CronometroStateService.InicializarAsync"/>
    /// retoma o registro e calcula o tempo decorrido como `agora − início`, contando o período em que
    /// o app ficou fechado como se fosse trabalho de verdade. Pausar automaticamente ao sair reaproveita
    /// o mecanismo de pausa já existente (mesmo usado pelo botão "Pausar"): grava o início da pausa no
    /// banco, e a próxima abertura mostra o cronômetro parado no tempo certo, exigindo um "Retomar"
    /// explícito pra continuar contando. Só cobre saída normal (botão Sair/fechar janela) — um
    /// encerramento forçado (processo morto, queda de energia) não passa por aqui, por natureza.
    /// </summary>
    private async Task PausarCronometroAtivoAsync()
    {
        var cronometroState = _host!.Services.GetRequiredService<Jess.Application.Services.CronometroStateService>();
        if (cronometroState.Ativo?.Status != Jess.Entities.Enums.StatusRegistroHoras.EmAndamento)
        {
            return;
        }

        try
        {
            await cronometroState.PausarAsync();
            Log.Information("Cronômetro pausado automaticamente ao encerrar o aplicativo");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Não foi possível pausar o cronômetro automaticamente ao encerrar");
        }
    }

    /// <summary>
    /// A barra de título é WPF nativo (fora do WebView2), então não enxerga `prefers-color-scheme`
    /// nem as variáveis CSS do app — o tema é lido uma vez do registro do Windows, na inicialização,
    /// e as cores são trocadas nos recursos definidos em App.xaml (espelhando app.css). Se a usuária
    /// mudar o tema do Windows com o app já aberto, a barra de título só atualiza no próximo início.
    /// </summary>
    private void AplicarTemaDaBarraDeTitulo()
    {
        var temaEscuro = false;
        try
        {
            using var chave = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            temaEscuro = chave?.GetValue("AppsUseLightTheme") is 0;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Não foi possível ler o tema do Windows no registro — usando tema claro na barra de título");
        }

        if (!temaEscuro)
        {
            return;
        }

        Resources["TitleBarBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(0x14, 0x17, 0x1A));
        Resources["TitleBarForegroundBrush"] = new SolidColorBrush(Color.FromRgb(0xED, 0xEE, 0xEC));
        Resources["TitleBarButtonHoverBrush"] = new SolidColorBrush(Color.FromRgb(0x23, 0x28, 0x2A));
        Resources["TitleBarCloseHoverBrush"] = new SolidColorBrush(Color.FromRgb(0xE0, 0x84, 0x68));
    }

    /// <summary>
    /// Notificações toast do Windows são atribuídas por AppUserModelID (AUMID). Apps empacotados
    /// (MSIX) ganham um automaticamente; um app desktop solto como este (rodando direto do .exe, sem
    /// instalador) precisa registrar o processo explicitamente — sem isso, o toast tende a falhar
    /// silenciosamente ou aparecer sem nome/ícone corretos. Não precisa de atalho no Menu Iniciar para
    /// esse registro básico funcionar. Ver Guia_Implementacao_IA.md, "Riscos técnicos conhecidos".
    /// </summary>
    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string appId);

    private static void RegistrarAppUserModelId()
    {
        try
        {
            SetCurrentProcessExplicitAppUserModelID("JRStudio.App");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Não foi possível registrar o AppUserModelID — notificações toast podem não funcionar corretamente");
        }
    }

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.ExceptionObject as Exception, "Erro fatal não tratado (AppDomain) — o aplicativo será encerrado");
        Log.CloseAndFlush();
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Exceção não observada em tarefa assíncrona em segundo plano");
        e.SetObserved();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Erro não tratado na interface do aplicativo — o aplicativo será encerrado");
        e.Handled = true;

        MessageBox.Show(
            "Ocorreu um erro inesperado e o aplicativo precisa ser fechado. Os detalhes foram registrados no arquivo de log.",
            "J.R Studio — Erro inesperado",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        Shutdown(1);
    }
}
