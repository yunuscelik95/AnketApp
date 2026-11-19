using System.ComponentModel.DataAnnotations;

namespace AnketApp.Models;

public class AnketFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Anket adı zorunludur.")]
    [StringLength(200, ErrorMessage = "Anket adı en fazla 200 karakter olabilir.")]
    [Display(Name = "Anket adı")]
    public string Ad { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string Aciklama { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;
}

public class SoruEkleModel
{
    public int AnketId { get; set; }

    [Required(ErrorMessage = "Soru metni zorunludur.")]
    [StringLength(255)]
    [Display(Name = "Soru")]
    public string Metin { get; set; }

    [Required(ErrorMessage = "En az iki seçenek girin.")]
    [Display(Name = "Seçenekler (her satıra bir tane)")]
    public string SeceneklerMetni { get; set; }

    // Textarea'daki her satır bir seçenek
    public List<string> SecenekListesi()
    {
        var liste = new List<string>();
        if (SeceneklerMetni == null)
            return liste;

        foreach (var satir in SeceneklerMetni.Split('\n'))
        {
            string secenek = satir.Trim();
            if (secenek != "" && !liste.Contains(secenek))
                liste.Add(secenek);
        }
        return liste;
    }
}

public class AnketDetayModel
{
    public Anket Anket { get; set; } = new();
    public List<Soru> Sorular { get; set; } = new();
    public SoruEkleModel YeniSoru { get; set; } = new();
}

public class AnketSonucModel
{
    public Anket Anket { get; set; } = new();
    public List<SonucSatiri> Satirlar { get; set; } = new();
}
