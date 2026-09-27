namespace Jess.Entities.Entities;

public class Tag
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public List<Arquivo> Arquivos { get; set; } = [];
}
