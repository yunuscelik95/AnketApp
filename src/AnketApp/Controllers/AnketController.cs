using AnketApp.Models;
using AnketApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AnketApp.Controllers;

public class AnketController : Controller
{
    private AnketRepository _repo;

    public AnketController(AnketRepository repo)
    {
        _repo = repo;
    }

    public IActionResult Index(string arama)
    {
        ViewData["Arama"] = arama;
        var anketler = _repo.TumunuGetir(arama);
        return View(anketler);
    }

    public IActionResult Ekle()
    {
        return View(new AnketFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Ekle(AnketFormModel form)
    {
        if (!ModelState.IsValid)
            return View(form);

        int yeniId = _repo.Ekle(form);
        TempData["Mesaj"] = "Anket oluşturuldu.";
        return RedirectToAction("Detay", new { id = yeniId });
    }

    public IActionResult Duzenle(int id)
    {
        var anket = _repo.Getir(id);

        var form = new AnketFormModel();
        form.Id = anket.Id;
        form.Ad = anket.Ad;
        form.Aciklama = anket.Aciklama;
        form.Aktif = anket.Aktif;
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Duzenle(AnketFormModel form)
    {
        if (!ModelState.IsValid)
            return View(form);

        _repo.Guncelle(form);
        TempData["Mesaj"] = "Anket güncellendi.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Sil(int id)
    {
        _repo.Sil(id);
        TempData["Mesaj"] = "Anket silindi.";
        return RedirectToAction("Index");
    }

    public IActionResult Detay(int id)
    {
        var detay = new AnketDetayModel();
        detay.Anket = _repo.Getir(id);
        detay.Sorular = _repo.SorulariGetir(id);
        detay.YeniSoru.AnketId = id;
        return View(detay);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SoruEkle([Bind(Prefix = "YeniSoru")] SoruEkleModel yeniSoru)
    {
        if (!ModelState.IsValid)
        {
            TempData["Hata"] = "Soru ve seçenekleri girin.";
            return RedirectToAction("Detay", new { id = yeniSoru.AnketId });
        }

        var secenekler = new List<string>();
        foreach (var satir in yeniSoru.SeceneklerMetni.Split('\n'))
        {
            if (satir.Trim() != "")
                secenekler.Add(satir.Trim());
        }

        _repo.SoruEkle(yeniSoru.AnketId, yeniSoru.Metin, secenekler);
        TempData["Mesaj"] = "Soru eklendi.";
        return RedirectToAction("Detay", new { id = yeniSoru.AnketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SoruSil(int id, int anketId)
    {
        _repo.SoruSil(id);
        TempData["Mesaj"] = "Soru silindi.";
        return RedirectToAction("Detay", new { id = anketId });
    }

    public IActionResult Doldur(int id)
    {
        var detay = new AnketDetayModel();
        detay.Anket = _repo.Getir(id);
        detay.Sorular = _repo.SorulariGetir(id);
        return View(detay);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Doldur(int id, Dictionary<int, int> cevaplar)
    {
        var sorular = _repo.SorulariGetir(id);

        if (cevaplar.Count < sorular.Count)
        {
            TempData["Hata"] = "Lütfen tüm soruları cevaplayın.";
            return RedirectToAction("Doldur", new { id = id });
        }

        _repo.CevaplariKaydet(cevaplar);
        TempData["Mesaj"] = "Katılımınız için teşekkürler!";
        return RedirectToAction("Sonuclar", new { id = id });
    }

    public IActionResult Sonuclar(int id)
    {
        var detay = new AnketDetayModel();
        detay.Anket = _repo.Getir(id);
        detay.Sorular = _repo.SorulariGetir(id);
        return View(detay);
    }
}
