using Jess.Entities.Enums;

namespace Jess.UI.Formatting;

public static class StatusLabels
{
    public static string Texto(StatusProjeto status) => status switch
    {
        StatusProjeto.Orcamento => "Orçamento",
        StatusProjeto.EmAndamento => "Em andamento",
        StatusProjeto.Pausado => "Pausado",
        StatusProjeto.Concluido => "Concluído",
        _ => status.ToString()
    };
}
