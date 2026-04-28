# 02 — Design Standards

## The Visual Language

Every application built with this engine speaks the same visual language.
Consistency across apps makes them feel like a suite, not a collection of one-offs.

---

## Color System

### Primary Palette (Dark Military — Default Theme)

```xml
<!-- Always defined in ResourceDictionary, never hardcoded -->
<Color x:Key="BackgroundDeep">#0A0D12</Color>       <!-- Near-black, main bg -->
<Color x:Key="BackgroundPanel">#12171F</Color>       <!-- Card/panel bg -->
<Color x:Key="BackgroundElevated">#1C2333</Color>    <!-- Hover/elevated bg -->
<Color x:Key="BorderSubtle">#2A3347</Color>          <!-- Subtle borders -->
<Color x:Key="BorderActive">#3D5A8A</Color>          <!-- Active/focused borders -->

<Color x:Key="AccentPrimary">#00B4FF</Color>         <!-- Electric blue — primary action -->
<Color x:Key="AccentSecondary">#FF6B35</Color>       <!-- Tactical orange — alerts/warnings -->
<Color x:Key="AccentSuccess">#00FF94</Color>         <!-- Mission green — success states -->
<Color x:Key="AccentDanger">#FF2D55</Color>          <!-- Red alert — errors/danger -->
<Color x:Key="AccentGold">#FFD700</Color>            <!-- Gold — achievements/highlights -->

<Color x:Key="TextPrimary">#FFFFFF</Color>           <!-- Main text -->
<Color x:Key="TextSecondary">#8899AA</Color>         <!-- Subdued text -->
<Color x:Key="TextDisabled">#445566</Color>          <!-- Disabled text -->
<Color x:Key="TextAccent">#00B4FF</Color>            <!-- Accent text/links -->
```

---

## Typography

### Font Stack

```xml
<FontFamily x:Key="FontPrimary">Segoe UI</FontFamily>
<FontFamily x:Key="FontMono">Cascadia Code, Consolas, Courier New</FontFamily>
<FontFamily x:Key="FontDisplay">Segoe UI Black, Impact</FontFamily>
```

### Font Sizes (Always Use These Keys)

```xml
<sys:Double x:Key="FontSizeXS">10</sys:Double>
<sys:Double x:Key="FontSizeSM">12</sys:Double>
<sys:Double x:Key="FontSizeMD">14</sys:Double>     <!-- Body default -->
<sys:Double x:Key="FontSizeLG">16</sys:Double>
<sys:Double x:Key="FontSizeXL">20</sys:Double>
<sys:Double x:Key="FontSizeXXL">28</sys:Double>
<sys:Double x:Key="FontSizeDisplay">48</sys:Double>
```

---

## Spacing & Layout

### Spacing Scale

```xml
<sys:Double x:Key="SpaceXS">4</sys:Double>
<sys:Double x:Key="SpaceSM">8</sys:Double>
<sys:Double x:Key="SpaceMD">16</sys:Double>
<sys:Double x:Key="SpaceLG">24</sys:Double>
<sys:Double x:Key="SpaceXL">40</sys:Double>
<sys:Double x:Key="SpaceXXL">64</sys:Double>
```

### Border Radius

```xml
<CornerRadius x:Key="RadiusNone">0</CornerRadius>
<CornerRadius x:Key="RadiusSM">4</CornerRadius>
<CornerRadius x:Key="RadiusMD">8</CornerRadius>
<CornerRadius x:Key="RadiusLG">12</CornerRadius>
<CornerRadius x:Key="RadiusFull">999</CornerRadius>
```

---

## Animation Principles

### Duration Scale

```xml
<Duration x:Key="DurationInstant">0:0:0.1</Duration>
<Duration x:Key="DurationFast">0:0:0.2</Duration>
<Duration x:Key="DurationNormal">0:0:0.35</Duration>
<Duration x:Key="DurationSlow">0:0:0.6</Duration>
<Duration x:Key="DurationDramatic">0:0:1.2</Duration>
```

### Easing Philosophy

- **Entering elements:** EaseOut (fast start, gentle landing)
- **Exiting elements:** EaseIn (gentle start, fast exit)
- **Status changes:** Linear or CubicEase
- **Dramatic reveals:** BounceEase or ElasticEase (use sparingly)

### When To Animate

| Situation | Animation |
|-----------|-----------|
| Page navigation | Slide + fade |
| Error appearance | Shake + red flash |
| Success state | Scale up + green pulse |
| Loading | Spinner or progress bar |
| Data refresh | Fade out → refresh → fade in |
| New item in list | Slide in from top |
| Delete item | Slide out + collapse height |
| Button press | Scale down 0.95 → release |

### Reduced Motion

Always add this check before animating:

```csharp
bool prefersReducedMotion = SystemParameters.MinimizeAnimation == false;
```

---

## Control Design Rules

### Buttons

- **Primary:** AccentPrimary fill, white text, subtle glow on hover
- **Secondary:** Transparent fill, AccentPrimary border, AccentPrimary text
- **Danger:** AccentDanger fill, white text
- **Ghost:** No border, TextSecondary text, BackgroundElevated on hover
- **Minimum tap target:** 40x40px

### Input Fields

- Dark background (BackgroundPanel)
- AccentPrimary border on focus (animated, 200ms)
- Error state: AccentDanger border + error message below
- Placeholder text in TextDisabled color

### Cards/Panels

- BackgroundPanel background
- BorderSubtle border (1px)
- RadiusMD corner radius
- SpaceMD padding
- Subtle drop shadow: `0 4 24 0 #33000000`

### Status Indicators

| Status | Color | Icon |
|--------|-------|------|
| Online/Active | AccentSuccess | ● |
| Idle/Standby | AccentGold | ◐ |
| Error/Offline | AccentDanger | ✕ |
| Loading | AccentPrimary | ↻ |
| Disabled | TextDisabled | — |

---

## Layout Rules

1. **Grid-based:** Use Grid for all major layouts. StackPanel only for simple linear lists.
2. **Sidebar width:** 240px expanded, 64px collapsed (icon-only mode)
3. **Title bar:** Custom-drawn, 48px tall, drag region defined
4. **Status bar:** 24px tall, always at the bottom
5. **Content padding:** SpaceLG (24px) on all sides
6. **Max content width:** 1400px — center content on ultra-wide screens

---

## Icons

Use Segoe MDL2 Assets or Segoe Fluent Icons (built into Windows).
Never use image files for icons — use font glyphs or vector paths.

```xml
<!-- Example: Search icon -->
<TextBlock FontFamily="Segoe MDL2 Assets" Text="&#xE721;" />
<!-- Example: Settings -->
<TextBlock FontFamily="Segoe MDL2 Assets" Text="&#xE713;" />
<!-- Example: Home -->
<TextBlock FontFamily="Segoe MDL2 Assets" Text="&#xE80F;" />
```

---

## Window Chrome

- **Window style:** No standard chrome — custom title bar always
- **Minimum size:** 800x600
- **Default size:** 1280x800
- **State:** Remember last position/size via settings
- **Drag region:** Top 48px of window (title bar)
- **Window buttons:** Custom X, □, — in top-right

---

## What "Finished" Looks Like

A finished screen has:
- [ ] No hardcoded pixel values (all from ResourceDictionary)
- [ ] All interactive elements have hover and press states
- [ ] All async operations show a loading indicator
- [ ] All error states are visible to the user
- [ ] All text is readable at 150% system DPI scaling
- [ ] Window can be resized from 800x600 to fullscreen without layout breaking
