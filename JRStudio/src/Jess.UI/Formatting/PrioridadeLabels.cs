using Jess.Entities.Enums;

namespace Jess.UI.Formatting;

public static class PrioridadeLabels
{
    public static string Texto(PrioridadeTarefa prioridade) => prioridade switch
    {
        PrioridadeTarefa.Baixa => "baixa",
        PrioridadeTarefa.Media => "média",
        PrioridadeTarefa.Alta => "alta",
        _ => prioridade.ToString()
    };

    public static string CorVariavel(PrioridadeTarefa prioridade) => prioridade switch
    {
        PrioridadeTarefa.Alta => "var(--danger)",
        PrioridadeTarefa.Media => "var(--warning)",
        _ => "var(--text-muted)"
    };
}
