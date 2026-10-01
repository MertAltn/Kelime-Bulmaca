using KelimeBulma.Core;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace KelimeBulma;

public sealed class MainPage : ContentPage
{
    private const string SaveKey = "garden.progress.v1";
    private static readonly Color Ink = Color.FromArgb("#203F36");
    private static readonly Color Green = Color.FromArgb("#286B55");
    private static readonly Color Muted = Color.FromArgb("#778578");
    private readonly Game game = new();
    private readonly LetterWheel wheel = new();
    private readonly Label level = Text("", 12, Muted);
    private readonly Label score = Text("", 16, Ink);
    private readonly Label progress = Text("", 12, Muted);
    private readonly Label draft = Text("HARFLERİ BİRLEŞTİR", 19, Green);
    private readonly Label message = Text("", 13, Muted);
    private readonly FlexLayout words = new()
    {
        Wrap = FlexWrap.Wrap, JustifyContent = FlexJustify.Center,
        AlignContent = FlexAlignContent.Center, AlignItems = FlexAlignItems.Center
    };
    private readonly ProgressBar meter = new() { ProgressColor = Green, BackgroundColor = Color.FromArgb("#E1E7D8"), HeightRequest = 5 };
    private readonly Button next, shuffle, hint;

    public MainPage()
    {
        Title = "Kelime Bahçesi";
        BackgroundColor = Color.FromArgb("#F6F5EC");
        Restore();
        var heading = new Grid { ColumnDefinitions = { new(GridLength.Star), new(GridLength.Auto) } };
        heading.Add(new VerticalStackLayout { Spacing = 4, Children =
        {
            Text("KELİME BAHÇESİ", 11, Muted), Text("Kelimeler çiçek açsın.", 23, Ink)
        } });
        heading.Add(Card(score, "#E9EEDA", 14, new Thickness(15, 10)), 1);
        var section = new Grid { ColumnDefinitions = { new(GridLength.Star), new(GridLength.Auto) } };
        section.Add(level); section.Add(progress, 1);
        // Only the board scrolls, so Android never steals the wheel's drag gesture.
        var board = new Grid { RowSpacing = 14, RowDefinitions = { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star) } };
        board.Add(section); board.Add(meter, 0, 1);
        board.Add(new ScrollView { Content = words }, 0, 2);
        draft.HorizontalTextAlignment = TextAlignment.Center;
        draft.FontAttributes = FontAttributes.Bold; draft.CharacterSpacing = 2;
        message.HorizontalTextAlignment = TextAlignment.Center; message.HeightRequest = 38;
        wheel.WordChanged += word => draft.Text = word.Length == 0 ? "HARFLERİ BİRLEŞTİR" : word;
        wheel.WordSubmitted += Submit;
        shuffle = Action("↻  Karıştır", () => wheel.Shuffle());
        hint = Action("✦  İpucu", () => message.Text = game.Hint());
        next = Action("Sonraki bahçe →", Next, true);
        var reset = Action("↺", async () =>
        {
            if (!await DisplayAlertAsync("Yeni oyun", "Puanın ve bölüm ilerlemen sıfırlansın mı?", "Sıfırla", "Vazgeç")) return;
            game.Reset(); Save(); Render(); wheel.SetLetters(game.Current.Letters);
        });
        SemanticProperties.SetDescription(reset, "Oyuna baştan başla");
        var actions = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new(GridLength.Star), new(GridLength.Auto), new(GridLength.Star) } };
        actions.Add(shuffle); actions.Add(reset, 1); actions.Add(hint, 2);
        var controls = new VerticalStackLayout { Spacing = 4, Children =
        {
            draft, wheel, message, actions, next,
            new Label { Text = "Harflerden geç • Bırakınca kelimeni bul", FontSize = 11,
                TextColor = Muted, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 8, 0, 0) }
        } };
        var layout = new Grid
        {
            Padding = new Thickness(20, 16, 20, 12), RowSpacing = 18,
            RowDefinitions = { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) }
        };
        layout.Add(heading); layout.Add(board, 0, 1); layout.Add(controls, 0, 2);
        Content = layout;
        SizeChanged += (_, _) => wheel.HeightRequest = Height < 700 ? 220 : 270;
        Render(); wheel.SetLetters(game.Current.Letters);
    }

    private static Label Text(string value, double size, Color color) => new()
    {
        Text = value, FontSize = size, TextColor = color, VerticalTextAlignment = TextAlignment.Center
    };
    private static Border Card(View child, string color, float radius, Thickness padding) => new()
    {
        Content = child, BackgroundColor = Color.FromArgb(color), StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(radius) }, Padding = padding
    };
    private static Button Action(string text, System.Action callback, bool primary = false)
    {
        var button = new Button
        {
            Text = text, CornerRadius = 18, MinimumHeightRequest = 48, FontSize = 14,
            FontAttributes = FontAttributes.Bold, Padding = new Thickness(14, 8),
            BackgroundColor = primary ? Green : Color.FromArgb("#E9EEDA"), TextColor = primary ? Colors.White : Ink
        };
        button.Clicked += (_, _) => callback(); return button;
    }
    private void Render()
    {
        level.Text = $"BAHÇE {game.LevelIndex + 1:00} / {Game.Levels.Count:00}";
        score.Text = $"✦  {game.Score}";
        progress.Text = $"{game.Found.Count} / {game.Current.Words.Count} kelime";
        meter.Progress = (double)game.Found.Count / game.Current.Words.Count;
        words.Children.Clear();
        foreach (var word in game.Current.Words.OrderBy(w => w.Length))
        {
            var found = game.Found.Contains(word);
            var row = new HorizontalStackLayout { Spacing = 4, Margin = new Thickness(5, 7) };
            foreach (var letter in word)
            {
                var label = Text(found ? letter.ToString() : "", 21, Colors.White);
                label.FontAttributes = FontAttributes.Bold; label.HorizontalTextAlignment = TextAlignment.Center;
                var tile = Card(label, found ? "#286B55" : "#FFFFFF", 9, 0);
                tile.WidthRequest = 35; tile.HeightRequest = 41;
                tile.StrokeThickness = found ? 0 : 1;
                tile.Stroke = new SolidColorBrush(Color.FromArgb("#DCE2D1"));
                row.Children.Add(tile);
            }
            SemanticProperties.SetDescription(row, found ? $"Bulunan kelime: {word}" : $"{word.Length} harfli kelime");
            words.Children.Add(row);
        }
        wheel.Locked = game.IsComplete; wheel.Cancel();
        next.IsVisible = game.IsComplete;
        next.Text = game.IsLastLevel ? "Bütün bahçeler tamamlandı ✓" : "Sonraki bahçe →";
        next.IsEnabled = !game.IsLastLevel;
        shuffle.IsEnabled = hint.IsEnabled = !game.IsComplete;
        message.Text = game.IsComplete ? "Harika! Bütün kelimeler çiçek açtı." : "Parmağını harflerin üzerinde sürükle.";
        message.TextColor = Muted;
    }
    private void Submit(string word)
    {
        var result = game.Submit(word);
        if (result == GuessResult.Accepted)
        {
            Save(); Render();
            if (!game.IsComplete) message.Text = $"{word}  •  +{word.Length * 10} puan. Çok iyi!";
            message.TextColor = Green;
        }
        else
        {
            message.Text = result switch
            {
                GuessResult.AlreadyFound => "Bu kelime zaten bahçende. Bir yenisini bul!",
                GuessResult.TooShort => "En az iki harfi birleştir.",
                _ => "Bu kelime kutulara uymadı. Bir daha dene."
            };
            message.TextColor = Color.FromArgb("#A15D3C");
        }
    }
    private void Next()
    {
        if (!game.NextLevel()) return;
        Save(); Render(); wheel.SetLetters(game.Current.Letters);
    }
    protected override void OnDisappearing() { wheel.Cancel(); base.OnDisappearing(); }
    private void Save() => Preferences.Default.Set(SaveKey, $"{game.LevelIndex}|{string.Join(",", game.Found)}");
    private void Restore()
    {
        var parts = Preferences.Default.Get(SaveKey, "").Split('|');
        if (parts.Length == 2 && int.TryParse(parts[0], out var index))
            game.Restore(index, parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries));
    }
}
