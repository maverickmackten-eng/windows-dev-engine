using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Serilog;

namespace DataGridDemo
{
    // Data model
    public class GridRow : System.ComponentModel.INotifyPropertyChanged
    {
        private string _status = "Active";
        public int      Id       { get; set; }
        public string   Name     { get; set; } = "";
        public string   Category { get; set; } = "";
        public string   Status   { get => _status; set { _status = value; OnPropertyChanged(nameof(Status)); } }
        public long     Value    { get; set; }
        public DateTime Updated  { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new(p));
    }

    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<GridRow> _rows = new();
        private ICollectionView? _view;
        private string _filter = "";
        private int _nextId = 1;
        private static readonly Random _rng = new();
        private static readonly string[] _cats = { "Ops", "Security", "Analytics", "Reports", "Network" };
        private static readonly string[] _statuses = { "Active", "Active", "Active", "Warning", "Error" };
        private static readonly string[] _names =
        {
            "Alpha Node", "Bravo Link", "Charlie Stream", "Delta Probe",
            "Echo Channel", "Foxtrot Hub", "Golf Route", "Hotel Gateway",
            "India Bridge", "Juliet Agent", "Kilo Worker", "Lima Task",
            "Mike Service", "November Job", "Oscar Relay", "Papa Sensor"
        };

        public MainWindow()
        {
            InitializeComponent();
            SeedRows(20);
            _view = CollectionViewSource.GetDefaultView(_rows);
            _view.Filter = FilterRow;
            MainGrid.ItemsSource = _view;
            UpdateStatusBar();
            Log.Debug("[DataGridDemo] Loaded with {Count} rows", _rows.Count);
        }

        private void CloseClick(object s, RoutedEventArgs e) => Close();
        private void MinimizeClick(object s, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void SeedRows(int count)
        {
            for (int i = 0; i < count; i++)
                AddRow();
        }

        private void AddRow()
        {
            _rows.Add(new GridRow
            {
                Id       = _nextId++,
                Name     = _names[_rng.Next(_names.Length)] + $" {_nextId - 1}",
                Category = _cats[_rng.Next(_cats.Length)],
                Status   = _statuses[_rng.Next(_statuses.Length)],
                Value    = _rng.Next(100, 99999),
                Updated  = DateTime.Now.AddDays(-_rng.Next(0, 365)),
            });
        }

        private bool FilterRow(object obj)
        {
            if (string.IsNullOrEmpty(_filter)) return true;
            if (obj is not GridRow r) return false;
            return r.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase)
                || r.Category.Contains(_filter, StringComparison.OrdinalIgnoreCase)
                || r.Status.Contains(_filter, StringComparison.OrdinalIgnoreCase)
                || r.Id.ToString().Contains(_filter);
        }

        private void SearchBox_TextChanged(object s, System.Windows.Controls.TextChangedEventArgs e)
        {
            _filter = SearchBox.Text;
            _view?.Refresh();
            UpdateStatusBar();
        }

        private void Grid_SelectionChanged(object s, System.Windows.Controls.SelectionChangedEventArgs e)
            => UpdateStatusBar();

        private void DeleteSelected_Click(object s, RoutedEventArgs e)
        {
            var selected = MainGrid.SelectedItems.Cast<GridRow>().ToList();
            foreach (var row in selected)
                _rows.Remove(row);
            UpdateStatusBar();
            Log.Debug("[DataGrid] Deleted {Count} rows", selected.Count);
        }

        private void AddRow_Click(object s, RoutedEventArgs e)
        {
            AddRow();
            UpdateStatusBar();
            // Scroll to new row
            MainGrid.ScrollIntoView(_rows[^1]);
        }

        private void UpdateStatusBar()
        {
            int visible  = _view?.Cast<object>().Count() ?? _rows.Count;
            int selected = MainGrid.SelectedItems.Count;
            RowCountLabel.Text = $"Rows: {visible} / {_rows.Count} total";
            SelCountLabel.Text = $"Selected: {selected}";
        }
    }
}
