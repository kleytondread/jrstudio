using Jess.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Jess.Desktop;

/// <summary>
/// Implementação nativa de <see cref="IToastService"/> via Microsoft.Toolkit.Uwp.Notifications.
/// Ver Guia_Implementacao_IA.md ("Riscos técnicos conhecidos") — toast em app desktop não empacotado
/// (sem instalador MSIX/atalho no Menu Iniciar) é uma área conhecida de fragilidade entre versões do
/// Windows; por isso qualquer falha aqui é logada e engolida, nunca deixada estourar — uma notificação
/// que não aparece não pode derrubar o app.
/// </summary>
public class WindowsToastService : IToastService
{
    private readonly ILogger<WindowsToastService> _logger;

    public WindowsToastService(ILogger<WindowsToastService> logger)
    {
        _logger = logger;
    }

    public void Mostrar(string titulo, string mensagem)
    {
        try
        {
            new ToastContentBuilder()
                .AddText(titulo)
                .AddText(mensagem)
                .Show();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível exibir a notificação toast \"{Titulo}\"", titulo);
        }
    }
}
