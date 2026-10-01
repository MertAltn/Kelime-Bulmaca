# Kelime Bahçesi

Android öncelikli, .NET MAUI ile geliştirilmiş Türkçe kelime oluşturma oyunu.

## İlk sürüm

- 8 bölüm; internetsiz, önceden belirlenmiş Türkçe hedef kelimeler.
- Üstte kelime kutucukları, altta dairesel karışık harfler.
- Harflerin üzerinden sürükleyerek kelime oluşturma; bırakınca otomatik gönderme.
- Seçim çizgisi ve bir önceki harfe dönerek son harfi geri alma.
- Harfleri karıştırma ve ücretsiz ilk harf ipucu.
- Doğru kelimenin her harfi için 10 puan; yinelenen tahminlere puan verilmez.
- Bölümdeki bütün hedefler bulununca sonraki bölüme geçiş.
- İlerlemenin cihazda otomatik saklanması ve onayla sıfırlanması.
- Türkçe İ/i ve I/ı ayrımı; aynı harften birden fazla varsa ayrı kutular.

Bu prototip genel bir Türkçe sözlük kullanmaz. Geçerli olsa bile bölümün hedef listesinde bulunmayan kelimeler sayılmaz. Ses ve çevrimiçi özellikler henüz yoktur.

## Android'de çalıştırma

Gereksinimler: .NET 10 SDK, MAUI Android iş yükü, uyumlu Android SDK/JDK ve Android emülatörü veya USB hata ayıklaması açık fiziksel cihaz. Minimum Android sürümü 7.0 (API 24).

Microsoft'un [MAUI kurulum rehberini](https://learn.microsoft.com/dotnet/maui/get-started/installation?view=net-maui-10.0) izleyerek ortamı kurun. .NET 10 SDK kurulduktan sonra proje kökünde:

```powershell
dotnet workload install maui-android
dotnet restore src/KelimeBulma/KelimeBulma.csproj
dotnet build src/KelimeBulma/KelimeBulma.csproj -f net10.0-android
```

IDE'de `KelimeBulma.sln` dosyasını açıp `KelimeBulma` uygulamasını başlangıç projesi seçin. Android cihazını/emülatörünü seçerek çalıştırın. Tek bir cihaz bağlıyken komut satırı alternatifi:

```powershell
dotnet build src/KelimeBulma/KelimeBulma.csproj -t:Run -f net10.0-android
```

## Oyun mantığı kontrolleri

Test uygulaması ek test paketi gerektirmeyen, hata durumunda sıfırdan farklı kodla çıkan bir konsol programıdır:

```powershell
dotnet run --project tests/KelimeBulma.Core.Tests
```

Mevcut geliştirme makinesindeki .NET 6 ile yalnızca oyun mantığını kontrol etmek için:

```powershell
dotnet run --project tests/KelimeBulma.Core.Tests -p:TestTargetFramework=net6.0 --framework net6.0
```

Bu kontroller Android arayüzünü çalıştırmaz. .NET 6, Android uygulamasının hedefi değildir.

## Dosya düzeni

- `src/KelimeBulma`: MAUI ekranı, cihaz kaydı ve Android başlangıç dosyaları.
- `src/KelimeBulma.Core`: platformdan bağımsız oyun kuralları ve bölüm verileri.
- `tests/KelimeBulma.Core.Tests`: puan, Türkçe harf, bölüm geçişi ve kayıt yükleme kontrolleri.

Yeni bölüm için `Game.cs` içindeki `Levels` listesine `Level` ekleyin. Kurucu, kelimelerin bölüm harfleriyle oluşturulabildiğini kontrol eder. Bölüm sırası veya hedefler değiştirilirse kayıt biçimi sürümünü (`SaveKey`) de güncelleyin.

## Doğrulama durumu

Oyun mantığında .NET 10 ile 62 kontrol geçti. Android x64 emülatörü için `DesignCheck` yapılandırmasında derleme 0 hata ve 0 uyarıyla tamamlandı. Pixel 7 API 36 emülatöründe kutucuklar, harf çemberi, sürükleme çizgisi ve kelime gönderimi kontrol edildi. Fiziksel cihaz ve farklı ekran boyutları henüz test edilmedi.
