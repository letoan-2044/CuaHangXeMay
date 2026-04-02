using Microsoft.EntityFrameworkCore;
using WebsiteBanXeMay.Data;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 🔥 SERVICES
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

// 🔥 AUTHENTICATION
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options => {
        options.LoginPath = "/TaiKhoan/DangNhap";
        options.AccessDeniedPath = "/Home/TruyCapBiChan";
    });

var app = builder.Build();

// 🔥 PIPELINE - THỨ TỰ CHÍNH XÁC ⭐
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();        // 1. Session
app.UseAuthentication(); // 2. Authentication ⭐ SAU Session, TRƯỚC Authorization ⭐
app.UseAuthorization();  // 3. Authorization

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "sanpham",
    pattern: "SanPham/{action=Index}/{id?}",
    defaults: new { controller = "SanPham", action = "Index" });

app.Run();