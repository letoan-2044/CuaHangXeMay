using Microsoft.EntityFrameworkCore;
using WebsiteBanXeMay.Data;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

// 🔥 PIPELINE ĐÚNG THỨ TỰ
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ✅ THỨ TỰ QUAN TRỌNG!
app.UseHttpsRedirection();
app.UseStaticFiles();  // 🔥 CHỈ 1 LẦN - TRƯỚC UseRouting()
app.UseRouting();

app.UseSession();      // ✅ Session sau Routing
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "sanpham",
    pattern: "SanPham/{action=Index}/{id?}",
    defaults: new { controller = "SanPham", action = "Index" });

app.Run();