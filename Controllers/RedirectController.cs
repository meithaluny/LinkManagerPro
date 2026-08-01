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

            if (link == null)
            {
                return NotFound();
            }

            // 1. جلب الـ IP الحقيقي (سيعمل بفضل تعديل Program.cs السابق)
            string? realIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // 2. تحليل الـ User Agent لمعرفة التطبيق
            string ua = Request.Headers["User-Agent"].ToString();
            string platformInfo = "";
            if (ua.Contains("FB4A") || ua.Contains("FB_IAB")) platformInfo = " [Facebook App]";
            else if (ua.Contains("Instagram")) platformInfo = " [Instagram App]";

            // 3. جلب اسم البلد
            // نحاول أولاً جلب البلد من Cloudflare/Render إذا كان متوفراً (سريع جداً)
            string country = Request.Headers["cf-ipcountry"].FirstOrDefault()
                             ?? Request.Headers["X-Vercel-IP-Country"].FirstOrDefault()
                             ?? "Unknown";

            // إذا لم نجد البلد وكان الـ IP حقيقياً، نستخدم API خارجي بسيط
            if (country == "Unknown" && !string.IsNullOrEmpty(realIp) && realIp != "::1" && realIp != "127.0.0.1")
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(2); // مهلة قصيرة لضمان عدم تأخير الزائر
                        var result = await client.GetFromJsonAsync<IpInfo>($"http://ip-api.com/json/{realIp}");
                        if (result != null && result.status == "success")
                        {
                            country = result.country;
                        }
                    }
                }
                catch
                {
                    // في حال فشل الـ API، نترك البلد "Unknown" لكي لا يتوقف الموقع عن العمل
                }
            }

            // 4. تسجيل النقرة مع البيانات الجديدة
            var click = new Click
            {
                LinkId = link.Id,
                ClickedAt = DateTime.UtcNow,
                UserAgent = ua + platformInfo,
                IpAddress = realIp,
                Country = country // القيمة الجديدة التي أضفتها أنت
            };

            link.ClickCount++;
            _context.Clicks.Add(click);
            await _context.SaveChangesAsync();

            // إعداد علامات Open Graph
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
