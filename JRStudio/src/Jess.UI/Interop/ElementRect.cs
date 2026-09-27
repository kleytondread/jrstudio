namespace Jess.UI.Interop;

/// <summary>Espelha o objeto retornado por <c>jessInterop.getBoundingRect</c> (wwwroot/js/interop.js).</summary>
public class ElementRect
{
    public double Top { get; set; }

    public double Left { get; set; }

    public double Right { get; set; }

    public double Bottom { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public double ViewportWidth { get; set; }

    public double ViewportHeight { get; set; }
}
