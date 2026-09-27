using Jess.Entities.Enums;

namespace Jess.Application.DTOs;

public class RegistroHorasDto
{
    public int Id { get; set; }

    public int ProjetoId { get; set; }

    public DateTime InicioSessao { get; set; }

    public DateTime? FimSessao { get; set; }

    public int DuracaoMinutos { get; set; }

    public OrigemRegistroHoras Origem { get; set; }

    public StatusRegistroHoras Status { get; set; }

    public CategoriaHoras? Categoria { get; set; }

    public string? Nota { get; set; }

    /// <summary>Início da pausa em aberto, se `Status` for `Pausado`. Usado para congelar o relógio ao vivo.</summary>
    public DateTime? PausaAbertaEm { get; set; }

    /// <summary>Soma exata das pausas já encerradas. Usado para descontar do relógio ao vivo sem o arredondamento de minuto usado em `DuracaoMinutos`.</summary>
    public TimeSpan TempoPausadoFechado { get; set; }

    public List<SegmentoHorasDto> Segmentos { get; set; } = [];
}
