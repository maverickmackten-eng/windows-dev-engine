using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EmojiShowcase
{
    public record EmojiEntry(string Emoji, string Name, string Codepoint);

    public partial class MainWindow : Window
    {
        private static readonly Dictionary<string, List<EmojiEntry>> _categories = new()
        {
            ["Faces"] = new()
            {
                new("😀", "Grinning Face",     "U+1F600"),
                new("😂", "Tears of Joy",      "U+1F602"),
                new("😎", "Sunglasses",        "U+1F60E"),
                new("😐", "Neutral Face",      "U+1F610"),
                new("😡", "Angry Face",        "U+1F621"),
                new("😥", "Disappointed",      "U+1F625"),
                new("😱", "Fearful",           "U+1F631"),
                new("🤔", "Thinking",          "U+1F914"),
                new("🤯", "Mind Blown",        "U+1F92F"),
                new("😈", "Smiling Devil",     "U+1F608"),
                new("👽", "Alien",             "U+1F47D"),
                new("🤖", "Robot",             "U+1F916"),
            },
            ["Symbols"] = new()
            {
                new("⚡", "Lightning",        "U+26A1"),
                new("✅", "Check Mark",       "U+2705"),
                new("❌", "Cross Mark",       "U+274C"),
                new("❗", "Exclamation",      "U+2757"),
                new("❓", "Question Mark",    "U+2753"),
                new("⭐", "Star",             "U+2B50"),
                new("🔥", "Fire",             "U+1F525"),
                new("🛡", "Shield",           "U+1F6E1"),
                new("🔒", "Locked",           "U+1F512"),
                new("🔓", "Unlocked",         "U+1F513"),
                new("♻", "Recycle",          "U+267B"),
                new("☢", "Radiation",        "U+2622"),
            },
            ["Objects"] = new()
            {
                new("🔧", "Wrench",           "U+1F527"),
                new("🔫", "Gun",              "U+1F52B"),
                new("💻", "Laptop",           "U+1F4BB"),
                new("📱", "Mobile Phone",     "U+1F4F1"),
                new("📡", "Satellite",        "U+1F4E1"),
                new("🔍", "Magnifier",        "U+1F50D"),
                new("📦", "Package",          "U+1F4E6"),
                new("📁", "File Folder",      "U+1F4C1"),
                new("⚙", "Gear",             "U+2699"),
                new("💡", "Light Bulb",       "U+1F4A1"),
                new("🔋", "Battery",          "U+1F50B"),
                new("📞", "Telephone",        "U+1F4DE"),
            },
            ["Nature"] = new()
            {
                new("🌟", "Glowing Star",     "U+1F31F"),
                new("🌍", "Earth Globe",      "U+1F30D"),
                new("🌌", "Milky Way",        "U+1F30C"),
                new("🌩", "Cloud Lightning",  "U+1F329"),
                new("☃", "Snowman",          "U+2603"),
                new("🔥", "Flame",            "U+1F525"),
                new("🌊", "Water Wave",       "U+1F30A"),
                new("🐘", "Elephant",         "U+1F418"),
                new("🐈", "Cat",              "U+1F408"),
                new("🐕", "Dog",              "U+1F415"),
                new("🐲", "Dragon Face",      "U+1F432"),
                new("🦅", "Eagle",            "U+1F985"),
            },
            ["Actions"] = new()
            {
                new("💥", "Collision",        "U+1F4A5"),
                new("🔫", "Weapon",           "U+1F52B"),
                new("🏆", "Trophy",           "U+1F3C6"),
                new("🚀", "Rocket",           "U+1F680"),
                new("💣", "Bomb",             "U+1F4A3"),
                new("🧨", "DNA",              "U+1F9E8"),
                new("💊", "Pill",             "U+1F48A"),
                new("⚽", "Soccer Ball",      "U+26BD"),
                new("🎯", "Direct Hit",       "U+1F3AF"),
                new("🏃", "Runner",           "U+1F3C3"),
                new("🥊", "Boxing Glove",     "U+1F94A"),
                new("🗡", "Dagger",           "U+1F5E1"),
            },
        };

        private EmojiEntry? _selected;

        public MainWindow()
        {
            InitializeComponent();
            LoadCategory("Faces");
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string category)
                LoadCategory(category);
        }

        private void LoadCategory(string category)
        {
            emojiPanel.Children.Clear();
            if (!_categories.TryGetValue(category, out var list)) return;

            foreach (var entry in list)
            {
                var btn = new Button
                {
                    Content     = new TextBlock
                    {
                        Text       = entry.Emoji,
                        FontFamily = new FontFamily("Segoe UI Emoji"),
                        FontSize   = 28,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment   = VerticalAlignment.Center,
                    },
                    Width           = 52,
                    Height          = 52,
                    Background      = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor          = Cursors.Hand,
                    ToolTip         = $"{entry.Name}  {entry.Codepoint}",
                    Tag             = entry,
                };
                btn.Click += EmojiBtn_Click;
                emojiPanel.Children.Add(btn);
            }

            // Select the first one by default
            if (list.Count > 0) SelectEmoji(list[0]);
        }

        private void EmojiBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is EmojiEntry entry)
                SelectEmoji(entry);
        }

        private void SelectEmoji(EmojiEntry entry)
        {
            _selected                = entry;
            selectedEmoji.Text      = entry.Emoji;
            selectedName.Text       = entry.Name;
            selectedCodepoint.Text  = entry.Codepoint;
            btnEmoji.Text           = entry.Emoji;
            statusEmoji.Text        = $"{entry.Emoji} Ready";
        }
    }
}
