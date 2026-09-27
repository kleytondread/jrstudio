using System.Diagnostics;
using Jess.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Jess.Desktop;

public class FileDialogService : IFileDialogService
{
    private readonly ILogger<FileDialogService> _logger;

    public FileDialogService(ILogger<FileDialogService> logger)
    {
        _logger = logger;
    }

    public Task<string?> EscolherDestinoParaSalvarAsync(string nomeSugerido, string filtro)
    {
        var dialog = new SaveFileDialog
        {
            FileName = nomeSugerido,
            Filter = filtro
        };

        return Task.FromResult(dialog.ShowDialog() == true ? dialog.FileName : null);
    }

    public Task<string?> EscolherArquivoParaAbrirAsync(string filtro)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filtro
        };

        return Task.FromResult(dialog.ShowDialog() == true ? dialog.FileName : null);
    }

    public Task<string[]> EscolherArquivosParaImportarAsync(string filtro)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filtro,
            Multiselect = true
        };

        return Task.FromResult(dialog.ShowDialog() == true ? dialog.FileNames : []);
    }

    public Task AbrirLocalDoArquivoAsync(string caminhoAbsoluto)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{caminhoAbsoluto}\"") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível abrir o Explorer para o arquivo {Caminho}", caminhoAbsoluto);
        }

        return Task.CompletedTask;
    }
}
