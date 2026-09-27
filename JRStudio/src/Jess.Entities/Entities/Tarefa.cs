using Jess.Entities.Enums;

namespace Jess.Entities.Entities;

public class Tarefa
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public int? ProjetoId { get; set; }

    public Projeto? Projeto { get; set; }

    public FaseProjeto? Fase { get; set; }

    public PrioridadeTarefa Prioridade { get; set; } = PrioridadeTarefa.Media;

    public DateTime? Prazo { get; set; }

    public ColunaTarefa Coluna { get; set; } = ColunaTarefa.AFazer;

    public string? RegraRecorrencia { get; set; }

    public DateTime DataCriacao { get; set; }

    public DateTime? DataConclusao { get; set; }

    public List<Subtarefa> Subtarefas { get; set; } = [];
}
