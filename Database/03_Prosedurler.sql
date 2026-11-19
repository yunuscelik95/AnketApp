/* =========================================================
   AnketDb - 03: Stored procedure'ler (tüm INSERT / UPDATE'ler buradan)
   ========================================================= */
USE AnketDb;
GO

CREATE OR ALTER PROCEDURE dbo.sp_AnketEkle
    @Ad       NVARCHAR(200),
    @Aciklama NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Anketler (Ad, Aciklama) VALUES (@Ad, @Aciklama);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS YeniId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_AnketGuncelle
    @Id       INT,
    @Ad       NVARCHAR(200),
    @Aciklama NVARCHAR(500) = NULL,
    @Aktif    BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Anketler
       SET Ad = @Ad, Aciklama = @Aciklama, Aktif = @Aktif
     WHERE Id = @Id;
    SELECT @@ROWCOUNT AS EtkilenenSatir;
END
GO

-- Sira verilmezse anketteki son sıranın bir fazlası kullanılır
CREATE OR ALTER PROCEDURE dbo.sp_SoruEkle
    @AnketId INT,
    @Metin   NVARCHAR(255),
    @Sira    INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Sira IS NULL
        SELECT @Sira = ISNULL(MAX(Sira), 0) + 1 FROM dbo.Sorular WHERE AnketId = @AnketId;

    INSERT INTO dbo.Sorular (AnketId, Metin, Sira) VALUES (@AnketId, @Metin, @Sira);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS YeniId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SoruGuncelle
    @Id    INT,
    @Metin NVARCHAR(255),
    @Sira  INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Sorular SET Metin = @Metin, Sira = @Sira WHERE Id = @Id;
    SELECT @@ROWCOUNT AS EtkilenenSatir;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SecenekEkle
    @SoruId INT,
    @Metin  NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Secenekler (SoruId, Metin) VALUES (@SoruId, @Metin);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS YeniId;
END
GO

-- Seçenek gerçekten o soruya mı ait, anket aktif mi? Değilse hata fırlatır.
CREATE OR ALTER PROCEDURE dbo.sp_CevapKaydet
    @SoruId    INT,
    @SecenekId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Secenekler WHERE Id = @SecenekId AND SoruId = @SoruId)
        THROW 50001, N'Seçilen seçenek bu soruya ait değil.', 1;

    IF NOT EXISTS (SELECT 1
                     FROM dbo.Sorular s
                     INNER JOIN dbo.Anketler a ON a.Id = s.AnketId
                    WHERE s.Id = @SoruId AND a.Aktif = 1)
        THROW 50002, N'Bu anket aktif değil.', 1;

    INSERT INTO dbo.Cevaplar (SoruId, SecenekId) VALUES (@SoruId, @SecenekId);
END
GO
