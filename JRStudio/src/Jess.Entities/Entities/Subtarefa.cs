namespace Jess.Entities.Entities;

public class Subtarefa
{
    public int Id { get; set; }

    public int TarefaId { get; set; }

    public Tarefa? Tarefa { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public bool Concluida { get; set; }

    public int Ordem { get; set; }
}
