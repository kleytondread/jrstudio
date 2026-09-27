using System.Windows;

namespace Jess.Desktop;

public partial class MainWindow : Window
{
    private const string GlyphMaximize = "";
    private const string GlyphRestore = "";

    public MainWindow(IServiceProvider services)
    {
        InitializeComponent();
        BlazorWebView.Services = services;
        StateChanged += (_, _) => AtualizarIconeMaximizar();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeRestore_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void AtualizarIconeMaximizar()
    {
        var maximizado = WindowState == WindowState.Maximized;
        MaximizeRestoreButton.Content = maximizado ? GlyphRestore : GlyphMaximize;
        MaximizeRestoreButton.ToolTip = maximizado ? "Restaurar" : "Maximizar";
    }
}
