namespace AnketApp.Models;

public class Anket
{
    public int Id { get; set; }
    public string Ad { get; set; }
    public string Aciklama { get; set; }
    public bool Aktif { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public int SoruSayisi { get; set; }
}

public class Soru
{
    public int Id { get; set; }
    public string Metin { get; set; }
    public List<Secenek> Secenekler { get; set; } = new List<Secenek>();
}

public class Secenek
{
    public int Id { get; set; }
    public string Metin { get; set; }
    public int CevapSayisi { get; set; }
}
