using System;
using System.IO;

namespace Jess.Infrastructure;

public static class AppPaths
{
    private static readonly string RootFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "JRStudio");

    public static string DatabaseFilePath => Path.Combine(RootFolder, "dados.db");

    public static string ArquivosFolder => Path.Combine(RootFolder, "Arquivos");

    public static string LogsFolder => Path.Combine(RootFolder, "Logs");

    public static void EnsureFoldersExist()
    {
        Directory.CreateDirectory(RootFolder);
        Directory.CreateDirectory(ArquivosFolder);
        Directory.CreateDirectory(LogsFolder);
    }
}
