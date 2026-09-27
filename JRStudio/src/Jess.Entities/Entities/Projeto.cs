using Jess.Entities.Enums;

namespace Jess.Entities.Entities;

public class Projeto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    public string? Endereco { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly? PrazoEstimado { get; set; }

    public StatusProjeto Status { get; set; } = StatusProjeto.Orcamento;

    public decimal ValorTotalAcordado { get; set; }

    public string? Observacoes { get; set; }

    public DateTime DataCriacao { get; set; }

    public DateTime? DataConclusao { get; set; }
}
