namespace Jess.Entities.Entities;

public class Cliente
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Telefone { get; set; }

    public string? Email { get; set; }

    public string? Observacoes { get; set; }

    public DateTime DataCadastro { get; set; }
}
