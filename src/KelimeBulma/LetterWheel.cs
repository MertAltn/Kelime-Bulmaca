using Microsoft.Maui.Graphics;

namespace KelimeBulma;

public sealed class LetterWheel : GraphicsView, IDrawable
{
    private char[] letters = Array.Empty<char>();
    private readonly List<int> selected = new();
    private PointF pointer;
    private bool dragging;
    private float width, height;
    public event Action<string>? WordChanged;
    public event Action<string>? WordSubmitted;
    public bool Locked { get; set; }

    public LetterWheel()
    {
        Drawable = this;
        HeightRequest = 270;
        StartInteraction += (_, e) =>
        {
            Cancel();
            if (Locked || e.Touches.Length == 0) return;
            dragging = true;
            Move(e.Touches[0]);
        };
        DragInteraction += (_, e) => { if (dragging && e.Touches.Length > 0) Move(e.Touches[0]); };
        EndInteraction += (_, e) =>
        {
            if (!dragging) return;
            if (e.Touches.Length > 0) Move(e.Touches[0]);
            var word = string.Concat(selected.Select(index => letters[index]));
            Cancel();
            if (word.Length > 0) WordSubmitted?.Invoke(word);
        };
        CancelInteraction += (_, _) => Cancel();
        SizeChanged += (_, _) => Cancel();
    }

    public void SetLetters(string value) { letters = value.ToCharArray(); Shuffle(); }
    public void Shuffle()
    {
        Cancel();
        var previous = new string(letters);
        Random.Shared.Shuffle(letters);
        if (letters.Length > 1 && new string(letters) == previous)
            letters = letters.Skip(1).Concat(letters.Take(1)).ToArray();
        SemanticProperties.SetDescription(this, $"Harfler: {string.Join(", ", letters)}. Harflerin üzerinden sürükleyip bırak.");
        Invalidate();
    }
    public void Cancel()
    {
        dragging = false; selected.Clear(); WordChanged?.Invoke(""); Invalidate();
    }
    private float Radius => Math.Min(width, height) * .34f;
    private float TileRadius => Math.Min(29, Math.Min(width, height) * .105f);
    private PointF Center(int index)
    {
        var angle = -Math.PI / 2 + 2 * Math.PI * index / letters.Length;
        return new PointF(width / 2 + (float)Math.Cos(angle) * Radius,
            height / 2 + (float)Math.Sin(angle) * Radius);
    }
    private void Move(PointF point)
    {
        pointer = point;
        for (var i = 0; i < letters.Length; i++)
        {
            var center = Center(i);
            if (Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2) > Math.Pow(TileRadius + 6, 2)) continue;
            // Backtracking removes one letter. Indices keep duplicate letter tiles independent.
            if (selected.Count > 1 && selected[^2] == i) selected.RemoveAt(selected.Count - 1);
            else if (!selected.Contains(i)) selected.Add(i);
            WordChanged?.Invoke(string.Concat(selected.Select(index => letters[index])));
            break;
        }
        Invalidate();
    }
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        width = dirtyRect.Width; height = dirtyRect.Height;
        var middle = new PointF(width / 2, height / 2);
        canvas.FillColor = Colors.White;
        canvas.FillCircle(middle, Math.Min(width, height) * .47f);
        canvas.StrokeColor = Color.FromArgb("#E6EBDC"); canvas.StrokeSize = 1;
        canvas.DrawCircle(middle, Math.Min(width, height) * .47f);
        canvas.StrokeColor = Color.FromArgb("#80B7A1"); canvas.StrokeSize = 10;
        canvas.StrokeLineCap = LineCap.Round;
        for (var i = 1; i < selected.Count; i++) canvas.DrawLine(Center(selected[i - 1]), Center(selected[i]));
        if (dragging && selected.Count > 0) canvas.DrawLine(Center(selected[^1]), pointer);
        canvas.FontColor = Color.FromArgb("#A8B8A7"); canvas.FontSize = 12;
        canvas.DrawString("✦", middle.X - 20, middle.Y - 20, 40, 40, HorizontalAlignment.Center, VerticalAlignment.Center);
        for (var i = 0; i < letters.Length; i++)
        {
            var center = Center(i); var active = selected.Contains(i);
            canvas.FillColor = Color.FromArgb(active ? "#286B55" : "#F1F3E9");
            canvas.FillCircle(center, TileRadius);
            canvas.FontColor = active ? Colors.White : Color.FromArgb("#203F36");
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold; canvas.FontSize = 29;
            canvas.DrawString(letters[i].ToString(), center.X - TileRadius, center.Y - TileRadius,
                TileRadius * 2, TileRadius * 2, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
