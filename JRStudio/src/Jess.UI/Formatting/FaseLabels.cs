using Jess.Entities.Enums;

namespace Jess.UI.Formatting;

public static class FaseLabels
{
    public static readonly FaseProjeto[] OrdemPadrao =
    [
        FaseProjeto.PreProjeto,
        FaseProjeto.EstudoPreliminar,
        FaseProjeto.Anteprojeto,
        FaseProjeto.ProjetoExecutivo
    ];

    public static string Texto(FaseProjeto fase) => fase switch
    {
        FaseProjeto.PreProjeto => "Pré-projeto",
        FaseProjeto.EstudoPreliminar => "Estudo preliminar",
        FaseProjeto.Anteprojeto => "Anteprojeto",
        FaseProjeto.ProjetoExecutivo => "Projeto executivo",
        _ => fase.ToString()
    };
}
