using Jess.Entities.Enums;

namespace Jess.Entities.Entities;

public class RegistroDeHoras
{
    public int Id { get; set; }

    public int ProjetoId { get; set; }

    public Projeto? Projeto { get; set; }

    public DateTime InicioSessao { get; set; }

    public DateTime? FimSessao { get; set; }

    public int DuracaoMinutos { get; set; }

    public OrigemRegistroHoras Origem { get; set; }

    public StatusRegistroHoras Status { get; set; }

    public CategoriaHoras? Categoria { get; set; }

    public string? Nota { get; set; }

    public List<SegmentoHoras> Segmentos { get; set; } = [];

    public List<PausaRegistroHoras> Pausas { get; set; } = [];
}
