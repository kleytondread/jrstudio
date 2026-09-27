using Jess.Entities.Enums;

namespace Jess.Application.DTOs;

public class TarefaDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public int? ProjetoId { get; set; }

    public FaseProjeto? Fase { get; set; }

    public PrioridadeTarefa Prioridade { get; set; }

    public DateTime? Prazo { get; set; }

    public ColunaTarefa Coluna { get; set; }

    public string? RegraRecorrencia { get; set; }

    public DateTime DataCriacao { get; set; }

    public DateTime? DataConclusao { get; set; }
}
