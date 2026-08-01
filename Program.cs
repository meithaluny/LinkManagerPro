using LinkManagerPro.Data;
using LinkManagerPro.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides; // 1. إضافة هذه المكتبة

var builder = WebApplication.CreateBuilder(args);

// 2. إعدادات معالجة البروكسي (Render) - أضف هذا الجزء هنا
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Render يستخدم بروكسي خارجي، لذا نمسح الشبكات المعروفة ليثق به
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 3. تفعيل معالجة البروكسي - يجب أن يكون في البداية تماماً قبل أي Middleware آخر
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "redirect",
    pattern: "p/{slug}",
    defaults: new { controller = "Redirect", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();


//// إنشاء قاعدة البيانات والمستخدم الافتراضي
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    db.Database.EnsureCreated();

//    // إنشاء مستخدم افتراضي
//    if (!db.Users.Any())
//    {
//        var defaultUser = new User
//        {
//            Username = "admin",
//            Email = "admin@example.com",
//            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
//            CreatedAt = DateTime.UtcNow
//        };
//        db.Users.Add(defaultUser);
//        db.SaveChanges();
//    }
//}


