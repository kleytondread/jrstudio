namespace Jess.Application.Services;

/// <summary>
/// Estado de um diálogo de confirmação no estilo visual do app (cores/fonte do tema), em vez do
/// `confirm()` nativo do navegador — que no WebView2 aparece como uma janela padrão do Windows,
/// quebrando a identidade visual. Renderizado uma única vez em <c>MainLayout</c> via
/// <c>ConfirmDialog.razor</c>; qualquer tela injeta este serviço e chama <see cref="ConfirmarAsync"/>
/// no lugar de <c>IJSRuntime.InvokeAsync&lt;bool&gt;("confirm", ...)</c>.
/// </summary>
public class ConfirmState
{
    private TaskCompletionSource<bool>? _tcs;

    public event Action? OnChange;

    public bool IsOpen { get; private set; }

    public string Titulo { get; private set; } = "Confirmar ação";

    public string Mensagem { get; private set; } = string.Empty;

    public Task<bool> ConfirmarAsync(string mensagem, string titulo = "Confirmar ação")
    {
        // Se já houver uma confirmação pendente (não deveria acontecer no fluxo normal de uso),
        // resolve como cancelada em vez de deixar a Task anterior pendurada para sempre.
        _tcs?.TrySetResult(false);

        Titulo = titulo;
        Mensagem = mensagem;
        IsOpen = true;
        _tcs = new TaskCompletionSource<bool>();

        OnChange?.Invoke();

        return _tcs.Task;
    }

    public void Responder(bool confirmado)
    {
        IsOpen = false;
        _tcs?.TrySetResult(confirmado);
        _tcs = null;

        OnChange?.Invoke();
    }
}
