using LinkManagerPro.Data;
using LinkManagerPro.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// إضافة Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

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

// Redirect route (بدون حماية)
app.MapControllerRoute(
    name: "redirect",
    pattern: "p/{slug}",
    defaults: new { controller = "Redirect", action = "Index" });

// جميع الروابط الأخرى
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

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

app.Run();
