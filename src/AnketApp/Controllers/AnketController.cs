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
    public IActionResult Ekle(AnketFormModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        int yeniId = _repo.Ekle(model);
        TempData["Mesaj"] = "Anket oluşturuldu. Şimdi soru ekleyebilirsiniz.";
        return RedirectToAction("Detay", new { id = yeniId });
    }

    // Anket düzenleme formu
    public IActionResult Duzenle(int id)
    {
        var anket = _repo.Getir(id);
        if (anket == null)
            return NotFound();

        var model = new AnketFormModel();
        model.Id = anket.Id;
        model.Ad = anket.Ad;
        model.Aciklama = anket.Aciklama;
        model.Aktif = anket.Aktif;
        return View(model);
    }

    [HttpPost]
    public IActionResult Duzenle(AnketFormModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        bool guncellendi = _repo.Guncelle(model);
        if (!guncellendi)
            return NotFound();

        TempData["Mesaj"] = "Anket güncellendi.";
        return RedirectToAction("Index");
    }

    // Anketin soruları ve soru ekleme formu
    public IActionResult Detay(int id)
    {
        var model = DetayModeliGetir(id);
        if (model == null)
            return NotFound();

        return View(model);
    }

    [HttpPost]
    public IActionResult SoruEkle(AnketDetayModel gelen)
    {
        var yeniSoru = gelen.YeniSoru;
        var secenekler = yeniSoru.SecenekListesi();

        if (secenekler.Count < 2)
            ModelState.AddModelError("YeniSoru.SeceneklerMetni", "En az iki farklı seçenek girin.");

        if (!ModelState.IsValid)
        {
            var model = DetayModeliGetir(yeniSoru.AnketId);
            model.YeniSoru = yeniSoru;
            return View("Detay", model);
        }

        _repo.SoruEkle(yeniSoru.AnketId, yeniSoru.Metin, secenekler);
        TempData["Mesaj"] = "Soru eklendi.";
        return RedirectToAction("Detay", new { id = yeniSoru.AnketId });
    }

    // Anket doldurma sayfası
    public IActionResult Doldur(int id)
    {
        var model = DetayModeliGetir(id);
        if (model == null)
            return NotFound();

        if (!model.Anket.Aktif)
        {
            TempData["Hata"] = "Bu anket aktif değil.";
            return RedirectToAction("Index");
        }

        return View(model);
    }

    [HttpPost]
    public IActionResult Doldur(int id, Dictionary<int, int> cevaplar)
    {
        var model = DetayModeliGetir(id);
        if (model == null)
            return NotFound();

        // Her soru cevaplanmış mı?
        foreach (var soru in model.Sorular)
        {
            if (!cevaplar.ContainsKey(soru.Id))
            {
                ViewData["Hata"] = "Lütfen tüm soruları cevaplayın.";
                ViewData["Secilenler"] = cevaplar;
                return View(model);
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
            return View(model);
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

        var model = new AnketSonucModel();
        model.Anket = anket;
        model.Satirlar = _repo.SonuclariGetir(id);
        return View(model);
    }

    // Detay ve Doldur sayfaları aynı bilgileri kullandığı için ortak metot
    private AnketDetayModel DetayModeliGetir(int id)
    {
        var anket = _repo.Getir(id);
        if (anket == null)
            return null;

        var model = new AnketDetayModel();
        model.Anket = anket;
        model.Sorular = _repo.SorulariGetir(id);
        model.YeniSoru = new SoruEkleModel();
        model.YeniSoru.AnketId = id;
        return model;
    }
}
