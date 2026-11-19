using AnketApp.Models;

namespace AnketApp.Repositories;

public interface IAnketRepository
{
    // SELECT'ler -> view'lar
    Task<IEnumerable<Anket>> TumunuGetirAsync();
    Task<Anket?> GetirAsync(int id);
    Task<List<Soru>> SorulariGetirAsync(int anketId);
    Task<List<SonucSatiri>> SonuclariGetirAsync(int anketId);

    // INSERT / UPDATE'ler -> stored procedure'ler
    Task<int> EkleAsync(AnketFormModel model);
    Task<bool> GuncelleAsync(AnketFormModel model);
    Task SoruEkleAsync(int anketId, string metin, IReadOnlyList<string> secenekler);
    Task CevaplariKaydetAsync(IDictionary<int, int> soruSecenek);
}
