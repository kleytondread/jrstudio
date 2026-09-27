using System.ComponentModel.DataAnnotations;
using Jess.Entities.Enums;

namespace Jess.Application.DTOs;

public class CriarTarefaRapidaRequest
{
    public int ProjetoId { get; set; }

    public FaseProjeto Fase { get; set; }

    public ColunaTarefa Coluna { get; set; }

    [Required(ErrorMessage = "Digite um título para a tarefa.")]
    public string Titulo { get; set; } = string.Empty;
}
