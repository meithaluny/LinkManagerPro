using LinkManagerPro.Data;
using LinkManagerPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace LinkManagerPro.Controllers
{
    public class RedirectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RedirectController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string slug)
        {
            var link = await _context.Links
                .Include(l => l.Clicks)
                .FirstOrDefaultAsync(l => l.Slug == slug);

            if (link == null) return NotFound();

            string ua = Request.Headers["User-Agent"].ToString();

            // 1. فلترة الروبوتات (Bots) - لا نحتسب نقرة إذا كان الزائر روبوت
            bool isBot = ua.Contains("facebookexternalhit") || ua.Contains("Googlebot") || ua.Contains("Twitterbot");

            // 2. جلب الـ IP الحقيقي من رأس X-Forwarded-For مباشرة (أكثر دقة على Render)
            string realIp = Request.Headers["X-Forwarded-For"].FirstOrDefault()?? HttpContext.Connection.RemoteIpAddress?.ToString();

            // 3. تحديد المنصة
            string platform = "Direct/Browser";
            if (ua.Contains("FB4A") || ua.Contains("FBIOS") || ua.Contains("FB_IAB")) platform = "Facebook";
            else if (ua.Contains("Instagram")) platform = "Instagram";
            else if (isBot) platform = "Facebook Bot (Preview)"; // تمييز الروبوت

            // 4. تحديد البلد (إجبار الاسم الكامل بدلاً من الرمز)
            string country = "Unknown";
            // إذا أردت الاسم الكامل دائماً، استدعِ الـ API مباشرة وتجاهل رأس cf-ipcountry
            if (!string.IsNullOrEmpty(realIp) && realIp != "::1" && !realIp.StartsWith("10."))
            {
                try
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(2);
                    var result = await client.GetFromJsonAsync<IpInfo>($"http://ip-api.com/json/{realIp}");
                    if (result != null && result.status == "success") country = result.country;
                }
                catch { }
            }

            // إذا فشل الـ API، نستخدم الرمز القادم من Render كحل أخير
            if (country == "Unknown")
            {
                country = Request.Headers["cf-ipcountry"].FirstOrDefault() ?? "Unknown";
            }

            // 5. تسجيل النقرة فقط إذا لم يكن روبوتاً (أو سجله مع تمييزه)
            if (!isBot)
            {
                var click = new Click
                {
                    LinkId = link.Id,
                    ClickedAt = DateTime.UtcNow,
                    UserAgent = ua,
                    IpAddress = realIp,
                    Country = country,
                    Platform = platform
                };
                link.ClickCount++;
                _context.Clicks.Add(click);
                await _context.SaveChangesAsync();
            }

            // إعداد Open Graph (سيظل الروبوت يرى هذه البيانات ليظهر المعاينة)
            ViewData["Title"] = link.Title;
            ViewData["Description"] = link.Description;
            ViewData["ImageUrl"] = link.ImageUrl;
            ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/p/{link.Slug}";

            return View(link);
        }




        public async Task<IActionResult> Index2(string slug)
        {
            var link = await _context.Links
                .Include(l => l.Clicks)
                .FirstOrDefaultAsync(l => l.Slug == slug);

            if (link == null)
            {
                return NotFound();
            }

            // Record click
            var click = new Click
            {
                LinkId = link.Id,
                ClickedAt = DateTime.UtcNow,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            link.ClickCount++;
            _context.Clicks.Add(click);
            await _context.SaveChangesAsync();

            // Set Open Graph meta tags
            ViewData["Title"] = link.Title;
            ViewData["Description"] = link.Description;
            ViewData["ImageUrl"] = link.ImageUrl;
            ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/p/{link.Slug}";

            return View(link);
        }
    }

    public class IpInfo
    {
        public string? status { get; set; }
        public string? country { get; set; }
    }
}
