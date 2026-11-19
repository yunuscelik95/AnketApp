namespace AnketApp.Models;

// vw_AnketListesi satırı (Dapper kolon adlarını property adlarıyla eşler)
public class Anket
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public bool Aktif { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public int SoruSayisi { get; set; }
    public int CevapSayisi { get; set; }
}

// vw_AnketSorulari satırı: her satır bir soru + bir seçenek
public class AnketSoruSatiri
{
    public int AnketId { get; set; }
    public int SoruId { get; set; }
    public string SoruMetni { get; set; } = "";
    public int Sira { get; set; }
    public int? SecenekId { get; set; }
    public string? SecenekMetni { get; set; }
}

// Düz satırlar C# tarafında soru -> seçenekler şeklinde gruplanır
public class Soru
{
    public int Id { get; set; }
    public string Metin { get; set; } = "";
    public int Sira { get; set; }
    public List<Secenek> Secenekler { get; set; } = new();
}

public class Secenek
{
    public int Id { get; set; }
    public string Metin { get; set; } = "";
}

// vw_AnketSonuclari satırı
public class SonucSatiri
{
    public int SoruId { get; set; }
    public string SoruMetni { get; set; } = "";
    public int Sira { get; set; }
    public int SecenekId { get; set; }
    public string SecenekMetni { get; set; } = "";
    public int CevapSayisi { get; set; }
}
