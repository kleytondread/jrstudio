using System.IO;
using Serilog;
using Serilog.Events;

namespace Jess.Infrastructure.Logging;

public static class LoggingSetup
{
    public const string LogFileName = "JRStudio_logs.txt";

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const int RetainedFileCount = 10;

    private const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration Configure(LoggerConfiguration configuration)
    {
        AppPaths.EnsureFoldersExist();

        return configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: Path.Combine(AppPaths.LogsFolder, LogFileName),
                outputTemplate: OutputTemplate,
                rollingInterval: RollingInterval.Infinite,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: MaxFileSizeBytes,
                retainedFileCountLimit: RetainedFileCount,
                shared: true);
    }
}
