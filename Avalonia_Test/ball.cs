using Avalonia.Media;
namespace Avalonia_Test;
public class Ball
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Vx { get; set; }
    public double Vy { get; set; }
    public double Radius { get; set; } = 13;
    public IBrush Color { get; set; }
    public Ball(double x, double y, IBrush color)
    {
        X = x;
        Y = y;
        Color = color;
    }
}