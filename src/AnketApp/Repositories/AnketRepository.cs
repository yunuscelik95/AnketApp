using System.Data;
using AnketApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AnketApp.Repositories;

// Veritabanı işlemleri burada. SELECT'ler view'dan, INSERT/UPDATE'ler prosedürden.
public class AnketRepository
{
    private readonly string _baglantiCumlesi;

    public AnketRepository(IConfiguration configuration)
    {
        _baglantiCumlesi = configuration.GetConnectionString("AnketDb");
    }

    private SqlConnection Baglanti()
    {
        return new SqlConnection(_baglantiCumlesi);
    }

    public async Task<IEnumerable<Anket>> TumunuGetirAsync()
    {
        using var db = Baglanti();
        return await db.QueryAsync<Anket>("SELECT * FROM vw_AnketListesi ORDER BY OlusturmaTarihi DESC");
    }

    public async Task<Anket> GetirAsync(int id)
    {
        using var db = Baglanti();
        return await db.QueryFirstOrDefaultAsync<Anket>("SELECT * FROM vw_AnketListesi WHERE Id = @Id", new { Id = id });
    }

    public async Task<List<Soru>> SorulariGetirAsync(int anketId)
    {
        using var db = Baglanti();
        var satirlar = await db.QueryAsync<AnketSoruSatiri>(
            "SELECT * FROM vw_AnketSorulari WHERE AnketId = @AnketId ORDER BY Sira, SoruId, SecenekId",
            new { AnketId = anketId });

        // View her seçenek için ayrı satır döndürüyor.
        // Aynı soruya ait satırları tek soru altında topluyorum.
        var sorular = new List<Soru>();
        Soru sonSoru = null;

        foreach (var satir in satirlar)
        {
            if (sonSoru == null || sonSoru.Id != satir.SoruId)
            {
                sonSoru = new Soru();
                sonSoru.Id = satir.SoruId;
                sonSoru.Metin = satir.SoruMetni;
                sonSoru.Sira = satir.Sira;
                sorular.Add(sonSoru);
            }

            if (satir.SecenekId != null)
            {
                var secenek = new Secenek();
                secenek.Id = (int)satir.SecenekId;
                secenek.Metin = satir.SecenekMetni;
                sonSoru.Secenekler.Add(secenek);
            }
        }

        return sorular;
    }

    public async Task<List<SonucSatiri>> SonuclariGetirAsync(int anketId)
    {
        using var db = Baglanti();
        var satirlar = await db.QueryAsync<SonucSatiri>(
            "SELECT * FROM vw_AnketSonuclari WHERE AnketId = @AnketId ORDER BY Sira, SoruId, SecenekId",
            new { AnketId = anketId });
        return satirlar.ToList();
    }

    public async Task<int> EkleAsync(AnketFormModel model)
    {
        using var db = Baglanti();
        return await db.ExecuteScalarAsync<int>("sp_AnketEkle",
            new { Ad = model.Ad, Aciklama = model.Aciklama },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> GuncelleAsync(AnketFormModel model)
    {
        using var db = Baglanti();
        int etkilenen = await db.ExecuteScalarAsync<int>("sp_AnketGuncelle",
            new { Id = model.Id, Ad = model.Ad, Aciklama = model.Aciklama, Aktif = model.Aktif },
            commandType: CommandType.StoredProcedure);
        return etkilenen > 0;
    }

    public async Task SoruEkleAsync(int anketId, string metin, List<string> secenekler)
    {
        using var db = Baglanti();
        db.Open();
        // Soru ve seçenekler birlikte kaydedilsin, biri hata verirse hiçbiri kaydedilmesin
        using var tx = db.BeginTransaction();

        int soruId = await db.ExecuteScalarAsync<int>("sp_SoruEkle",
            new { AnketId = anketId, Metin = metin },
            transaction: tx, commandType: CommandType.StoredProcedure);

        foreach (var secenek in secenekler)
        {
            await db.ExecuteAsync("sp_SecenekEkle",
                new { SoruId = soruId, Metin = secenek },
                transaction: tx, commandType: CommandType.StoredProcedure);
        }

        tx.Commit();
    }

    public async Task CevaplariKaydetAsync(Dictionary<int, int> cevaplar)
    {
        using var db = Baglanti();
        db.Open();
        using var tx = db.BeginTransaction();

        // cevaplar: Key = SoruId, Value = SecenekId
        foreach (var cevap in cevaplar)
        {
            await db.ExecuteAsync("sp_CevapKaydet",
                new { SoruId = cevap.Key, SecenekId = cevap.Value },
                transaction: tx, commandType: CommandType.StoredProcedure);
        }

        tx.Commit();
    }
}
