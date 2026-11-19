using AnketApp.Models;
using AnketApp.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AnketApp.Controllers;

public class AnketController : Controller
{
    private readonly IAnketRepository _repo;

    // Repository, Program.cs'teki DI kaydı sayesinde constructor'a otomatik gelir
    public AnketController(IAnketRepository repo)
    {
        _repo = repo;
    }

    public async Task<IActionResult> Index()
    {
        var anketler = await _repo.TumunuGetirAsync();
        return View(anketler);
    }

    public IActionResult Ekle() => View(new AnketFormModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(AnketFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var yeniId = await _repo.EkleAsync(model);
        TempData["Mesaj"] = "Anket oluşturuldu. Şimdi soru ekleyebilirsiniz.";
        return RedirectToAction(nameof(Detay), new { id = yeniId });
    }

    public async Task<IActionResult> Duzenle(int id)
    {
        var anket = await _repo.GetirAsync(id);
        if (anket is null) return NotFound();

        return View(new AnketFormModel
        {
            Id = anket.Id, Ad = anket.Ad, Aciklama = anket.Aciklama, Aktif = anket.Aktif
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(AnketFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (!await _repo.GuncelleAsync(model)) return NotFound();

        TempData["Mesaj"] = "Anket güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detay(int id)
    {
        var model = await DetayModeliOlustur(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoruEkle([Bind(Prefix = "YeniSoru")] SoruEkleModel yeniSoru)
    {
        var secenekler = yeniSoru.SecenekListesi();
        if (secenekler.Count < 2)
            ModelState.AddModelError("YeniSoru.SeceneklerMetni", "En az iki farklı seçenek girin.");

        if (!ModelState.IsValid)
        {
            var model = await DetayModeliOlustur(yeniSoru.AnketId);
            if (model is null) return NotFound();
            model.YeniSoru = yeniSoru;
            return View(nameof(Detay), model);
        }

        await _repo.SoruEkleAsync(yeniSoru.AnketId, yeniSoru.Metin, secenekler);
        TempData["Mesaj"] = "Soru eklendi.";
        return RedirectToAction(nameof(Detay), new { id = yeniSoru.AnketId });
    }

    public async Task<IActionResult> Doldur(int id)
    {
        var model = await DetayModeliOlustur(id);
        if (model is null) return NotFound();
        if (!model.Anket.Aktif)
        {
            TempData["Hata"] = "Bu anket aktif değil.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Doldur(int id, Dictionary<int, int> cevaplar)
    {
        var model = await DetayModeliOlustur(id);
        if (model is null) return NotFound();

        // Her soru cevaplanmış olmalı
        if (model.Sorular.Any(s => !cevaplar.ContainsKey(s.Id)))
        {
            ViewData["Hata"] = "Lütfen tüm soruları cevaplayın.";
            ViewData["Secilenler"] = cevaplar;
            return View(model);
        }

        try
        {
            await _repo.CevaplariKaydetAsync(cevaplar);
        }
        catch (SqlException ex) when (ex.Number is 50001 or 50002)
        {
            // Prosedürdeki THROW ile fırlatılan iş kuralı hataları
            ViewData["Hata"] = ex.Message;
            return View(model);
        }

        TempData["Mesaj"] = "Katılımınız için teşekkürler!";
        return RedirectToAction(nameof(Sonuclar), new { id });
    }

    public async Task<IActionResult> Sonuclar(int id)
    {
        var anket = await _repo.GetirAsync(id);
        if (anket is null) return NotFound();

        return View(new AnketSonucModel
        {
            Anket = anket,
            Satirlar = await _repo.SonuclariGetirAsync(id)
        });
    }

    private async Task<AnketDetayModel?> DetayModeliOlustur(int id)
    {
        var anket = await _repo.GetirAsync(id);
        if (anket is null) return null;

        return new AnketDetayModel
        {
            Anket = anket,
            Sorular = await _repo.SorulariGetirAsync(id),
            YeniSoru = new SoruEkleModel { AnketId = id }
        };
    }
}
