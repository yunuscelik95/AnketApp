# AnketApp

ASP.NET Core MVC (.NET 8), Dapper ve MSSQL ile yazılmış basit bir anket uygulaması.

## Özellikler

- Anket ekleme, düzenleme, silme ve arama
- Ankete soru ve seçenek ekleme, soru silme
- Anket doldurma ve sonuçları yüzde olarak görme

## Kurulum

1. `Database/AnketDb.sql` dosyasını SSMS'te çalıştırın.
2. Bağlantı cümlesi `src/AnketApp/appsettings.json` içinde. Farklıysa değiştirin.
3. `src/AnketApp` klasöründe `dotnet run` yazın, `http://localhost:5000` adresinden açın.
