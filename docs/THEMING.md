# Theming Guide

Windows Dev Engine uses WPF `ResourceDictionary` files for theming.
All colors, brushes, fonts, and control styles are declared in a single theme file.
Switching themes at runtime requires only swapping the merged dictionary.

---

## Theme File Location

```
MyApp/
  Themes/
    DarkMilitary.xaml     ← default (dark, blue-accent)
    WarmOrange.xaml       ← alternate (dark, orange-accent)
    HighContrast.xaml     ← accessibility variant
```

Merge your chosen theme in `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/DarkMilitary.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

---

## Full ResourceDictionary Key Reference

Every key below must be defined in your theme file.
Do not hardcode colors in XAML — always use `{StaticResource}` or `{DynamicResource}` (use `DynamicResource` if you support runtime theme switching).

### Background Layers

| Key | Type | DarkMilitary | WarmOrange | Usage |
|---|---|---|---|---|
| `BgDeep` | `SolidColorBrush` | `#0A0D12` | `#0F0A06` | Window background |
| `BgPanel` | `SolidColorBrush` | `#12171F` | `#17100A` | Cards, panels |
| `BgSide` | `SolidColorBrush` | `#0E1318` | `#120E08` | Sidebar, toolbar |
| `BgInput` | `SolidColorBrush` | `#0E1318` | `#120E08` | TextBox, ComboBox bg |
| `BgHover` | `SolidColorBrush` | `#0F1E2D` | `#1C1206` | Control hover state |
| `BgSelected` | `SolidColorBrush` | `#0D3A5C` | `#3A1A00` | Selected row/item |

### Borders

| Key | Type | DarkMilitary | WarmOrange | Usage |
|---|---|---|---|---|
| `Border1` | `SolidColorBrush` | `#1E2D3D` | `#2D1F0E` | Primary borders |
| `Border2` | `SolidColorBrush` | `#2A3347` | `#3D2A14` | Secondary / dividers |

### Text

| Key | Type | DarkMilitary | WarmOrange | Usage |
|---|---|---|---|---|
| `TextPrimary` | `SolidColorBrush` | `#FFFFFF` | `#FFF5E6` | Headings, values |
| `TextSecondary` | `SolidColorBrush` | `#8899AA` | `#AA9077` | Labels, hints |
| `TextDisabled` | `SolidColorBrush` | `#445566` | `#664433` | Disabled controls |
| `TextCode` | `SolidColorBrush` | `#CCDDE8` | `#E8D4BB` | Monospace / code |

### Accent Colors

| Key | Type | DarkMilitary | WarmOrange | Usage |
|---|---|---|---|---|
| `Accent` | `SolidColorBrush` | `#00B4FF` | `#FF6B00` | Primary CTAs, highlights |
| `AccentDim` | `SolidColorBrush` | `#0080CC` | `#CC5500` | Pressed state |
| `AccentGlow` | `Color` | `#3300B4FF` | `#33FF6B00` | Drop shadows, glows |
| `AccentFg` | `SolidColorBrush` | `#0A0D12` | `#0F0A06` | Text on accent bg |

### Semantic Colors

| Key | Type | Value | Usage |
|---|---|---|---|
| `ColorSuccess` | `SolidColorBrush` | `#00C47A` | Success badges, indicators |
| `ColorWarning` | `SolidColorBrush` | `#FFD700` | Warning badges, toasts |
| `ColorError` | `SolidColorBrush` | `#FF2D55` | Error badges, toasts |
| `ColorInfo` | `SolidColorBrush` | `#00B4FF` | Info badges, toasts |
| `ColorSuccessBg` | `SolidColorBrush` | `#0A2E1A` | Success badge background |
| `ColorWarningBg` | `SolidColorBrush` | `#2A1F00` | Warning badge background |
| `ColorErrorBg` | `SolidColorBrush` | `#2A0A10` | Error badge background |
| `ColorInfoBg` | `SolidColorBrush` | `#00192A` | Info badge background |

### Typography

| Key | Type | Value | Usage |
|---|---|---|---|
| `FontPrimary` | `FontFamily` | `Segoe UI` | UI labels, body text |
| `FontDisplay` | `FontFamily` | `Segoe UI Black` | Headings, titles |
| `FontMono` | `FontFamily` | `Cascadia Code, Consolas` | Code, log output, numbers |
| `FontSizeXS` | `sys:Double` | `9` | Micro labels, badges |
| `FontSizeSM` | `sys:Double` | `11` | Secondary info |
| `FontSizeMD` | `sys:Double` | `13` | Body / default |
| `FontSizeLG` | `sys:Double` | `16` | Subheadings |
| `FontSizeXL` | `sys:Double` | `22` | Page titles |
| `FontSizeHero` | `sys:Double` | `32` | Hero numbers / stat displays |

### Dimensions

| Key | Type | Value | Usage |
|---|---|---|---|
| `RadiusSM` | `CornerRadius` | `4` | Badges, small elements |
| `RadiusMD` | `CornerRadius` | `8` | Inputs, cards |
| `RadiusLG` | `CornerRadius` | `12` | Modal dialogs, large cards |
| `TitleBarHeight` | `sys:Double` | `48` | Window chrome |
| `SidebarWidth` | `sys:Double` | `200` | Navigation sidebar |
| `SidebarCollapsed` | `sys:Double` | `52` | Collapsed sidebar |

