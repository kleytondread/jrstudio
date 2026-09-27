using Jess.Entities.Enums;

namespace Jess.Entities.Entities;

public class Arquivo
{
    public int Id { get; set; }

    public string NomeArquivo { get; set; } = string.Empty;

    /// <summary>Caminho relativo à pasta de arquivos do app (%AppData%\JRStudio\Arquivos\), não o caminho absoluto.</summary>
    public string CaminhoArquivo { get; set; } = string.Empty;

    public TipoArquivo TipoArquivo { get; set; }

    public long TamanhoBytes { get; set; }

    public DateTime DataImportacao { get; set; }

    public bool Favorito { get; set; }

    public List<Tag> Tags { get; set; } = [];

    public List<Projeto> Projetos { get; set; } = [];
}
