using AnketApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AnketApp.Repositories;

public class AnketRepository
{
    private string _baglantiCumlesi;

    public AnketRepository(IConfiguration configuration)
    {
        _baglantiCumlesi = configuration.GetConnectionString("AnketDb");
    }

    public List<Anket> TumunuGetir(string arama)
    {
        using var db = new SqlConnection(_baglantiCumlesi);

        string sql = @"SELECT a.*,
                         (SELECT COUNT(*) FROM Sorular WHERE AnketId = a.Id) AS SoruSayisi
                       FROM Anketler a
                       WHERE a.Ad LIKE @Arama
                       ORDER BY a.Id DESC";

        return db.Query<Anket>(sql, new { Arama = "%" + arama + "%" }).ToList();
    }

    public Anket Getir(int id)
    {
        using var db = new SqlConnection(_baglantiCumlesi);
        return db.QueryFirstOrDefault<Anket>("SELECT * FROM Anketler WHERE Id = @Id", new { Id = id });
    }

    public List<Soru> SorulariGetir(int anketId)
    {
        using var db = new SqlConnection(_baglantiCumlesi);

        var sorular = db.Query<Soru>("SELECT * FROM Sorular WHERE AnketId = @AnketId", new { AnketId = anketId }).ToList();

        foreach (var soru in sorular)
        {
            string sql = @"SELECT s.*,
                             (SELECT COUNT(*) FROM Cevaplar WHERE SecenekId = s.Id) AS CevapSayisi
                           FROM Secenekler s
                           WHERE s.SoruId = @SoruId";

            soru.Secenekler = db.Query<Secenek>(sql, new { SoruId = soru.Id }).ToList();
        }

        return sorular;
    }

    public int Ekle(AnketFormModel form)
    {
        using var db = new SqlConnection(_baglantiCumlesi);
        string sql = @"INSERT INTO Anketler (Ad, Aciklama) VALUES (@Ad, @Aciklama);
                       SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return db.QuerySingle<int>(sql, form);
    }

    public void Guncelle(AnketFormModel form)
    {
        using var db = new SqlConnection(_baglantiCumlesi);
        db.Execute("UPDATE Anketler SET Ad = @Ad, Aciklama = @Aciklama, Aktif = @Aktif WHERE Id = @Id", form);
    }

    public void Sil(int id)
    {
        using var db = new SqlConnection(_baglantiCumlesi);
        db.Execute("DELETE FROM Cevaplar WHERE SecenekId IN (SELECT Id FROM Secenekler WHERE SoruId IN (SELECT Id FROM Sorular WHERE AnketId = @Id))", new { Id = id });
        db.Execute("DELETE FROM Secenekler WHERE SoruId IN (SELECT Id FROM Sorular WHERE AnketId = @Id)", new { Id = id });
        db.Execute("DELETE FROM Sorular WHERE AnketId = @Id", new { Id = id });
        db.Execute("DELETE FROM Anketler WHERE Id = @Id", new { Id = id });
    }

    public void SoruEkle(int anketId, string metin, List<string> secenekler)
    {
        using var db = new SqlConnection(_baglantiCumlesi);

        string sql = @"INSERT INTO Sorular (AnketId, Metin) VALUES (@AnketId, @Metin);
                       SELECT CAST(SCOPE_IDENTITY() AS INT);";
        int soruId = db.QuerySingle<int>(sql, new { AnketId = anketId, Metin = metin });

        foreach (var secenek in secenekler)
        {
            db.Execute("INSERT INTO Secenekler (SoruId, Metin) VALUES (@SoruId, @Metin)", new { SoruId = soruId, Metin = secenek });
        }
    }

    public void SoruSil(int soruId)
    {
        using var db = new SqlConnection(_baglantiCumlesi);
        db.Execute("DELETE FROM Cevaplar WHERE SecenekId IN (SELECT Id FROM Secenekler WHERE SoruId = @Id)", new { Id = soruId });
        db.Execute("DELETE FROM Secenekler WHERE SoruId = @Id", new { Id = soruId });
        db.Execute("DELETE FROM Sorular WHERE Id = @Id", new { Id = soruId });
    }

    public void CevaplariKaydet(Dictionary<int, int> cevaplar)
    {
        using var db = new SqlConnection(_baglantiCumlesi);

        foreach (var cevap in cevaplar)
        {
            db.Execute("INSERT INTO Cevaplar (SecenekId) VALUES (@SecenekId)", new { SecenekId = cevap.Value });
        }
    }
}
