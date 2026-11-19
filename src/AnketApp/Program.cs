using AnketApp.Data;
using AnketApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Dependency Injection: bağlantı fabrikası tek örnek, repository her istekte yeni örnek
builder.Services.AddSingleton<IDbBaglantiFabrikasi, SqlBaglantiFabrikasi>();
builder.Services.AddScoped<IAnketRepository, AnketRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Anket}/{action=Index}/{id?}");

app.Run();
