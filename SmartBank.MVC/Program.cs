var builder = WebApplication.CreateBuilder(args);

// ─── MVC ──────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ─── HTTP Client → SmartBank API ─────────────────────────────────────────────
builder.Services.AddHttpClient("SmartBankAPI", client =>
{
    var apiBase = builder.Configuration["ApiSettings:BaseUrl"]
                  ?? "https://localhost:7201/";
    client.BaseAddress = new Uri(apiBase);
});

// ─── Session / Cookie ─────────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name:    "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
