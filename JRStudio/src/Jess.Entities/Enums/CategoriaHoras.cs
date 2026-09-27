namespace Jess.Entities.Enums;

public enum CategoriaHoras
{
    Detalhamento,
    ReuniaoCliente,
    VisitaObra,
    Administrativo,
    Outro,
    // Adicionado depois de já haver registros em produção com Outro = 4 (EF Core grava o enum como
    // int por padrão) — precisa ficar no fim para não renumerar valores já persistidos no SQLite.
    Projeto
}
