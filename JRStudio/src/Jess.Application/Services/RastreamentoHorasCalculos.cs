namespace Jess.Application.Services;

/// <summary>
/// Regras de cálculo do módulo de Rastreamento de Horas (Seção 6.3 da especificação). Funções
/// puras — sem repositório nem estado — para serem testáveis diretamente, sem mocks.
/// </summary>
public static class RastreamentoHorasCalculos
{
    public static int CalcularDuracaoMinutos(DateTime inicio, DateTime fim, IEnumerable<(DateTime InicioPausa, DateTime? FimPausa)> pausas)
    {
        var totalMinutos = (int)Math.Round((fim - inicio).TotalMinutes);
        var minutosPausados = pausas.Sum(p => (int)Math.Round(((p.FimPausa ?? fim) - p.InicioPausa).TotalMinutes));

        return Math.Max(0, totalMinutos - minutosPausados);
    }

    public static decimal? CalcularValorPorHora(decimal valorTotalAcordado, int totalMinutosTrabalhados)
    {
        if (totalMinutosTrabalhados <= 0)
        {
            return null;
        }

        var horas = totalMinutosTrabalhados / 60m;
        return valorTotalAcordado / horas;
    }
}
