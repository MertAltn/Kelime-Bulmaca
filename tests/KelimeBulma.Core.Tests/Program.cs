using KelimeBulma.Core;

var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception($"BAŞARISIZ: {name}");
    Console.WriteLine($"GEÇTİ: {name}");
    passed++;
}

Check(Game.Normalize("  çilek  ") == "ÇİLEK", "Türkçe büyük harf ve boşluk dönüşümü");
Check(Game.Normalize("sakız") == "SAKIZ", "Noktasız ı dönüşümü");
Check(Game.Normalize("c\u0327ilek") == "ÇİLEK", "Birleşik Unicode Türkçe harf dönüşümü");
Check(Game.CanForm("BARDAK", "arka"), "Tekrarlanan harf kullanılabilir");
Check(!Game.CanForm("KALEM", "KALEME"), "Harf sayısı aşılamaz");
var game = new Game();
Check(!game.NextLevel(), "Tamamlanmamış bölüm geçilemez");
Check(game.Submit("a") == GuessResult.TooShort, "Tek harf reddedilir");
Check(game.Submit("masa") == GuessResult.NotInLevel && game.Score == 0, "Yanlış tahmin puan kazandırmaz");
Check(game.Submit("el") == GuessResult.Accepted && game.Score == 20, "Doğru kelime puanı");
Check(game.Submit("EL") == GuessResult.AlreadyFound && game.Score == 20, "Tekrarlanan kelime puan kazandırmaz");
var copy = new Game();
copy.Restore(game.LevelIndex, game.Found.Concat(new[] { "EL", "YANLIŞ" }));
Check(copy.Score == 20 && copy.Found.Count == 1, "Kayıt yüklenirken yinelenen ve yabancı kelimeler elenir");
Check(game.Hint().Contains("E ile"), "İpucu bulunmamış hedefi gösterir");
foreach (var word in game.Current.Words) game.Submit(word);
Check(game.IsComplete && game.Score == 170, "İlk bölüm tamamlanır ve doğru puan hesaplanır");
Check(game.NextLevel() && game.LevelIndex == 1 && game.Found.Count == 0 && game.Score == 170,
    "Bölüm geçişi puanı korur ve bulunan kelimeleri temizler");
game.Submit("kitap");
copy.Restore(game.LevelIndex, game.Found);
Check(copy.Score == game.Score && copy.Found.Contains("KİTAP"), "Bölümler arası ilerleme geri yüklenir");
while (true)
{
    foreach (var word in game.Current.Words)
    {
        Check(Game.CanForm(game.Current.Letters, word), $"Bölüm {game.LevelIndex + 1}: {word} oluşturulabilir");
        game.Submit(word);
    }
    if (!game.NextLevel()) break;
}
Check(game.IsComplete && game.IsLastLevel && game.Score == Game.Levels.Sum(l => l.Words.Sum(w => w.Length * 10)),
    "Tüm oyun tamamlanır, toplam puan tutarlıdır");
Check(!game.NextLevel(), "Son bölümden taşılmaz");
copy.Restore(game.LevelIndex, game.Found);
Check(copy.IsComplete && copy.IsLastLevel && copy.Score == game.Score, "Bitmiş oyun geri yüklenir");
game.Reset();
Check(game.LevelIndex == 0 && game.Score == 0 && game.Found.Count == 0, "Yeni oyun temiz başlar");
Console.WriteLine($"\n{passed} kontrol başarılı.");
