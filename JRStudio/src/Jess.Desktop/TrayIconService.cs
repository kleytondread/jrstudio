using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using Jess.Application.Services;

namespace Jess.Desktop;

/// <summary>
/// Ícone na bandeja do sistema (Seção 2.4/7.1) — mantém o cronômetro visível via texto do tooltip
/// mesmo com a janela minimizada, sem alterar o comportamento normal de minimizar/fechar da janela
/// (o ícone fica presente o tempo todo em que o app está rodando, independente do estado da janela).
/// </summary>
public class TrayIconService : IDisposable
{
    private readonly CronometroStateService _cronometroState;
    private readonly MainWindow _mainWindow;
    private TaskbarIcon? _taskbarIcon;

    public TrayIconService(CronometroStateService cronometroState, MainWindow mainWindow)
    {
        _cronometroState = cronometroState;
        _mainWindow = mainWindow;
    }

    public void Iniciar()
    {
        var menuAbrir = new MenuItem { Header = "Abrir J.R Studio" };
        menuAbrir.Click += (_, _) => Restaurar();

        var menuSair = new MenuItem { Header = "Sair" };
        menuSair.Click += (_, _) => System.Windows.Application.Current.Shutdown();

        var menu = new ContextMenu();
        menu.Items.Add(menuAbrir);
        menu.Items.Add(new Separator());
        menu.Items.Add(menuSair);

        _taskbarIcon = new TaskbarIcon
        {
            IconSource = new BitmapImage(new Uri("pack://application:,,,/Resources/logo-mark.png")),
            ToolTipText = "J.R Studio",
            ContextMenu = menu
        };
        _taskbarIcon.TrayMouseDoubleClick += (_, _) => Restaurar();

        _cronometroState.OnChange += AtualizarTooltip;
        AtualizarTooltip();
    }

    private void Restaurar()
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    // O tique de 1 em 1 segundo do cronômetro (CronometroStateService) dispara OnChange numa thread
    // do ThreadPool, não na thread de UI — TaskbarIcon é um FrameworkElement e só pode ser tocado a
    // partir da thread que o criou. Sem esse Dispatcher.BeginInvoke, essa atribuição lançava uma
    // InvalidOperationException a cada tique; o System.Timers.Timer engole essa exceção silenciosamente
    // (por isso o app não travava), mas isso interrompia a cadeia do evento multicast antes de chegar
    // aos assinantes seguintes (TimerWidget.razor, que é quem atualiza o relógio ao vivo na sidebar) —
    // era esse o bug relatado de "relógio para de atualizar enquanto o cronômetro está rodando".
    private void AtualizarTooltip()
    {
        if (_taskbarIcon is null)
        {
            return;
        }

        var texto = _cronometroState.Ativo is null
            ? "J.R Studio"
            : $"J.R Studio — {_cronometroState.ProjetoNomeAtivo}: {_cronometroState.TempoDecorrido:hh\\:mm\\:ss}";

        _taskbarIcon.Dispatcher.BeginInvoke(() => _taskbarIcon.ToolTipText = texto);
    }

    public void Dispose()
    {
        _cronometroState.OnChange -= AtualizarTooltip;
        _taskbarIcon?.Dispose();
    }
}
