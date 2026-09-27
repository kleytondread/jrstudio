using Jess.Entities.Enums;

namespace Jess.UI.Formatting;

public static class CategoriaHorasLabels
{
    public static readonly CategoriaHoras[] Todas =
    [
        CategoriaHoras.Detalhamento,
        CategoriaHoras.Projeto,
        CategoriaHoras.ReuniaoCliente,
        CategoriaHoras.VisitaObra,
        CategoriaHoras.Administrativo,
        CategoriaHoras.Outro
    ];

    public static string Texto(CategoriaHoras categoria) => categoria switch
    {
        CategoriaHoras.Detalhamento => "Detalhamento",
        CategoriaHoras.Projeto => "Projeto",
        CategoriaHoras.ReuniaoCliente => "Reunião com cliente",
        CategoriaHoras.VisitaObra => "Visita a obra",
        CategoriaHoras.Administrativo => "Administrativo",
        CategoriaHoras.Outro => "Outro",
        _ => categoria.ToString()
    };
}
