using Jess.Entities.Enums;

namespace Jess.Application.DTOs;

public class PagamentoDto
{
    public int Id { get; set; }

    public int ProjetoId { get; set; }

    public decimal ValorPrevisto { get; set; }

    public DateOnly DataPrevista { get; set; }

    public decimal? ValorRecebido { get; set; }

    public DateOnly? DataRecebimento { get; set; }

    public string? Observacoes { get; set; }

    public StatusPagamento Status { get; set; }
}
