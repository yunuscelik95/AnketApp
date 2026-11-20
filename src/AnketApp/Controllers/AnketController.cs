using AnketApp.Models;
using AnketApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AnketApp.Controllers;

public class AnketController : Controller
{
    private readonly AnketRepository _repo;

    public AnketController(AnketRepository repo)
    {
        _repo = repo;
    }

    // Anket listesi
    public IActionResult Index()
    {
        var anketler = _repo.TumunuGetir();
        return View(anketler);
    }

    // Yeni anket formu
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
        TempData["Mesaj"] = "Anket oluşturuldu. Şimdi soru ekleyebilirsiniz.";
        return RedirectToAction("Detay", new { id = yeniId });
    }

    // Anket düzenleme formu
    public IActionResult Duzenle(int id)
    {
        var anket = _repo.Getir(id);
        if (anket == null)
            return NotFound();

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

        bool guncellendi = _repo.Guncelle(form);
        if (!guncellendi)
            return NotFound();

        TempData["Mesaj"] = "Anket güncellendi.";
        return RedirectToAction("Index");
    }

    // Anketin soruları ve soru ekleme formu
    public IActionResult Detay(int id)
    {
        var detay = DetayModeliGetir(id);
        if (detay == null)
            return NotFound();

        return View(detay);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SoruEkle(AnketDetayModel gelen)
    {
        var yeniSoru = gelen.YeniSoru;
        var secenekler = yeniSoru.SecenekListesi();

        if (secenekler.Count < 2)
            ModelState.AddModelError("YeniSoru.SeceneklerMetni", "En az iki farklı seçenek girin.");

        if (!ModelState.IsValid)
        {
            var detay = DetayModeliGetir(yeniSoru.AnketId);
            detay.YeniSoru = yeniSoru;
            return View("Detay", detay);
        }

        _repo.SoruEkle(yeniSoru.AnketId, yeniSoru.Metin, secenekler);
        TempData["Mesaj"] = "Soru eklendi.";
        return RedirectToAction("Detay", new { id = yeniSoru.AnketId });
    }

    // Anket doldurma sayfası
    public IActionResult Doldur(int id)
    {
        var detay = DetayModeliGetir(id);
        if (detay == null)
            return NotFound();

        if (!detay.Anket.Aktif)
        {
            TempData["Hata"] = "Bu anket aktif değil.";
            return RedirectToAction("Index");
        }

        return View(detay);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Doldur(int id, Dictionary<int, int> cevaplar)
    {
        var detay = DetayModeliGetir(id);
        if (detay == null)
            return NotFound();

        // Her soru cevaplanmış mı?
        foreach (var soru in detay.Sorular)
        {
            if (!cevaplar.ContainsKey(soru.Id))
            {
                ViewData["Hata"] = "Lütfen tüm soruları cevaplayın.";
                ViewData["Secilenler"] = cevaplar;
                return View(detay);
            }
        }

        try
        {
            _repo.CevaplariKaydet(cevaplar);
        }
        catch (SqlException ex)
        {
            // Prosedürdeki THROW buraya düşer
            ViewData["Hata"] = ex.Message;
            return View(detay);
        }

        TempData["Mesaj"] = "Katılımınız için teşekkürler!";
        return RedirectToAction("Sonuclar", new { id = id });
    }

    // Sonuçlar sayfası
    public IActionResult Sonuclar(int id)
    {
        var anket = _repo.Getir(id);
        if (anket == null)
            return NotFound();

        var sonuc = new AnketSonucModel();
        sonuc.Anket = anket;
        sonuc.Satirlar = _repo.SonuclariGetir(id);
        return View(sonuc);
    }

    // Detay ve Doldur sayfaları aynı bilgileri kullandığı için ortak metot
    private AnketDetayModel DetayModeliGetir(int id)
    {
        var anket = _repo.Getir(id);
        if (anket == null)
            return null;

        var detay = new AnketDetayModel();
        detay.Anket = anket;
        detay.Sorular = _repo.SorulariGetir(id);
        detay.YeniSoru = new SoruEkleModel();
        detay.YeniSoru.AnketId = id;
        return detay;
    }
}
