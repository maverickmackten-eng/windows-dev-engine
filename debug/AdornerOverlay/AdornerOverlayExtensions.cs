using System.Windows;
using System.Windows.Documents;

namespace AdornerOverlay;

// Extension methods — wire these into your MainWindow keyboard shortcut handler.
// Ctrl+Shift+G: toggle grid, Ctrl+Shift+B: toggle bounds on hovered element
public static class AdornerOverlayExtensions
{
    private static GridAdorner? _grid;
    private static readonly List<BoundsAdorner> _bounds = new();

    public static void ToggleGrid(UIElement root)
    {
        var layer = AdornerLayer.GetAdornerLayer(root);
        if (layer == null) return;

        if (_grid == null)
        {
            _grid = new GridAdorner(root);
            layer.Add(_grid);
        }
        else
        {
            _grid.Toggle();
        }
    }

    public static void AddBounds(UIElement element)
    {
        var layer = AdornerLayer.GetAdornerLayer(element);
        if (layer == null) return;
        var adorner = new BoundsAdorner(element);
        layer.Add(adorner);
        _bounds.Add(adorner);
    }

    public static void ClearAllBounds(UIElement root)
    {
        var layer = AdornerLayer.GetAdornerLayer(root);
        if (layer == null) return;
        foreach (var b in _bounds) layer.Remove(b);
        _bounds.Clear();
    }
}
