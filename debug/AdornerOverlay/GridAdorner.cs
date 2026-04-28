using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace AdornerOverlay;

// Draws a debug grid over any UIElement via the WPF AdornerLayer.
// Usage: AdornerLayer.GetAdornerLayer(element)?.Add(new GridAdorner(element));
public class GridAdorner : Adorner
{
    private bool _isVisible = true;
    private readonly Pen _gridPen = new(new SolidColorBrush(Color.FromArgb(60, 0, 180, 255)), 0.5);
    private readonly Pen _majorPen = new(new SolidColorBrush(Color.FromArgb(100, 0, 220, 255)), 1);

    public GridAdorner(UIElement adornedElement) : base(adornedElement)
    {
        IsHitTestVisible = false;
    }

    public void Toggle() { _isVisible = !_isVisible; InvalidateVisual(); }

    protected override void OnRender(DrawingContext dc)
    {
        if (!_isVisible) return;

        var element = AdornedElement as FrameworkElement;
        if (element == null) return;

        double w = element.ActualWidth;
        double h = element.ActualHeight;
        const double gridSize = 8;
        const double majorEvery = 5;

        for (double x = 0; x <= w; x += gridSize)
        {
            var pen = (Math.Round(x / gridSize) % majorEvery == 0) ? _majorPen : _gridPen;
            dc.DrawLine(pen, new Point(x, 0), new Point(x, h));
        }
        for (double y = 0; y <= h; y += gridSize)
        {
            var pen = (Math.Round(y / gridSize) % majorEvery == 0) ? _majorPen : _gridPen;
            dc.DrawLine(pen, new Point(0, y), new Point(w, y));
        }
    }
}
