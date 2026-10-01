using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KelimeBulma.Core;

public sealed class Level
{
    public string Letters { get; }
    public IReadOnlyList<string> Words { get; }

    public Level(string letters, params string[] words)
    {
        Letters = Game.Normalize(letters);
        Words = Array.AsReadOnly(words.Select(Game.Normalize).ToArray());
        if (Words.Count == 0 || Words.Distinct().Count() != Words.Count ||
            Words.Any(word => word.Length < 2 || !Game.CanForm(Letters, word)))
            throw new ArgumentException("Seviye kelimeleri benzersiz olmalı ve verilen harflerle oluşturulabilmeli.");
    }
}

public enum GuessResult { Accepted, AlreadyFound, NotInLevel, TooShort }

public sealed class Game
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
    public static IReadOnlyList<Level> Levels { get; } = Array.AsReadOnly(new[]
    {
        new Level("KALEM", "EL", "EK", "AL", "KEL", "KEM", "KALEM"),
        new Level("KİTAP", "AT", "İP", "KAT", "KİT", "TAKİP", "KİTAP"),
        new Level("ORMAN", "AN", "ON", "NAR", "MOR", "ROMAN", "ORMAN"),
        new Level("DENİZ", "İN", "İZ", "DİN", "DİZ", "ZİNDE", "DENİZ"),
        new Level("BARDAK", "AK", "AR", "DAR", "KAR", "ARKA", "BARDAK"),
        new Level("BULUT", "BU", "BUL", "BUT", "ULU", "BULUT"),
        new Level("ÇİLEK", "EL", "İL", "EK", "ÇİL", "ÇEK", "ÇİLE", "ÇİLEK"),
        new Level("SAKIZ", "AK", "AZ", "KAS", "KAZ", "KIZ", "SAZ", "SAKIZ")
    });

    private readonly HashSet<string> found = new HashSet<string>(StringComparer.Ordinal);
    public int LevelIndex { get; private set; }
    public int Score { get; private set; }
    public Level Current => Levels[LevelIndex];
    public IReadOnlyCollection<string> Found => found.ToArray();
    public bool IsComplete => found.Count == Current.Words.Count;
    public bool IsLastLevel => LevelIndex == Levels.Count - 1;

    public static string Normalize(string word) => word.Trim().Normalize().ToUpper(Turkish);

    public static bool CanForm(string letters, string word)
    {
        var remaining = Normalize(letters).ToList();
        foreach (var letter in Normalize(word))
            if (!remaining.Remove(letter)) return false;
        return true;
    }

    public GuessResult Submit(string input)
    {
        var word = Normalize(input);
        if (word.Length < 2) return GuessResult.TooShort;
        if (!Current.Words.Contains(word)) return GuessResult.NotInLevel;
        if (!found.Add(word)) return GuessResult.AlreadyFound;
        Score += word.Length * 10;
        return GuessResult.Accepted;
    }

    public string Hint()
    {
        if (IsComplete) return "Bu bölümü tamamladın!";
        var word = Current.Words.First(w => !found.Contains(w));
        return $"{word.Length} harfli bir kelime: {word[0]} ile başlıyor.";
    }

    public bool NextLevel()
    {
        if (!IsComplete || IsLastLevel) return false;
        LevelIndex++;
        found.Clear();
        return true;
    }

    public void Restore(int levelIndex, IEnumerable<string> words)
    {
        LevelIndex = Math.Max(0, Math.Min(levelIndex, Levels.Count - 1));
        found.Clear();
        foreach (var word in words.Select(Normalize))
            if (Current.Words.Contains(word)) found.Add(word);
        Score = Levels.Take(LevelIndex).Sum(level => level.Words.Sum(w => w.Length * 10))
            + found.Sum(w => w.Length * 10);
    }

    public void Reset() => Restore(0, Array.Empty<string>());
}
