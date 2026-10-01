using KelimeBulma.Core;
using Microsoft.Maui.Controls.Shapes;

namespace KelimeBulma;

public sealed class MainPage : ContentPage
{
    private const string SaveKey = "garden.progress.v1";
    private static readonly Color Ink = Color.FromArgb("#183D36");
    private static readonly Color Muted = Color.FromArgb("#687C72");
    private static readonly Color Cream = Color.FromArgb("#F7F4EB");
    private readonly Game game = new();
    private readonly Random random = new();
    private readonly List<Button> selected = new();
    private readonly Label level = Text("", 13, Muted);
    private readonly Label score = Text("", 16, Ink);
    private readonly Label progress = Text("", 14, Muted);
    private readonly Label draft = Text("Harfleri seç", 30, Ink);
    private readonly Label message = Text("", 14, Muted);
    private readonly VerticalStackLayout words = new() { Spacing = 8 };
    private readonly FlexLayout letters = new()
    {
        Wrap = FlexWrap.Wrap, JustifyContent = FlexJustify.Center,
        AlignItems = FlexAlignItems.Center
    };
    private readonly Button submit;
    private readonly Button next;
    private readonly Button shuffle;
    private readonly Button hint;

    public MainPage()
    {
        Title = "Kelime Bahçesi";
        BackgroundColor = Cream;
        Restore();

        var heading = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } }
        };
        heading.Add(new VerticalStackLayout { Spacing = 6, Children =
        {
            Text("KELİME BAHÇESİ", 12, Muted), Text("Biraz düşün,\nbir kelime bul.", 30, Ink)
        } });
        score.VerticalOptions = LayoutOptions.Center;
        heading.Add(score, 1);
        draft.HorizontalTextAlignment = TextAlignment.Center;
        draft.FontAttributes = FontAttributes.Bold;
        message.HorizontalTextAlignment = TextAlignment.Center;
        message.MinimumHeightRequest = 44;

        submit = Action("Kelimeyi gönder", Submit);
        next = Action("Sonraki bölüm →", Next);
        shuffle = Action("Karıştır", Shuffle, secondary: true);
        hint = Action("İpucu", () => message.Text = game.Hint(), secondary: true);
        var undo = Action("Geri al", Undo, secondary: true);
        var clear = Action("Temizle", Clear, secondary: true);
        var reset = Action("Baştan başla", async () =>
        {
            if (!await DisplayAlertAsync("Baştan başla", "Puanın ve bölüm ilerlemen sıfırlansın mı?", "Sıfırla", "Vazgeç")) return;
            game.Reset();
            Save();
            Render();
            Shuffle();
        }, secondary: true);

        var body = new VerticalStackLayout
        {
            Padding = new Thickness(22, 24, 22, 30), Spacing = 20,
            Children =
            {
                heading,
                new Border
                {
                    BackgroundColor = Colors.White, StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
                    Padding = 20,
                    Content = new VerticalStackLayout { Spacing = 12, Children = { level, progress, words } }
                },
                new VerticalStackLayout { Spacing = 12, Children =
                {
                    Text("Harfleri sırayla seçerek kelime oluştur.", 14, Muted),
                    draft, letters, Pair(undo, clear), submit, message, Pair(shuffle, hint), next
                } },
                Text("Her harf 10 puan • İpuçları ücretsiz\nHer bölümde yalnızca belirlenen hedef kelimeler sayılır.", 12, Muted),
                reset
            }
        };
        Content = new ScrollView { Content = body };
        Render();
        Shuffle();
    }

    private static Label Text(string value, double size, Color color) => new()
    {
        Text = value, FontSize = size, TextColor = color
    };

    private static Button Action(string text, System.Action callback, bool secondary = false)
    {
        var button = new Button
        {
            Text = text, CornerRadius = 16, MinimumHeightRequest = 48,
            FontSize = 15, FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(12, 10),
            BackgroundColor = secondary ? Color.FromArgb("#E7EBDD") : Ink,
            TextColor = secondary ? Ink : Colors.White
        };
        button.Clicked += (_, _) => callback();
        return button;
    }

    private static Grid Pair(View left, View right)
    {
        var grid = new Grid { ColumnSpacing = 10, ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Star }
        } };
        grid.Add(left);
        grid.Add(right, 1);
        return grid;
    }

    private void Render()
    {
        level.Text = $"BÖLÜM {game.LevelIndex + 1:00} / {Game.Levels.Count:00}";
        score.Text = $"{game.Score}\nPUAN";
        progress.Text = $"{game.Found.Count} / {game.Current.Words.Count} kelime bulundu";
        words.Children.Clear();
        foreach (var word in game.Current.Words.OrderBy(w => w.Length))
        {
            var found = game.Found.Contains(word);
            var row = Text(found ? $"✓  {word}" : string.Join("  ", Enumerable.Repeat("_", word.Length)), 20, found ? Ink : Muted);
            row.FontAttributes = found ? FontAttributes.Bold : FontAttributes.None;
            SemanticProperties.SetDescription(row, found ? $"Bulunan kelime: {word}" : $"{word.Length} harfli kelime henüz bulunmadı");
            words.Children.Add(row);
        }
        next.IsVisible = game.IsComplete;
        next.Text = game.IsLastLevel ? "Tüm bölümler tamamlandı!" : "Sonraki bölüm →";
        next.IsEnabled = !game.IsLastLevel;
        shuffle.IsEnabled = hint.IsEnabled = !game.IsComplete;
        Clear();
        message.Text = game.IsComplete
            ? game.IsLastLevel ? $"Tebrikler! Bahçendeki tüm kelimeleri buldun. Toplam {game.Score} puan!" : "Harika! Bu bölümdeki tüm kelimeleri buldun."
            : "Bakalım ilk hangi kelimeyi bulacaksın?";
    }

    private void Shuffle()
    {
        Clear();
        letters.Children.Clear();
        var shuffled = game.Current.Letters.ToCharArray();
        for (var i = shuffled.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        foreach (var letter in shuffled)
        {
            var tile = new Button
            {
                Text = letter.ToString(), WidthRequest = 64, HeightRequest = 64,
                Margin = 5, Padding = 0, CornerRadius = 20, FontSize = 26,
                FontAttributes = FontAttributes.Bold, BackgroundColor = Color.FromArgb("#EADDCA"), TextColor = Ink,
                IsEnabled = !game.IsComplete
            };
            SemanticProperties.SetDescription(tile, $"{letter} harfini seç");
            tile.Clicked += (_, _) =>
            {
                selected.Add(tile);
                tile.IsEnabled = false;
                UpdateDraft();
            };
            letters.Children.Add(tile);
        }
    }

    private void UpdateDraft()
    {
        draft.Text = selected.Count == 0 ? "Harfleri seç" : string.Concat(selected.Select(b => b.Text));
        submit.IsEnabled = selected.Count >= 2 && !game.IsComplete;
    }

    private void Undo()
    {
        if (selected.Count == 0) return;
        selected[^1].IsEnabled = !game.IsComplete;
        selected.RemoveAt(selected.Count - 1);
        UpdateDraft();
    }

    private void Clear()
    {
        selected.Clear();
        foreach (var tile in letters.Children.OfType<Button>()) tile.IsEnabled = !game.IsComplete;
        UpdateDraft();
    }

    private void Submit()
    {
        var word = string.Concat(selected.Select(b => b.Text));
        var result = game.Submit(word);
        if (result == GuessResult.Accepted)
        {
            Save();
            Render();
            if (!game.IsComplete) message.Text = $"{word} doğru! +{word.Length * 10} puan";
            return;
        }
        message.Text = result switch
        {
            GuessResult.AlreadyFound => "Bu kelimeyi zaten buldun. Başka bir kelime dene.",
            GuessResult.TooShort => "En az iki harf seç.",
            _ => "Bu kelime bölümün hedefleri arasında değil. Tekrar dene."
        };
        Clear();
    }

    private void Next()
    {
        if (!game.NextLevel()) return;
        Save();
        Render();
        Shuffle();
    }

    private void Save() => Preferences.Default.Set(SaveKey,
        $"{game.LevelIndex}|{string.Join(",", game.Found)}");

    private void Restore()
    {
        var parts = Preferences.Default.Get(SaveKey, "").Split('|');
        if (parts.Length == 2 && int.TryParse(parts[0], out var index))
            game.Restore(index, parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries));
    }
}
