using System.Data;
using Microsoft.Data.SqlClient;

namespace AnketApp.Data;

public interface IDbBaglantiFabrikasi
{
    IDbConnection Olustur();
}

/// <summary>appsettings.json içindeki "AnketDb" bağlantı cümlesiyle SQL Server bağlantısı üretir.</summary>
public class SqlBaglantiFabrikasi : IDbBaglantiFabrikasi
{
    private readonly string _baglantiCumlesi;

    public SqlBaglantiFabrikasi(IConfiguration configuration)
    {
        _baglantiCumlesi = configuration.GetConnectionString("AnketDb")
            ?? throw new InvalidOperationException("'AnketDb' bağlantı cümlesi bulunamadı.");
    }

    public IDbConnection Olustur() => new SqlConnection(_baglantiCumlesi);
}
