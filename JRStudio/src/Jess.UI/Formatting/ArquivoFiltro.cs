using System.Globalization;
using System.Text;
using Jess.Application.DTOs;

namespace Jess.UI.Formatting;

public static class ArquivoFiltro
{
    public static List<ArquivoDto> PorTag(List<ArquivoDto> arquivos, string? tag) =>
        tag is null
            ? arquivos
            : arquivos.Where(a => a.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();

    public static List<ArquivoDto> PorBusca(List<ArquivoDto> arquivos, string? termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            return arquivos;
        }

        var termoNormalizado = RemoverAcentos(termo);

        return arquivos
            .Where(a => RemoverAcentos(a.NomeArquivo).Contains(termoNormalizado, StringComparison.OrdinalIgnoreCase)
                || a.Tags.Any(t => RemoverAcentos(t).Contains(termoNormalizado, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>Pra buscar "nao" e encontrar "não" — sem isso, OrdinalIgnoreCase ignora maiúsculas/minúsculas mas trata acentos como caracteres diferentes.</summary>
    private static string RemoverAcentos(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var semAcentos = normalizado.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(semAcentos.ToArray()).Normalize(NormalizationForm.FormC);
    }
}
