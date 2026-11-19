# AnketApp

ASP.NET Core MVC ve Dapper öğrenmek için yaptığım küçük bir anket uygulaması. Veritabanı olarak MSSQL kullandım.

İş yerinde de anket projeleri üzerinde çalıştığım için konuyu tanıdık bir şey seçtim. Uygulamada anket oluşturuluyor, ankete soru ve seçenek ekleniyor, anket dolduruluyor ve sonuçlar yüzde olarak gösteriliyor.

## Kullandıklarım

- .NET 8 / ASP.NET Core MVC
- Dapper
- MSSQL (SQL Server Express)
- Bootstrap

## Nasıl çalışıyor

Projede kendime şöyle bir kural koydum:

- SELECT sorgularının hepsi view üzerinden
- INSERT ve UPDATE işlemlerinin hepsi stored procedure ile

View'lar:
- `vw_AnketListesi` : anketler, soru ve cevap sayılarıyla
- `vw_AnketSorulari` : bir anketin soruları ve seçenekleri
- `vw_AnketSonuclari` : her seçeneğin kaç kere seçildiği

Prosedürler:
- `sp_AnketEkle`, `sp_AnketGuncelle`
- `sp_SoruEkle`, `sp_SoruGuncelle`
- `sp_SecenekEkle`
- `sp_CevapKaydet` : cevabı kaydetmeden önce seçenek o soruya ait mi, anket aktif mi diye kontrol ediyor. Değilse THROW ile hata dönüyor.

C# tarafında veritabanı işlemlerini `AnketRepository` sınıfında topladım. Bir soruyu seçenekleriyle birlikte eklerken ve anket cevaplarını kaydederken transaction kullandım, yarıda hata olursa yarım kayıt kalmasın diye.

## Tablolar

- **Anketler**
- **Sorular** (Anketler'e bağlı)
- **Secenekler** (Sorular'a bağlı)
- **Cevaplar** (Sorular'a ve Secenekler'e bağlı)

## Kurulum

1. `Database` klasöründeki scriptleri SSMS'te sırayla çalıştırın:
   `01_Tablolar.sql`, `02_Viewlar.sql`, `03_Prosedurler.sql`, `04_OrnekVeri.sql`
2. Bağlantı cümlesi `src/AnketApp/appsettings.json` içinde. Ben `localhost\SQLEXPRESS` ve Windows Authentication kullandım, sizde farklıysa oradan değiştirin.
3. Sonra:

```
cd src/AnketApp
dotnet run
```

`http://localhost:5000` adresinden açılıyor.

## Bu projede öğrendiklerim

- Dapper ile view'dan veri çekmek ve stored procedure çağırmak
- Dapper'ın kolon adlarını sınıfın property'leriyle nasıl eşlediği
- Foreign key'lerin hangi tabloya yazılacağı ve tabloların hangi sırayla oluşturulup silineceği
- Transaction kullanımı

## İleride eklemek istediklerim

- Kullanıcı girişi
- Soru ve seçenek silme
- Sonuçları Excel'e aktarma
