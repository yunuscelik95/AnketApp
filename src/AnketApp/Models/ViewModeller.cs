using System.ComponentModel.DataAnnotations;

namespace AnketApp.Models;

public class AnketFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Anket adı zorunludur.")]
    public string Ad { get; set; }

    public string Aciklama { get; set; }

    public bool Aktif { get; set; } = true;
}

public class SoruEkleModel
{
    public int AnketId { get; set; }

    [Required(ErrorMessage = "Soru metni zorunludur.")]
    public string Metin { get; set; }

    [Required(ErrorMessage = "Seçenekleri girin.")]
    public string SeceneklerMetni { get; set; }
}

public class AnketDetayModel
{
    public Anket Anket { get; set; }
    public List<Soru> Sorular { get; set; }
    public SoruEkleModel YeniSoru { get; set; } = new SoruEkleModel();
}
