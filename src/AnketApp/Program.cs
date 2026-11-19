using AnketApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Controller'da AnketRepository istendiğinde .NET otomatik oluşturup verir
builder.Services.AddScoped<AnketRepository>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Anket}/{action=Index}/{id?}");

app.Run();
