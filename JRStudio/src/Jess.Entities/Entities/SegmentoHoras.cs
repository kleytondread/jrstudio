using Jess.Entities.Enums;

namespace Jess.Entities.Entities;

public class SegmentoHoras
{
    public int Id { get; set; }

    public int RegistroHorasId { get; set; }

    public RegistroDeHoras? RegistroHoras { get; set; }

    public int DuracaoMinutos { get; set; }

    public CategoriaHoras Categoria { get; set; }

    public string? Nota { get; set; }

    public int Ordem { get; set; }
}
