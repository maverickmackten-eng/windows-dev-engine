using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace AdornerOverlay;

// Highlights the bounding box + margin of any UIElement for layout debugging.
// Usage: AdornerLayer.GetAdornerLayer(element)?.Add(new BoundsAdorner(element));
public class BoundsAdorner : Adorner
{
    private static readonly Pen BoxPen = new(new SolidColorBrush(Color.FromArgb(200, 0, 180, 255)), 1.5);
    private static readonly Pen MarginPen = new(new SolidColorBrush(Color.FromArgb(120, 255, 107, 53)), 1)
    {
        DashStyle = new DashStyle(new double[] { 4, 3 }, 0)
    };
    private static readonly SolidColorBrush FillBrush = new(Color.FromArgb(15, 0, 180, 255));
    private static readonly SolidColorBrush InfoBg = new(Color.FromArgb(200, 10, 13, 18));

    public BoundsAdorner(UIElement adornedElement) : base(adornedElement)
    {
        IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext dc)
    {
        var element = AdornedElement as FrameworkElement;
        if (element == null) return;

        double w = element.ActualWidth;
        double h = element.ActualHeight;

        // Fill
        dc.DrawRectangle(FillBrush, BoxPen, new Rect(0, 0, w, h));

        // Margin box
        var margin = element.Margin;
        if (margin != new Thickness(0))
        {
            var marginRect = new Rect(-margin.Left, -margin.Top,
                w + margin.Left + margin.Right, h + margin.Top + margin.Bottom);
            dc.DrawRectangle(null, MarginPen, marginRect);
        }

        // Size label
        var text = new FormattedText(
            $"{w:F0} × {h:F0}",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Cascadia Code"),
            10, Brushes.White, 96);

        double tx = 4, ty = h - text.Height - 4;
        dc.DrawRectangle(InfoBg, null, new Rect(tx - 2, ty - 2, text.Width + 8, text.Height + 4));
        dc.DrawText(text, new Point(tx + 2, ty));
    }
}
