CREATE DATABASE AnketDb;
GO
USE AnketDb;
GO

CREATE TABLE Anketler (
    Id              INT IDENTITY PRIMARY KEY,
    Ad              NVARCHAR(200) NOT NULL,
    Aciklama        NVARCHAR(500) NULL,
    Aktif           BIT NOT NULL DEFAULT 1,
    OlusturmaTarihi DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Sorular (
    Id      INT IDENTITY PRIMARY KEY,
    AnketId INT NOT NULL,
    Metin   NVARCHAR(255) NOT NULL
);

CREATE TABLE Secenekler (
    Id     INT IDENTITY PRIMARY KEY,
    SoruId INT NOT NULL,
    Metin  NVARCHAR(255) NOT NULL
);

CREATE TABLE Cevaplar (
    Id        INT IDENTITY PRIMARY KEY,
    SecenekId INT NOT NULL
);
GO

INSERT INTO Anketler (Ad, Aciklama) VALUES (N'Müşteri Memnuniyeti Anketi', N'Hizmetlerimizi değerlendirin.');

INSERT INTO Sorular (AnketId, Metin) VALUES (1, N'Hizmetimizden ne kadar memnunsunuz?');
INSERT INTO Secenekler (SoruId, Metin) VALUES (1, N'Çok memnunum');
INSERT INTO Secenekler (SoruId, Metin) VALUES (1, N'Memnunum');
INSERT INTO Secenekler (SoruId, Metin) VALUES (1, N'Kararsızım');
INSERT INTO Secenekler (SoruId, Metin) VALUES (1, N'Memnun değilim');

INSERT INTO Sorular (AnketId, Metin) VALUES (1, N'Bizi bir arkadaşınıza tavsiye eder misiniz?');
INSERT INTO Secenekler (SoruId, Metin) VALUES (2, N'Evet');
INSERT INTO Secenekler (SoruId, Metin) VALUES (2, N'Hayır');
GO
