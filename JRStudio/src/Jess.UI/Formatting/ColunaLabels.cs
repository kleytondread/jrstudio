using Jess.Entities.Enums;

namespace Jess.UI.Formatting;

public static class ColunaLabels
{
    public static readonly ColunaTarefa[] OrdemPadrao =
    [
        ColunaTarefa.AFazer,
        ColunaTarefa.Fazendo,
        ColunaTarefa.Concluido
    ];

    public static string Texto(ColunaTarefa coluna) => coluna switch
    {
        ColunaTarefa.AFazer => "A fazer",
        ColunaTarefa.Fazendo => "Fazendo",
        ColunaTarefa.Concluido => "Concluído",
        _ => coluna.ToString()
    };
}
