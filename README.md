# AnketApp

ASP.NET Core MVC, Dapper ve MSSQL ile geliştirilmiş basit bir anket uygulaması.
Anket oluşturulur, sorular ve seçenekler eklenir, anket doldurulur, sonuçlar yüzde olarak gösterilir.

## Kullanılan teknolojiler

- .NET 8, ASP.NET Core MVC
- Dapper (micro ORM), Microsoft.Data.SqlClient
- MSSQL (SQL Server Express)
- Bootstrap 5

## Mimari

```
Controller  ->  Repository (IAnketRepository)  ->  Dapper  ->  SQL Server
                                                      |-- SELECT : View
                                                      |-- INSERT / UPDATE : Stored Procedure
```

- **SELECT işlemleri view'lardan yapılır:** `vw_AnketListesi`, `vw_AnketSorulari`, `vw_AnketSonuclari`
- **INSERT / UPDATE işlemleri stored procedure ile yapılır:** `sp_AnketEkle`, `sp_AnketGuncelle`,
  `sp_SoruEkle`, `sp_SoruGuncelle`, `sp_SecenekEkle`, `sp_CevapKaydet`
- Repository, `Program.cs` içinde **Dependency Injection** ile kaydedilir ve controller'a constructor'dan gelir.
- Soru + seçenek ekleme ve anket cevaplarını kaydetme **transaction** içinde yapılır (biri hata verirse hiçbiri kaydedilmez).
- Tüm sorgular **parametreli**dir (SQL injection'a karşı güvenli).
- `sp_CevapKaydet`, seçeneğin gerçekten o soruya ait olduğunu ve anketin aktif olduğunu kontrol eder; değilse `THROW` ile hata döner.

## Veritabanı

```
Anketler (1) ──< Sorular (1) ──< Secenekler
                     │                │
                     └──< Cevaplar >──┘
```

| Tablo | Açıklama |
|---|---|
| Anketler | Anket adı, açıklaması, aktif/pasif durumu |
| Sorular | Ankete bağlı sorular ve sıraları |
| Secenekler | Soruya bağlı cevap seçenekleri |
| Cevaplar | Kullanıcının hangi soruda hangi seçeneği seçtiği |

Script'ler `Database` klasöründe, sırasıyla çalıştırılır:

1. `01_Tablolar.sql` – tablolar, primary/foreign key'ler, default'lar, index'ler
2. `02_Viewlar.sql` – view'lar
3. `03_Prosedurler.sql` – stored procedure'ler
4. `04_OrnekVeri.sql` – örnek anket (prosedürler kullanılarak eklenir)

## Çalıştırma

1. SQL Server'da `Database` klasöründeki script'leri sırayla çalıştırın (SSMS veya `sqlcmd`).
2. Gerekirse `src/AnketApp/appsettings.json` içindeki `ConnectionStrings:AnketDb` değerini kendi sunucunuza göre düzenleyin
   (varsayılan: `localhost\SQLEXPRESS`, Windows Authentication).
3. Uygulamayı başlatın:

```bash
cd src/AnketApp
dotnet run
```

Tarayıcıda `http://localhost:5000` adresini açın.

## Proje yapısı

```
Database/                 SQL script'leri (tablo, view, prosedür, örnek veri)
src/AnketApp/
  Controllers/            AnketController (listeleme, ekleme, düzenleme, doldurma, sonuçlar)
  Data/                   SQL bağlantı fabrikası
  Models/                 View satır modelleri ve form modelleri
  Repositories/           IAnketRepository + Dapper ile AnketRepository
  Views/Anket/            Razor sayfaları
```
