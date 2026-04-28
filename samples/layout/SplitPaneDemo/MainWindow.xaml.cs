using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Serilog;

namespace SplitPaneDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowHorizontalSplit();
            Log.Debug("[SplitPaneDemo] Loaded");
        }

        private void CloseClick(object s, RoutedEventArgs e)    => Close();
        private void MinimizeClick(object s, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void LayoutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            SetTabActive(btn);
            switch (btn.Tag?.ToString())
            {
                case "H": ShowHorizontalSplit(); break;
                case "V": ShowVerticalSplit();   break;
                case "T": ShowTripleSplit();     break;
            }
        }

        private void SetTabActive(Button active)
        {
            foreach (var b in new[] { BtnH, BtnV, BtnT })
            {
                b.Background  = Brush(0x1C, 0x23, 0x33);
                b.Foreground  = Brush(0x88, 0x99, 0xAA);
                b.BorderBrush = Brush(0x2A, 0x33, 0x47);
            }
            active.Background  = Brush(0x0D, 0x3A, 0x5C);
            active.Foreground  = Brush(0x00, 0xB4, 0xFF);
            active.BorderBrush = Brush(0x00, 0xB4, 0xFF);
        }

        // 1. Horizontal: left list | right detail
        private void ShowHorizontalSplit()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300), MinWidth = 140 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 200 });

            var left     = MakePane("Left — Item List",   MakeListBox(new[] { "Alpha Node", "Bravo Link", "Charlie Stream", "Delta Probe", "Echo Channel", "Foxtrot Hub" }));
            var splitter = MakeVSplitter(grid, 0, 2);       Grid.SetColumn(splitter, 1);
            var right    = MakePane("Right — Detail",     MakeDetail()); Grid.SetColumn(right, 2);

            grid.Children.Add(left);
            grid.Children.Add(splitter);
            grid.Children.Add(right);
            SplitHost.Content = grid;
            Log.Debug("[SplitPane] Horizontal");
        }

        // 2. Vertical: top editor | bottom log
        private void ShowVerticalSplit()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 100 });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180), MinHeight = 60 });

            var top      = MakePane("Top — Editor",       MakeEditor());
            var splitter = MakeHSplitter(grid, 0, 2);       Grid.SetRow(splitter, 1);
            var bottom   = MakePane("Bottom — Output",    MakeLog()); Grid.SetRow(bottom, 2);

            grid.Children.Add(top);
            grid.Children.Add(splitter);
            grid.Children.Add(bottom);
            SplitHost.Content = grid;
            Log.Debug("[SplitPane] Vertical");
        }

        // 3. Triple: tree | main | props
        private void ShowTripleSplit()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220), MinWidth = 100 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 180 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240), MinWidth = 120 });

            var left    = MakePane("Left — Tree",        MakeListBox(new[] { "Root", "  ▸ Node A", "  ▸ Node B", "      Leaf 1", "      Leaf 2", "  ▸ Node C" }));
            var spl1    = MakeVSplitter(grid, 0, 2);     Grid.SetColumn(spl1, 1);
            var center  = MakePane("Center — Main",     MakeEditor()); Grid.SetColumn(center, 2);
            var spl2    = MakeVSplitter(grid, 2, 4);     Grid.SetColumn(spl2, 3);
            var right   = MakePane("Right — Properties",MakeProps()); Grid.SetColumn(right, 4);

            grid.Children.Add(left);
            grid.Children.Add(spl1);
            grid.Children.Add(center);
            grid.Children.Add(spl2);
            grid.Children.Add(right);
            SplitHost.Content = grid;
            Log.Debug("[SplitPane] Triple");
        }

        // ── Pane wrapper ─────────────────────────────────────────
        private static Border MakePane(string title, UIElement content)
        {
            var g = new Grid();
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var hdr = new Border
            {
                Background = Brush(0x0E, 0x13, 0x18), Padding = new Thickness(12, 7),
                BorderBrush = Brush(0x1E, 0x2D, 0x3D), BorderThickness = new Thickness(0, 0, 0, 1),
                Child = new TextBlock { Text = title, FontSize = 10, FontWeight = FontWeights.Bold,
                    Foreground = Brush(0x88, 0x99, 0xAA) }
            };
            Grid.SetRow(hdr, 0);

            var body = new Border { Background = Brush(0x0A, 0x0D, 0x12), Child = content };
            Grid.SetRow(body, 1);

            g.Children.Add(hdr);
            g.Children.Add(body);
            return new Border { BorderBrush = Brush(0x1E, 0x2D, 0x3D), BorderThickness = new Thickness(1), Child = g };
        }

        // ── Splitter factories ─────────────────────────────────
        private GridSplitter MakeVSplitter(Grid owner, int colA, int colB)
        {
            var s = new GridSplitter
            {
                Width = 5, Background = Brush(0x1E, 0x2D, 0x3D),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Cursor = Cursors.SizeWE, ResizeBehavior = GridResizeBehavior.PreviousAndNext
            };
            s.MouseEnter      += (_, _) => s.Background = Brush(0x00, 0xB4, 0xFF);
            s.MouseLeave      += (_, _) => s.Background = Brush(0x1E, 0x2D, 0x3D);
            s.MouseDoubleClick += (_, _) =>
            {
                double tot = owner.ColumnDefinitions[colA].ActualWidth
                           + owner.ColumnDefinitions[colB].ActualWidth;
                owner.ColumnDefinitions[colA].Width = new GridLength(tot / 2, GridUnitType.Pixel);
                owner.ColumnDefinitions[colB].Width = new GridLength(tot / 2, GridUnitType.Pixel);
            };
            return s;
        }

        private GridSplitter MakeHSplitter(Grid owner, int rowA, int rowB)
        {
            var s = new GridSplitter
            {
                Height = 5, Background = Brush(0x1E, 0x2D, 0x3D),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Cursor = Cursors.SizeNS, ResizeBehavior = GridResizeBehavior.PreviousAndNext
            };
            s.MouseEnter      += (_, _) => s.Background = Brush(0x00, 0xB4, 0xFF);
            s.MouseLeave      += (_, _) => s.Background = Brush(0x1E, 0x2D, 0x3D);
            s.MouseDoubleClick += (_, _) =>
            {
                double tot = owner.RowDefinitions[rowA].ActualHeight
                           + owner.RowDefinitions[rowB].ActualHeight;
                owner.RowDefinitions[rowA].Height = new GridLength(tot / 2, GridUnitType.Pixel);
                owner.RowDefinitions[rowB].Height = new GridLength(tot / 2, GridUnitType.Pixel);
            };
            return s;
        }

        // ── Content placeholders ────────────────────────────────
        private static ListBox MakeListBox(string[] items)
        {
            var lb = new ListBox { Background = Brushes.Transparent, BorderThickness = new Thickness(0), FontSize = 13 };
            foreach (var item in items)
                lb.Items.Add(new ListBoxItem { Content = item, Padding = new Thickness(14, 6), Foreground = Brush(0x88, 0x99, 0xAA) });
            return lb;
        }

        private static ScrollViewer MakeEditor()
            => new() { Content = new TextBox {
                Text = "// Editor area\nusing System;\n\nnamespace Demo\n{\n    class Program { static void Main() => Console.WriteLine(\"Hello!\"); }\n}",
                Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                Foreground = Brush(0xCC, 0xDD, 0xEE), FontFamily = new FontFamily("Cascadia Code, Consolas"),
                FontSize = 13, AcceptsReturn = true, AcceptsTab = true, Padding = new Thickness(14),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto } };

        private static ScrollViewer MakeLog()
        {
            var sp = new StackPanel { Margin = new Thickness(14, 8) };
            foreach (var (ts, lvl, msg, hex) in new[]
            {
                ("14:23:01", "[INF]", "Build started",           "#8899AA"),
                ("14:23:02", "[OK ]", "Compiled successfully",   "#00C47A"),
                ("14:23:02", "[WRN]", "Nullable: 3 warnings",    "#FFD700"),
                ("14:23:03", "[OK ]", "47/47 tests passed",      "#00C47A"),
                ("14:23:04", "[ERR]", "Deploy skipped: no cert", "#FF2D55"),
            })
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2) };
                row.Children.Add(Mono(ts,  "#445566")); row.Children.Add(new Border { Width = 10 });
                row.Children.Add(Mono(lvl, hex));       row.Children.Add(new Border { Width = 10 });
                row.Children.Add(Mono(msg, hex));
                sp.Children.Add(row);
            }
            return new ScrollViewer { Content = sp, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        }

        private static StackPanel MakeDetail()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(new TextBlock { Text = "Alpha Node", FontFamily = new FontFamily("Segoe UI Black"), FontSize = 20, Foreground = Brushes.White, Margin = new Thickness(0, 0, 0, 14) });
            foreach (var (k, v) in new[] { ("ID","#A-2047"),("Category","Operations"),("Status","Active"),("Value","48,291"),("Updated","2026-04-27") })
            {
                var g = new Grid { Margin = new Thickness(0, 0, 0, 8) };
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.Children.Add(new TextBlock { Text = k, FontSize = 12, Foreground = Brush(0x88, 0x99, 0xAA) });
                var vb = new TextBlock { Text = v, FontSize = 12, Foreground = Brushes.White };
                Grid.SetColumn(vb, 1); g.Children.Add(vb);
                sp.Children.Add(g);
            }
            return sp;
        }

        private static StackPanel MakeProps()
        {
            var sp = new StackPanel { Margin = new Thickness(14) };
            sp.Children.Add(new TextBlock { Text = "PROPERTIES", FontSize = 9, FontWeight = FontWeights.Bold, Foreground = Brush(0x00, 0xB4, 0xFF), Margin = new Thickness(0, 0, 0, 10) });
            foreach (var (k, v) in new[] { ("Width","480px"),("Height","auto"),("Padding","16"),("Background","#0A0D12"),("Border","1px #1E2D3D"),("CornerRadius","8") })
            {
                var g = new Grid { Margin = new Thickness(0, 0, 0, 6) };
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.Children.Add(Mono(k, "#8899AA"));
                var vb = Mono(v, "#FFFFFF"); vb.HorizontalAlignment = HorizontalAlignment.Right;
                Grid.SetColumn(vb, 1); g.Children.Add(vb);
                sp.Children.Add(g);
            }
            return sp;
        }

        // ── Micro helpers ───────────────────────────────────────────
        private static SolidColorBrush Brush(byte r, byte g, byte b)
            => new(Color.FromRgb(r, g, b));

        private static TextBlock Mono(string text, string hex)
            => new() { Text = text, FontFamily = new FontFamily("Cascadia Code, Consolas"),
                       FontSize = 11, Foreground = new SolidColorBrush(
                           (Color)ColorConverter.ConvertFromString(hex)) };
    }
}
