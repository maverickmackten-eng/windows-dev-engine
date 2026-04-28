using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Serilog;

namespace NavigationShellDemo
{
    public partial class MainWindow : Window
    {
        private bool _collapsed = false;
        private Button? _activeNav;
        private readonly Dictionary<string, UIElement> _pageCache = new();

        // Nav button -> label TextBlock map (for hiding text when collapsed)
        private Dictionary<Button, TextBlock>? _navLabels;

        public MainWindow()
        {
            InitializeComponent();
            _navLabels = new()
            {
                { NavDashboard, LblDashboard },
                { NavOps,       LblOps },
                { NavAnalytics, LblAnalytics },
                { NavSecurity,  LblSecurity },
                { NavSettings,  LblSettings },
            };
            NavigateTo(NavDashboard, "Dashboard");
            Log.Debug("[NavShell] Initialized");
        }

        private void CloseClick(object s, RoutedEventArgs e) => Close();
        private void MinimizeClick(object s, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void NavItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                NavigateTo(btn, btn.Tag?.ToString() ?? "");
        }

        private void NavigateTo(Button navBtn, string pageName)
        {
            if (_activeNav == navBtn) return;

            // Deactivate old item
            if (_activeNav != null)
                SetNavActive(_activeNav, false);

            _activeNav = navBtn;
            SetNavActive(navBtn, true);
            PageTitleBar.Text = pageName;

            // Fade out current page
            var fadeOut = new DoubleAnimation(1, 0, System.TimeSpan.FromMilliseconds(100));
            fadeOut.Completed += (_, _) =>
            {
                ContentHost.Content = GetOrCreatePage(pageName);
                ((Storyboard)FindResource("PageFadeIn")).Begin();
            };
            ContentHost.BeginAnimation(OpacityProperty, fadeOut);
            Log.Debug("[NavShell] Navigate -> {Page}", pageName);
        }

        private void SetNavActive(Button btn, bool active)
        {
            // Walk the visual tree to find the ActiveBar Border inside the NavItem template
            btn.ApplyTemplate();
            if (btn.Template.FindName("ActiveBar", btn) is Border bar)
                bar.Opacity = active ? 1 : 0;
            btn.Foreground = active
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0xB4, 0xFF))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x88, 0x99, 0xAA));
            if (active && btn.Template.FindName("Bg", btn) is Border bg)
                bg.Background = new SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(0x18, 0x00, 0xB4, 0xFF));
        }

        private UIElement GetOrCreatePage(string name)
        {
            if (_pageCache.TryGetValue(name, out var page)) return page;
            page = BuildPlaceholderPage(name);
            _pageCache[name] = page;
            return page;
        }

        // Lightweight placeholder page — replace with real UserControls in production
        private static UIElement BuildPlaceholderPage(string name)
        {
            return new Border
            {
                Padding = new Thickness(40),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text       = name,
                            FontFamily = new FontFamily("Segoe UI Black"),
                            FontSize   = 32,
                            Foreground = new SolidColorBrush(Colors.White),
                        },
                        new TextBlock
                        {
                            Text       = $"This is the {name} page. Replace with a real UserControl.",
                            FontSize   = 14,
                            Foreground = new SolidColorBrush(
                                System.Windows.Media.Color.FromRgb(0x88, 0x99, 0xAA)),
                            Margin = new Thickness(0, 10, 0, 0),
                        }
                    }
                }
            };
        }

        // Sidebar collapse / expand toggle
        private void CollapseBtn_Click(object sender, RoutedEventArgs e)
        {
            _collapsed = !_collapsed;
            var sb = (Storyboard)FindResource(_collapsed ? "SideCollapse" : "SideExpand");
            sb.Begin();

            // Hide/show nav labels
            if (_navLabels != null)
                foreach (var lbl in _navLabels.Values)
                    lbl.Visibility = _collapsed ? Visibility.Collapsed : Visibility.Visible;

            // Flip collapse icon arrow direction
            CollapseIcon.Text = _collapsed ? "\uE760" : "\uE761";
            Log.Debug("[NavShell] Sidebar {State}", _collapsed ? "collapsed" : "expanded");
        }
    }
}
