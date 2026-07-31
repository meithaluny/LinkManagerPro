using LinkManagerPro.Data;
using LinkManagerPro.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add DbContext
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "redirect",
    pattern: "p/{slug}",
    defaults: new { controller = "Redirect", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}
// Create database and seed default user
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    db.Database.EnsureCreated();

//    // إنشاء مستخدم افتراضي إذا لم يكن موجوداً
//    if (!db.Users.Any())
//    {
//        var defaultUser = new User
//        {
//            Id = 1,
//            Username = "Admin",
//            Email = "admin@example.com",
//            PasswordHash = "hashed_password",
//            CreatedAt = DateTime.UtcNow
//        };
//        db.Users.Add(defaultUser);
//        db.SaveChanges();
//    }
//}



app.Run();
