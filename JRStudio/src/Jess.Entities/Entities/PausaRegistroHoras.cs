namespace Jess.Entities.Entities;

public class PausaRegistroHoras
{
    public int Id { get; set; }

    public int RegistroHorasId { get; set; }

    public RegistroDeHoras? RegistroHoras { get; set; }

    public DateTime InicioPausa { get; set; }

    public DateTime? FimPausa { get; set; }
}
