namespace Jess.Entities.Entities;

public class Pagamento
{
    public int Id { get; set; }

    public int ProjetoId { get; set; }

    public Projeto? Projeto { get; set; }

    public decimal ValorPrevisto { get; set; }

    public DateOnly DataPrevista { get; set; }

    public decimal? ValorRecebido { get; set; }

    public DateOnly? DataRecebimento { get; set; }

    public string? Observacoes { get; set; }
}
