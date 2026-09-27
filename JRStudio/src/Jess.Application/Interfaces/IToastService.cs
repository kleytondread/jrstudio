namespace Jess.Application.Interfaces;

/// <summary>
/// Ponte para notificações nativas (toast) do Windows — o Blazor/WebView não expõe isso, então é
/// implementado no host (Jess.Desktop, via Microsoft.Toolkit.Uwp.Notifications).
/// </summary>
public interface IToastService
{
    void Mostrar(string titulo, string mensagem);
}
