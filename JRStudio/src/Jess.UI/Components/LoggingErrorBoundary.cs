using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace Jess.UI.Components;

public class LoggingErrorBoundary : ErrorBoundary
{
    [Microsoft.AspNetCore.Components.Inject]
    private ILogger<LoggingErrorBoundary> Logger { get; set; } = null!;

    protected override Task OnErrorAsync(Exception exception)
    {
        Logger.LogError(exception, "Erro não tratado ao renderizar um componente");
        return base.OnErrorAsync(exception);
    }
}
