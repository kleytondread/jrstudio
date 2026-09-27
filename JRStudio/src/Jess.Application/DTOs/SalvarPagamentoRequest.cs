using System.ComponentModel.DataAnnotations;

namespace Jess.Application.DTOs;

public class SalvarPagamentoRequest
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Informe um valor previsto maior que zero.")]
    public decimal ValorPrevisto { get; set; }

    [Required(ErrorMessage = "Informe a data prevista.")]
    public DateOnly? DataPrevista { get; set; }

    public string? Observacoes { get; set; }
}
