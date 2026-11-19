using System.Data;
using AnketApp.Data;
using AnketApp.Models;
using Dapper;

namespace AnketApp.Repositories;

/// <summary>
/// Veritabanı erişimi Dapper ile yapılır.
/// Okuma işlemleri view'lardan, yazma işlemleri stored procedure'lerden geçer.
/// Tüm değerler parametre olarak gönderilir (SQL injection'a karşı güvenli).
/// </summary>
public class AnketRepository : IAnketRepository
{
    private readonly IDbBaglantiFabrikasi _baglanti;

    public AnketRepository(IDbBaglantiFabrikasi baglanti)
    {
        _baglanti = baglanti;
    }

    public async Task<IEnumerable<Anket>> TumunuGetirAsync()
    {
        using var db = _baglanti.Olustur();
        return await db.QueryAsync<Anket>(
            "SELECT * FROM dbo.vw_AnketListesi ORDER BY OlusturmaTarihi DESC");
    }

    public async Task<Anket?> GetirAsync(int id)
    {
        using var db = _baglanti.Olustur();
        return await db.QueryFirstOrDefaultAsync<Anket>(
            "SELECT * FROM dbo.vw_AnketListesi WHERE Id = @Id", new { Id = id });
    }

    public async Task<List<Soru>> SorulariGetirAsync(int anketId)
    {
        using var db = _baglanti.Olustur();
        var satirlar = await db.QueryAsync<AnketSoruSatiri>(
            "SELECT * FROM dbo.vw_AnketSorulari WHERE AnketId = @AnketId ORDER BY Sira, SoruId, SecenekId",
            new { AnketId = anketId });

        // Her satır soru+seçenek; soruya göre gruplayıp seçenekleri listeye topluyoruz
        return satirlar
            .GroupBy(s => new { s.SoruId, s.SoruMetni, s.Sira })
            .Select(g => new Soru
            {
                Id = g.Key.SoruId,
                Metin = g.Key.SoruMetni,
                Sira = g.Key.Sira,
                Secenekler = g.Where(x => x.SecenekId.HasValue)
                              .Select(x => new Secenek { Id = x.SecenekId!.Value, Metin = x.SecenekMetni! })
                              .ToList()
            })
            .ToList();
    }

    public async Task<List<SonucSatiri>> SonuclariGetirAsync(int anketId)
    {
        using var db = _baglanti.Olustur();
        var satirlar = await db.QueryAsync<SonucSatiri>(
            "SELECT * FROM dbo.vw_AnketSonuclari WHERE AnketId = @AnketId ORDER BY Sira, SoruId, SecenekId",
            new { AnketId = anketId });
        return satirlar.ToList();
    }

    public async Task<int> EkleAsync(AnketFormModel model)
    {
        using var db = _baglanti.Olustur();
        return await db.ExecuteScalarAsync<int>(
            "dbo.sp_AnketEkle",
            new { model.Ad, model.Aciklama },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> GuncelleAsync(AnketFormModel model)
    {
        using var db = _baglanti.Olustur();
        var etkilenen = await db.ExecuteScalarAsync<int>(
            "dbo.sp_AnketGuncelle",
            new { model.Id, model.Ad, model.Aciklama, model.Aktif },
            commandType: CommandType.StoredProcedure);
        return etkilenen > 0;
    }

    public async Task SoruEkleAsync(int anketId, string metin, IReadOnlyList<string> secenekler)
    {
        using var db = _baglanti.Olustur();
        db.Open();
        // Soru ve seçenekleri tek transaction içinde ekliyoruz: biri hata verirse hiçbiri kaydedilmez
        using var tx = db.BeginTransaction();

        var soruId = await db.ExecuteScalarAsync<int>(
            "dbo.sp_SoruEkle",
            new { AnketId = anketId, Metin = metin },
            transaction: tx, commandType: CommandType.StoredProcedure);

        foreach (var secenek in secenekler)
        {
            await db.ExecuteScalarAsync<int>(
                "dbo.sp_SecenekEkle",
                new { SoruId = soruId, Metin = secenek },
                transaction: tx, commandType: CommandType.StoredProcedure);
        }

        tx.Commit();
    }

    public async Task CevaplariKaydetAsync(IDictionary<int, int> soruSecenek)
    {
        using var db = _baglanti.Olustur();
        db.Open();
        using var tx = db.BeginTransaction();

        foreach (var (soruId, secenekId) in soruSecenek)
        {
            await db.ExecuteAsync(
                "dbo.sp_CevapKaydet",
                new { SoruId = soruId, SecenekId = secenekId },
                transaction: tx, commandType: CommandType.StoredProcedure);
        }

        tx.Commit();
    }
}
