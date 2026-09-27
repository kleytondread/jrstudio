using System.ComponentModel.DataAnnotations;
using Jess.Entities.Enums;

namespace Jess.Application.DTOs;

public class SalvarTarefaRequest
{
    [Required(ErrorMessage = "Digite um título para a tarefa.")]
    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public PrioridadeTarefa Prioridade { get; set; } = PrioridadeTarefa.Media;

    public DateTime? Prazo { get; set; }

    public ColunaTarefa Coluna { get; set; } = ColunaTarefa.AFazer;

    public string? RegraRecorrencia { get; set; }
}
