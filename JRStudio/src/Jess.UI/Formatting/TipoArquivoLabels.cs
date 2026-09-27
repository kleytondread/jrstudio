using Jess.Entities.Enums;

namespace Jess.UI.Formatting;

public static class TipoArquivoLabels
{
    public static string Texto(TipoArquivo tipo) => tipo switch
    {
        TipoArquivo.Imagem => "Imagem",
        TipoArquivo.Textura => "Textura",
        TipoArquivo.PDF => "PDF",
        TipoArquivo.Documento => "Documento",
        _ => "Outro"
    };

    /// <summary>Legenda curta mostrada na miniatura de arquivos sem preview de imagem.</summary>
    public static string Sigla(TipoArquivo tipo) => tipo switch
    {
        TipoArquivo.PDF => "PDF",
        TipoArquivo.Documento => "DOC",
        _ => "ARQ"
    };
}
