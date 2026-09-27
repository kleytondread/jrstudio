using Jess.Entities.Enums;

namespace Jess.Application.DTOs;

public class ArquivoDto
{
    public int Id { get; set; }

    public string NomeArquivo { get; set; } = string.Empty;

    public string CaminhoRelativo { get; set; } = string.Empty;

    public TipoArquivo TipoArquivo { get; set; }

    public long TamanhoBytes { get; set; }

    public DateTime DataImportacao { get; set; }

    public bool Favorito { get; set; }

    public List<string> Tags { get; set; } = [];

    public List<int> ProjetoIds { get; set; } = [];
}
