using Jess.Entities.Enums;

namespace Jess.Application.DTOs;

public class SegmentoHorasDto
{
    public int DuracaoMinutos { get; set; }

    public CategoriaHoras Categoria { get; set; }

    public string? Nota { get; set; }

    public int Ordem { get; set; }
}
