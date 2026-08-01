using LinkManagerPro.Data;
using LinkManagerPro.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace LinkManagerPro.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var x = BCrypt.Net.BCrypt.HashPassword("admin123").ToString();
            ViewBag.ahmad = x;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "يرجى إدخال اسم المستخدم وكلمة المرور";
                return View();
            }

            // البحث عن المستخدم
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                ViewBag.ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة";
                return View();
            }

            // التحقق من كلمة المرور
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة";
                return View();
            }

            // إنشاء Session
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Login");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                scheme: "Cookies",
                principal: new ClaimsPrincipal(claimsIdentity),
                properties: authProperties);

            return RedirectToAction("Index", "Dashboard");
        }

        


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public IActionResult ChangePassword() => View();

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "كلمة السر الجديدة غير متطابقة.");
                return View();
            }

            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                ModelState.AddModelError("", "كلمة السر القديمة غير صحيحة.");
                return View();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تغيير كلمة السر بنجاح!";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
