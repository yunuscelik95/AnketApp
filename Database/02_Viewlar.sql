/* =========================================================
   AnketDb - 02: View'lar (uygulamadaki tüm SELECT'ler buradan)
   ========================================================= */
USE AnketDb;
GO

-- Anket listesi: her anketin soru ve cevap sayısı ile
CREATE OR ALTER VIEW dbo.vw_AnketListesi
AS
SELECT
    a.Id,
    a.Ad,
    a.Aciklama,
    a.Aktif,
    a.OlusturmaTarihi,
    (SELECT COUNT(*) FROM dbo.Sorular s WHERE s.AnketId = a.Id) AS SoruSayisi,
    (SELECT COUNT(*)
       FROM dbo.Cevaplar c
       INNER JOIN dbo.Sorular s ON s.Id = c.SoruId
      WHERE s.AnketId = a.Id) AS CevapSayisi
FROM dbo.Anketler a;
GO

-- Bir anketin soruları ve seçenekleri (detay / anket doldurma ekranı)
CREATE OR ALTER VIEW dbo.vw_AnketSorulari
AS
SELECT
    s.AnketId,
    s.Id     AS SoruId,
    s.Metin  AS SoruMetni,
    s.Sira,
    se.Id    AS SecenekId,
    se.Metin AS SecenekMetni
FROM dbo.Sorular s
LEFT JOIN dbo.Secenekler se ON se.SoruId = s.Id;
GO

-- Sonuçlar: her seçeneğin kaç kez seçildiği (hiç seçilmeyenler 0 gelir)
CREATE OR ALTER VIEW dbo.vw_AnketSonuclari
AS
SELECT
    s.AnketId,
    s.Id     AS SoruId,
    s.Metin  AS SoruMetni,
    s.Sira,
    se.Id    AS SecenekId,
    se.Metin AS SecenekMetni,
    COUNT(c.Id) AS CevapSayisi
FROM dbo.Sorular s
INNER JOIN dbo.Secenekler se ON se.SoruId = s.Id
LEFT  JOIN dbo.Cevaplar   c  ON c.SecenekId = se.Id
GROUP BY s.AnketId, s.Id, s.Metin, s.Sira, se.Id, se.Metin;
GO
