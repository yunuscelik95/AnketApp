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

    public List<Anket> TumunuGetir()
    {
        using var db = Baglanti();
        return db.Query<Anket>("SELECT * FROM vw_AnketListesi ORDER BY OlusturmaTarihi DESC").ToList();
    }

    public Anket Getir(int id)
    {
        using var db = Baglanti();
        return db.QueryFirstOrDefault<Anket>("SELECT * FROM vw_AnketListesi WHERE Id = @Id", new { Id = id });
    }

    public List<Soru> SorulariGetir(int anketId)
    {
        using var db = Baglanti();

        // Önce anketin soruları
        var sorular = db.Query<Soru>(
            "SELECT DISTINCT SoruId AS Id, SoruMetni AS Metin, Sira FROM vw_AnketSorulari WHERE AnketId = @AnketId ORDER BY Sira, Id",
            new { AnketId = anketId }).ToList();

        // Sonra her sorunun seçenekleri
        foreach (var soru in sorular)
        {
            soru.Secenekler = db.Query<Secenek>(
                "SELECT SecenekId AS Id, SecenekMetni AS Metin FROM vw_AnketSorulari WHERE SoruId = @SoruId AND SecenekId IS NOT NULL ORDER BY SecenekId",
                new { SoruId = soru.Id }).ToList();
        }

        return sorular;
    }

    public List<SonucSatiri> SonuclariGetir(int anketId)
    {
        using var db = Baglanti();
        return db.Query<SonucSatiri>(
            "SELECT * FROM vw_AnketSonuclari WHERE AnketId = @AnketId ORDER BY Sira, SoruId, SecenekId",
            new { AnketId = anketId }).ToList();
    }

    public int Ekle(AnketFormModel model)
    {
        using var db = Baglanti();
        return db.ExecuteScalar<int>("sp_AnketEkle",
            new { Ad = model.Ad, Aciklama = model.Aciklama },
            commandType: CommandType.StoredProcedure);
    }

    public bool Guncelle(AnketFormModel model)
    {
        using var db = Baglanti();
        int etkilenen = db.ExecuteScalar<int>("sp_AnketGuncelle",
            new { Id = model.Id, Ad = model.Ad, Aciklama = model.Aciklama, Aktif = model.Aktif },
            commandType: CommandType.StoredProcedure);
        return etkilenen > 0;
    }

    public void SoruEkle(int anketId, string metin, List<string> secenekler)
    {
        using var db = Baglanti();
        db.Open();
        // Soru ve seçenekler birlikte kaydedilsin, biri hata verirse hiçbiri kaydedilmesin
        using var tx = db.BeginTransaction();

        int soruId = db.ExecuteScalar<int>("sp_SoruEkle",
            new { AnketId = anketId, Metin = metin },
            transaction: tx, commandType: CommandType.StoredProcedure);

        foreach (var secenek in secenekler)
        {
            db.Execute("sp_SecenekEkle",
                new { SoruId = soruId, Metin = secenek },
                transaction: tx, commandType: CommandType.StoredProcedure);
        }

        tx.Commit();
    }

    public void CevaplariKaydet(Dictionary<int, int> cevaplar)
    {
        using var db = Baglanti();
        db.Open();
        using var tx = db.BeginTransaction();

        // cevaplar: Key = SoruId, Value = SecenekId
        foreach (var cevap in cevaplar)
        {
            db.Execute("sp_CevapKaydet",
                new { SoruId = cevap.Key, SecenekId = cevap.Value },
                transaction: tx, commandType: CommandType.StoredProcedure);
        }

        tx.Commit();
    }
}
