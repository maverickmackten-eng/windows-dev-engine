# Controls Reference

This document covers every reusable control and template in Windows Dev Engine.
Each entry shows: purpose, key XAML usage, data interface, and live sample location.

---

## Table of Contents

1. [Button Styles](#1-button-styles)
2. [TextBox](#2-textbox)
3. [ComboBox / DropdownMenu](#3-combobox--dropdownmenu)
4. [Toast Notification](#4-toast-notification)
5. [Popup Modal / Dialog](#5-popup-modal--dialog)
6. [DataGrid](#6-datagrid)
7. [Navigation Shell](#7-navigation-shell)
8. [Split Pane](#8-split-pane)
9. [Progress Bar](#9-progress-bar)
10. [Status Badge](#10-status-badge)
11. [Log / Output Panel](#11-log--output-panel)

---

## 1. Button Styles

**Live sample:** `samples/controls/DropdownMenuDemo`

| Style Key | Appearance | Use for |
|---|---|---|
| `BtnPrimary` | Solid accent fill | Primary CTA, default action |
| `BtnSecondary` | Accent border + transparent bg | Secondary actions |
| `BtnDanger` | Red border + red text | Destructive actions (delete) |
| `BtnGhost` | Fully transparent, text only | Toolbar actions, icon buttons |
| `BtnNav` | Transparent, left-aligned, accent active bar | Sidebar navigation items |

**Usage:**
```xml
<!-- Primary -->
<Button Style="{DynamicResource BtnPrimary}" Content="Save" Click="Save_Click"/>

<!-- Danger -->
<Button Style="{DynamicResource BtnDanger}" Content="Delete" Click="Delete_Click"/>

<!-- Icon-only ghost -->
<Button Style="{DynamicResource BtnGhost}" ToolTip="Settings">
    <TextBlock Text="&#xE713;" FontFamily="Segoe MDL2 Assets" FontSize="16"/>
</Button>
```

**State behavior:**
- `IsEnabled=False` → foreground drops to `TextDisabled`, cursor `Arrow`
- `IsMouseOver` → background lightens by ~8% (defined in `ControlTemplate.Triggers`)
- `IsPressed` → uses `AccentDim` instead of `Accent`

---

## 2. TextBox

**Live sample:** `samples/controls/PopupModalDemo` (form modal)

Default theme style keys applied globally via implicit style in theme file:

```xml
<TextBox Background="{DynamicResource BgInput}"
         Foreground="{DynamicResource TextPrimary}"
         BorderBrush="{DynamicResource Border1}"
         BorderThickness="1"
         Padding="10,8"
         CaretBrush="{DynamicResource Accent}"
         SelectionBrush="{DynamicResource Accent}">
    <TextBox.Resources>
        <!-- Removes default WPF FocusVisual blue border -->
        <Style TargetType="Border"><Setter Property="CornerRadius" Value="6"/></Style>
    </TextBox.Resources>
</TextBox>
```

**Focus ring:** Add to `ControlTemplate` — when `IsFocused=True`, `BorderBrush` transitions to `Accent`.

**Validation error style:**
```xml
<Style.Triggers>
    <Trigger Property="Validation.HasError" Value="True">
        <Setter Property="BorderBrush" Value="{DynamicResource ColorError}"/>
        <Setter Property="ToolTip"
                Value="{Binding RelativeSource={RelativeSource Self},
                                Path=(Validation.Errors)[0].ErrorContent}"/>
    </Trigger>
</Style.Triggers>
```

---

## 3. ComboBox / DropdownMenu

**Live sample:** `samples/controls/DropdownMenuDemo`

Three dropdown patterns covered:

### Styled ComboBox
Animated chevron rotates 180° on open. Item list has dark bg + hover highlight.

```xml
<ComboBox x:Name="MyCombo" Style="{StaticResource DarkCombo}">
    <ComboBoxItem Content="Option A"/>
    <ComboBoxItem Content="Option B"/>
</ComboBox>
```

### Popup Context Menu
Icon + label + keyboard hint per item. Right-click or button-triggered.

```csharp
// Open at button position
var menu = new ContextMenu();
menu.Items.Add(new MenuItem { Header = "Edit", Icon = new TextBlock { Text = "\uE70F", FontFamily = new FontFamily("Segoe MDL2 Assets") } });
menu.Items.Add(new MenuItem { Header = "Delete" });
menu.PlacementTarget = btn;
menu.IsOpen = true;
```

### Cascading Submenu
```xml
<MenuItem Header="Export">
    <MenuItem Header="As CSV"/>
    <MenuItem Header="As JSON"/>
    <MenuItem Header="As Markdown"/>
</MenuItem>
```
Submenus spawn to the right. Style them by targeting `MenuItem` in your theme.

---

## 4. Toast Notification

**Live sample:** `samples/controls/ToastDemo`

Toasts stack bottom-right (or top-right), auto-dismiss after a configurable delay, and support manual close.

### ToastManager API

```csharp
// Anywhere in your app:
ToastManager.Show("File saved successfully", ToastType.Success);
ToastManager.Show("Disk almost full",        ToastType.Warning, durationMs: 8000);
ToastManager.Show("Connection failed",       ToastType.Error,   sticky: true);
ToastManager.Show("Scan complete: 47 files", ToastType.Info);
```

### ToastType Enum

| Value | Border color | Icon |
|---|---|---|
| `Info` | `#00B4FF` | `ℹ` |
| `Success` | `#00C47A` | `✔` |
| `Warning` | `#FFD700` | `⚠` |
| `Error` | `#FF2D55` | `✘` |

### Implementation Pattern

```csharp
public static class ToastManager
{
    private static ToastHost? _host;
    public static void Register(ToastHost host) => _host = host;

    public static void Show(string message, ToastType type = ToastType.Info,
        int durationMs = 4000, bool sticky = false)
        => _host?.Dispatcher.InvokeAsync(() =>
            _host.AddToast(message, type, durationMs, sticky));
}
```

Place `<controls:ToastHost x:Name="Toasts"/>` as the last child of your root `Grid`
and call `ToastManager.Register(Toasts)` in `MainWindow` constructor.

---

## 5. Popup Modal / Dialog

**Live sample:** `samples/controls/PopupModalDemo`

Three modal patterns:

### Confirm Dialog
```csharp
var result = await ConfirmDialog.ShowAsync(
    owner:   this,
    title:   "Delete item?",
    message: "This cannot be undone.",
    confirm: "Delete",
    cancel:  "Cancel",
    danger:  true);

if (result == true)
    DeleteItem();
```

**Animation:** scale from 0.85 → 1.0 + fade in over 160ms. Esc key cancels.

### Form Modal
```csharp
var data = await FormModal.ShowAsync(owner: this, title: "Add Item");
if (data != null)
    Items.Add(data);
```

Internally a `Window` with `WindowStyle=None`, overlay dim on the parent.

### Detail Drawer
Slides in from the right edge. Width = 380px by default.
```csharp
DetailDrawer.Open(owner: this, content: new ItemDetailView(item));
```

---

## 6. DataGrid

**Live sample:** `samples/layout/DataGridDemo`

**Style key:** `DarkGrid`

```xml
<DataGrid Style="{StaticResource DarkGrid}"
          ItemsSource="{Binding Items}"
          AutoGenerateColumns="False"
          SelectionMode="Extended">
    <DataGrid.Columns>
        <DataGridTextColumn     Header="ID"     Binding="{Binding Id}"     Width="70"/>
        <DataGridTextColumn     Header="NAME"   Binding="{Binding Name}"   Width="*"/>
        <DataGridTemplateColumn Header="STATUS" Width="120">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <!-- Status badge — see §10 -->
                    <local:StatusBadge Status="{Binding Status}"/>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

**Filtering with ICollectionView:**
```csharp
var view = CollectionViewSource.GetDefaultView(Items);
view.Filter = obj => obj is MyItem item &&
    item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
DataGrid.ItemsSource = view;

// On search text change:
view.Refresh();
```

**Sorting:** click column headers triggers `SortDescriptions` automatically when
`CanUserSortColumns=True` (default).

---

## 7. Navigation Shell

**Live sample:** `samples/layout/NavigationShellDemo`

```xml
<!-- In MainWindow.xaml -->
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200" x:Name="SidebarCol" MinWidth="52"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <local:NavigationSidebar Grid.Column="0"
                              ActivePage="{Binding CurrentPage}"
                              NavigateCommand="{Binding NavigateCommand}"/>

    <ContentControl Grid.Column="1"
                     Content="{Binding CurrentPageContent}"/>
</Grid>
```

**Page swap animation:** fade out (100ms) → swap content → fade+translate in (150ms).
Set `EnableAnimations=False` in `AppSettings` to disable for low-end hardware.

**Sidebar collapse:** width animates 200→52px, nav labels hidden, icons remain.
Double-click any nav item to toggle collapse.

---

## 8. Split Pane

**Live sample:** `samples/layout/SplitPaneDemo`

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="300" MinWidth="140"/>
        <ColumnDefinition Width="5"/>  <!-- splitter -->
        <ColumnDefinition Width="*"  MinWidth="200"/>
    </Grid.ColumnDefinitions>

    <local:LeftPane  Grid.Column="0"/>

    <GridSplitter Grid.Column="1"
                  Style="{StaticResource VSplitter}"/>

    <local:RightPane Grid.Column="2"/>
</Grid>
```

**Style keys:** `VSplitter` (vertical, `SizeWE` cursor), `HSplitter` (horizontal, `SizeNS` cursor).
**Double-click to reset:** attach a `MouseDoubleClick` handler that sets both `ColumnDefinition.Width`
to `new GridLength(total/2, GridUnitType.Pixel)`.

---

## 9. Progress Bar

Dark-styled `ProgressBar` with accent fill:

```xml
<ProgressBar x:Name="Pb" Height="4" Minimum="0" Maximum="100"
             Value="{Binding Progress}"
             Background="#1E2D3D"
             Foreground="{DynamicResource Accent}"
             BorderThickness="0"/>
```

**Indeterminate mode** (for operations without a known count):
```xml
<ProgressBar IsIndeterminate="True" .../>
```
Style the indeterminate animation in the `ControlTemplate` using a looping `DoubleAnimation`
on a `TranslateTransform`.

---

## 10. Status Badge

Colored pill: background + border + text all driven by `Status` string.

```xml
<Border CornerRadius="4" Padding="8,2" HorizontalAlignment="Left">
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="Background" Value="{DynamicResource ColorInfoBg}"/>
            <Style.Triggers>
                <DataTrigger Binding="{Binding Status}" Value="Active">
                    <Setter Property="Background"  Value="{DynamicResource ColorSuccessBg}"/>
                    <Setter Property="BorderBrush" Value="{DynamicResource ColorSuccess}"/>
                </DataTrigger>
                <DataTrigger Binding="{Binding Status}" Value="Warning">
                    <Setter Property="Background"  Value="{DynamicResource ColorWarningBg}"/>
                    <Setter Property="BorderBrush" Value="{DynamicResource ColorWarning}"/>
                </DataTrigger>
                <DataTrigger Binding="{Binding Status}" Value="Error">
                    <Setter Property="Background"  Value="{DynamicResource ColorErrorBg}"/>
                    <Setter Property="BorderBrush" Value="{DynamicResource ColorError}"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
    <TextBlock Text="{Binding Status}" FontSize="11" FontWeight="SemiBold"/>
</Border>
```

---

## 11. Log / Output Panel

**Used in:** `SplitPaneDemo` (bottom pane), `NavigationShellDemo` (Dashboard page)

Pattern: `ScrollViewer` wrapping a `StackPanel`, each log line is a horizontal `StackPanel`
with timestamp + level + message. Auto-scroll to bottom after append:

```csharp
private void AppendLog(string timestamp, string level, string message, string hexColor)
{
    var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,2) };
    row.Children.Add(Mono(timestamp, "#445566"));
    row.Children.Add(Spacer(10));
    row.Children.Add(Mono(level,     hexColor));
    row.Children.Add(Spacer(10));
    row.Children.Add(Mono(message,   hexColor));
    LogPanel.Children.Add(row);

    // Auto-scroll
    LogScroller.ScrollToBottom();

    // Cap at 500 lines to prevent memory growth
    while (LogPanel.Children.Count > 500)
        LogPanel.Children.RemoveAt(0);
}

static TextBlock Mono(string t, string hex) => new()
{
    Text       = t,
    FontFamily = new FontFamily("Cascadia Code, Consolas"),
    FontSize   = 11,
    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex))
};

static Border Spacer(double w) => new() { Width = w };
```

**Color routing convention:**

| Level | Hex | Meaning |
|---|---|---|
| `[DBG]` | `#445566` | Debug / trace |
| `[INF]` | `#8899AA` | Info / status |
| `[OK ]` | `#00C47A` | Success |
| `[WRN]` | `#FFD700` | Warning |
| `[ERR]` | `#FF2D55` | Error |
| `[FTL]` | `#FF2D55` | Fatal (bold) |
