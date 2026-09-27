using System.ComponentModel.DataAnnotations;
using Jess.Entities.Enums;

namespace Jess.Application.DTOs;

public class SalvarProjetoRequest
{
    [Required(ErrorMessage = "Digite um nome para o projeto.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o cliente do projeto.")]
    public string ClienteNome { get; set; } = string.Empty;

    public string? Endereco { get; set; }

    [Required(ErrorMessage = "Informe a data de início.")]
    public DateOnly? DataInicio { get; set; }

    public DateOnly? PrazoEstimado { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "O valor não pode ser negativo.")]
    public decimal ValorTotalAcordado { get; set; }

    public string? Observacoes { get; set; }

    public StatusProjeto Status { get; set; } = StatusProjeto.Orcamento;
}
