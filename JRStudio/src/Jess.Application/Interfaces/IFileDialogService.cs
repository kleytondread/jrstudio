namespace Jess.Application.Interfaces;

/// <summary>
/// Ponte para diálogos nativos de arquivo do sistema operacional — o Blazor/WebView não tem
/// diálogo de arquivo nativo próprio, então isso é implementado no host (Jess.Desktop, via WPF).
/// </summary>
public interface IFileDialogService
{
    /// <summary>Abre o diálogo "Salvar como" nativo. Retorna null se a usuária cancelar.</summary>
    Task<string?> EscolherDestinoParaSalvarAsync(string nomeSugerido, string filtro);

    /// <summary>Abre o diálogo "Abrir arquivo" nativo. Retorna null se a usuária cancelar.</summary>
    Task<string?> EscolherArquivoParaAbrirAsync(string filtro);

    /// <summary>Abre o diálogo "Abrir arquivo" nativo com seleção múltipla. Retorna vazio se a usuária cancelar.</summary>
    Task<string[]> EscolherArquivosParaImportarAsync(string filtro);

    /// <summary>Abre o Explorer do Windows já com o arquivo selecionado (equivalente a "Mostrar na pasta" do clique direito).</summary>
    Task AbrirLocalDoArquivoAsync(string caminhoAbsoluto);
}
