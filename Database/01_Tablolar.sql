/* =========================================================
   AnketDb - 01: Tablolar
   Bu script tabloları sıfırdan oluşturur (mevcut veriler silinir).
   ========================================================= */
IF DB_ID('AnketDb') IS NULL
    CREATE DATABASE AnketDb COLLATE Turkish_CI_AS;
GO
USE AnketDb;
GO

-- Silme sırası: önce en çok bağımlı (child) tablo
DROP TABLE IF EXISTS dbo.Cevaplar;
DROP TABLE IF EXISTS dbo.Secenekler;
DROP TABLE IF EXISTS dbo.Sorular;
DROP TABLE IF EXISTS dbo.Anketler;
GO

-- Oluşturma sırası: önce hiçbir tabloya bağlı olmayan (parent) tablo
CREATE TABLE dbo.Anketler
(
    Id              INT IDENTITY(1,1) NOT NULL,
    Ad              NVARCHAR(200)     NOT NULL,
    Aciklama        NVARCHAR(500)     NULL,
    Aktif           BIT               NOT NULL CONSTRAINT DF_Anketler_Aktif DEFAULT (1),
    OlusturmaTarihi DATETIME          NOT NULL CONSTRAINT DF_Anketler_OlusturmaTarihi DEFAULT (GETDATE()),
    CONSTRAINT PK_Anketler PRIMARY KEY (Id)
);

CREATE TABLE dbo.Sorular
(
    Id      INT IDENTITY(1,1) NOT NULL,
    AnketId INT               NOT NULL,
    Metin   NVARCHAR(255)     NOT NULL,
    Sira    INT               NOT NULL CONSTRAINT DF_Sorular_Sira DEFAULT (1),
    CONSTRAINT PK_Sorular PRIMARY KEY (Id),
    CONSTRAINT FK_Sorular_Anketler FOREIGN KEY (AnketId) REFERENCES dbo.Anketler (Id)
);

CREATE TABLE dbo.Secenekler
(
    Id     INT IDENTITY(1,1) NOT NULL,
    SoruId INT               NOT NULL,
    Metin  NVARCHAR(255)     NOT NULL,
    CONSTRAINT PK_Secenekler PRIMARY KEY (Id),
    CONSTRAINT FK_Secenekler_Sorular FOREIGN KEY (SoruId) REFERENCES dbo.Sorular (Id)
);

CREATE TABLE dbo.Cevaplar
(
    Id        INT IDENTITY(1,1) NOT NULL,
    SoruId    INT               NOT NULL,
    SecenekId INT               NOT NULL,
    Tarih     DATETIME          NOT NULL CONSTRAINT DF_Cevaplar_Tarih DEFAULT (GETDATE()),
    CONSTRAINT PK_Cevaplar PRIMARY KEY (Id),
    CONSTRAINT FK_Cevaplar_Sorular    FOREIGN KEY (SoruId)    REFERENCES dbo.Sorular (Id),
    CONSTRAINT FK_Cevaplar_Secenekler FOREIGN KEY (SecenekId) REFERENCES dbo.Secenekler (Id)
);
GO

-- Sık kullanılan JOIN kolonları için index'ler
CREATE INDEX IX_Sorular_AnketId    ON dbo.Sorular (AnketId);
CREATE INDEX IX_Secenekler_SoruId  ON dbo.Secenekler (SoruId);
CREATE INDEX IX_Cevaplar_SecenekId ON dbo.Cevaplar (SecenekId);
GO
