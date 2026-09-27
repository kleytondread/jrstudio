using Jess.Entities.Enums;

namespace Jess.Application.DTOs;

public class SegmentoInput
{
    public int DuracaoMinutos { get; set; }

    public CategoriaHoras Categoria { get; set; }

    public string? Nota { get; set; }
}
