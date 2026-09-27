using Jess.Entities.Enums;

namespace Jess.UI.Formatting;

public static class StatusPagamentoLabels
{
    public static string Texto(StatusPagamento status) => status switch
    {
        StatusPagamento.Pendente => "pendente",
        StatusPagamento.Recebido => "recebido",
        StatusPagamento.Atrasado => "atrasado",
        _ => status.ToString()
    };
}
