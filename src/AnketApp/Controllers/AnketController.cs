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
    public async Task<IActionResult> Index()
    {
        var anketler = await _repo.TumunuGetirAsync();
        return View(anketler);
    }

    // Yeni anket formu
    public IActionResult Ekle()
    {
        return View(new AnketFormModel());
    }

    [HttpPost]
    public async Task<IActionResult> Ekle(AnketFormModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        int yeniId = await _repo.EkleAsync(model);
        TempData["Mesaj"] = "Anket oluşturuldu. Şimdi soru ekleyebilirsiniz.";
        return RedirectToAction("Detay", new { id = yeniId });
    }

    // Anket düzenleme formu
    public async Task<IActionResult> Duzenle(int id)
    {
        var anket = await _repo.GetirAsync(id);
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
    public async Task<IActionResult> Duzenle(AnketFormModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        bool guncellendi = await _repo.GuncelleAsync(model);
        if (!guncellendi)
            return NotFound();

        TempData["Mesaj"] = "Anket güncellendi.";
        return RedirectToAction("Index");
    }

    // Anketin soruları ve soru ekleme formu
    public async Task<IActionResult> Detay(int id)
    {
        var model = await DetayModeliGetir(id);
        if (model == null)
            return NotFound();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SoruEkle(AnketDetayModel gelen)
    {
        var yeniSoru = gelen.YeniSoru;
        var secenekler = yeniSoru.SecenekListesi();

        if (secenekler.Count < 2)
            ModelState.AddModelError("YeniSoru.SeceneklerMetni", "En az iki farklı seçenek girin.");

        if (!ModelState.IsValid)
        {
            var model = await DetayModeliGetir(yeniSoru.AnketId);
            model.YeniSoru = yeniSoru;
            return View("Detay", model);
        }

        await _repo.SoruEkleAsync(yeniSoru.AnketId, yeniSoru.Metin, secenekler);
        TempData["Mesaj"] = "Soru eklendi.";
        return RedirectToAction("Detay", new { id = yeniSoru.AnketId });
    }

    // Anket doldurma sayfası
    public async Task<IActionResult> Doldur(int id)
    {
        var model = await DetayModeliGetir(id);
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
    public async Task<IActionResult> Doldur(int id, Dictionary<int, int> cevaplar)
    {
        var model = await DetayModeliGetir(id);
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
            await _repo.CevaplariKaydetAsync(cevaplar);
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
    public async Task<IActionResult> Sonuclar(int id)
    {
        var anket = await _repo.GetirAsync(id);
        if (anket == null)
            return NotFound();

        var model = new AnketSonucModel();
        model.Anket = anket;
        model.Satirlar = await _repo.SonuclariGetirAsync(id);
        return View(model);
    }

    // Detay ve Doldur sayfaları aynı bilgileri kullandığı için ortak metot
    private async Task<AnketDetayModel> DetayModeliGetir(int id)
    {
        var anket = await _repo.GetirAsync(id);
        if (anket == null)
            return null;

        var model = new AnketDetayModel();
        model.Anket = anket;
        model.Sorular = await _repo.SorulariGetirAsync(id);
        model.YeniSoru = new SoruEkleModel();
        model.YeniSoru.AnketId = id;
        return model;
    }
}