---

## Minimal Theme File Template

Create `Themes/DarkMilitary.xaml`:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:sys="clr-namespace:System;assembly=mscorlib">

    <!-- Backgrounds -->
    <SolidColorBrush x:Key="BgDeep"      Color="#0A0D12"/>
    <SolidColorBrush x:Key="BgPanel"     Color="#12171F"/>
    <SolidColorBrush x:Key="BgSide"      Color="#0E1318"/>
    <SolidColorBrush x:Key="BgInput"     Color="#0E1318"/>
    <SolidColorBrush x:Key="BgHover"     Color="#0F1E2D"/>
    <SolidColorBrush x:Key="BgSelected"  Color="#0D3A5C"/>

    <!-- Borders -->
    <SolidColorBrush x:Key="Border1"     Color="#1E2D3D"/>
    <SolidColorBrush x:Key="Border2"     Color="#2A3347"/>

    <!-- Text -->
    <SolidColorBrush x:Key="TextPrimary"   Color="#FFFFFF"/>
    <SolidColorBrush x:Key="TextSecondary" Color="#8899AA"/>
    <SolidColorBrush x:Key="TextDisabled"  Color="#445566"/>
    <SolidColorBrush x:Key="TextCode"      Color="#CCDDE8"/>

    <!-- Accent -->
    <SolidColorBrush x:Key="Accent"      Color="#00B4FF"/>
    <SolidColorBrush x:Key="AccentDim"   Color="#0080CC"/>
    <Color           x:Key="AccentGlow"         >#3300B4FF</Color>
    <SolidColorBrush x:Key="AccentFg"    Color="#0A0D12"/>

    <!-- Semantic -->
    <SolidColorBrush x:Key="ColorSuccess"   Color="#00C47A"/>
    <SolidColorBrush x:Key="ColorWarning"   Color="#FFD700"/>
    <SolidColorBrush x:Key="ColorError"     Color="#FF2D55"/>
    <SolidColorBrush x:Key="ColorInfo"      Color="#00B4FF"/>
    <SolidColorBrush x:Key="ColorSuccessBg" Color="#0A2E1A"/>
    <SolidColorBrush x:Key="ColorWarningBg" Color="#2A1F00"/>
    <SolidColorBrush x:Key="ColorErrorBg"   Color="#2A0A10"/>
    <SolidColorBrush x:Key="ColorInfoBg"    Color="#00192A"/>

    <!-- Typography -->
    <FontFamily x:Key="FontPrimary">Segoe UI</FontFamily>
    <FontFamily x:Key="FontDisplay">Segoe UI Black</FontFamily>
    <FontFamily x:Key="FontMono">Cascadia Code, Consolas</FontFamily>
    <sys:Double x:Key="FontSizeXS">9</sys:Double>
    <sys:Double x:Key="FontSizeSM">11</sys:Double>
    <sys:Double x:Key="FontSizeMD">13</sys:Double>
    <sys:Double x:Key="FontSizeLG">16</sys:Double>
    <sys:Double x:Key="FontSizeXL">22</sys:Double>
    <sys:Double x:Key="FontSizeHero">32</sys:Double>

    <!-- Dimensions -->
    <CornerRadius x:Key="RadiusSM">4</CornerRadius>
    <CornerRadius x:Key="RadiusMD">8</CornerRadius>
    <CornerRadius x:Key="RadiusLG">12</CornerRadius>
    <sys:Double x:Key="TitleBarHeight">48</sys:Double>
    <sys:Double x:Key="SidebarWidth">200</sys:Double>
    <sys:Double x:Key="SidebarCollapsed">52</sys:Double>
</ResourceDictionary>
```

---

## Runtime Theme Switching

To swap themes at runtime without restarting the app:

```csharp
public static void SwitchTheme(string themeName)
{
    var uri = new Uri($"Themes/{themeName}.xaml", UriKind.Relative);
    var dict = new ResourceDictionary { Source = uri };

    // Remove current theme (first merged dictionary by convention)
    var existing = Application.Current.Resources.MergedDictionaries;
    if (existing.Count > 0)
        existing.RemoveAt(0);

    existing.Insert(0, dict);
    Log.Debug("[Theme] Switched to {Theme}", themeName);
}
```

This works because all controls use `{DynamicResource}` instead of `{StaticResource}`.
If you only need a fixed theme, `{StaticResource}` is faster (resolved at load time).

---

## Creating a Custom Theme

1. Copy `Themes/DarkMilitary.xaml` to `Themes/MyTheme.xaml`
2. Change the accent colors:
   ```xml
   <SolidColorBrush x:Key="Accent"    Color="#7B2FBE"/>  <!-- purple -->
   <SolidColorBrush x:Key="AccentDim" Color="#5A1F9A"/>
   <SolidColorBrush x:Key="AccentFg"  Color="#FFFFFF"/>
   ```
3. Update background layer hues to complement your accent
4. Merge `MyTheme.xaml` in `App.xaml` or call `SwitchTheme("MyTheme")` at runtime

> **Rule:** Never hardcode `#00B4FF` or any other color hex in a `.xaml` control file.
> Always use `{DynamicResource Accent}`. This is the single biggest factor that makes
> theming actually work.
