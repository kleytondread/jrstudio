using System.Diagnostics;
using Jess.Application.Interfaces;

namespace Jess.Desktop;

public class AppLifecycleService : IAppLifecycleService
{
    public void Reiniciar()
    {
        var exePath = Environment.ProcessPath;
        if (exePath is not null)
        {
            Process.Start(exePath);
        }

        System.Windows.Application.Current.Shutdown();
    }
}
