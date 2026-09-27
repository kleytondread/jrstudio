using Jess.Entities.Enums;

namespace Jess.Application.DTOs;

public class ProjetoDto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string ClienteNome { get; set; } = string.Empty;

    public string? Endereco { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly? PrazoEstimado { get; set; }

    public StatusProjeto Status { get; set; }

    public decimal ValorTotalAcordado { get; set; }

    public string? Observacoes { get; set; }

    public DateTime DataCriacao { get; set; }

    public DateTime? DataConclusao { get; set; }
}
