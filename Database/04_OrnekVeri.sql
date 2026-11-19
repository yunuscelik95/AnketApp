/* =========================================================
   AnketDb - 04: Örnek veri (prosedürler kullanılarak eklenir)
   ========================================================= */
USE AnketDb;
GO
SET NOCOUNT ON;

DECLARE @AnketId INT, @SoruId INT;
DECLARE @t TABLE (Id INT);

INSERT INTO @t EXEC dbo.sp_AnketEkle @Ad = N'Müşteri Memnuniyeti Anketi',
                                     @Aciklama = N'Hizmetlerimizi değerlendirin.';
SELECT @AnketId = Id FROM @t; DELETE FROM @t;

INSERT INTO @t EXEC dbo.sp_SoruEkle @AnketId = @AnketId, @Metin = N'Hizmetimizden ne kadar memnunsunuz?';
SELECT @SoruId = Id FROM @t; DELETE FROM @t;
INSERT INTO @t EXEC dbo.sp_SecenekEkle @SoruId, N'Çok memnunum';
INSERT INTO @t EXEC dbo.sp_SecenekEkle @SoruId, N'Memnunum';
INSERT INTO @t EXEC dbo.sp_SecenekEkle @SoruId, N'Kararsızım';
INSERT INTO @t EXEC dbo.sp_SecenekEkle @SoruId, N'Memnun değilim';
DELETE FROM @t;

INSERT INTO @t EXEC dbo.sp_SoruEkle @AnketId = @AnketId, @Metin = N'Bizi bir arkadaşınıza tavsiye eder misiniz?';
SELECT @SoruId = Id FROM @t; DELETE FROM @t;
INSERT INTO @t EXEC dbo.sp_SecenekEkle @SoruId, N'Evet';
INSERT INTO @t EXEC dbo.sp_SecenekEkle @SoruId, N'Hayır';
GO
