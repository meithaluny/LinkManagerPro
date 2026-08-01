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

            string realIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            string ua = Request.Headers["User-Agent"].ToString();

            // --- منطق تحديد المنصة ---
            string platform = "Direct/Browser";
            if (ua.Contains("FB4A") || ua.Contains("FBIOS") || ua.Contains("FB_IAB")) platform = "Facebook";
            else if (ua.Contains("Instagram")) platform = "Instagram";
            else if (ua.Contains("TikTok")) platform = "TikTok";
            else if (ua.Contains("Twitter") || ua.Contains("t.co")) platform = "Twitter/X";
            else if (ua.Contains("WhatsApp")) platform = "WhatsApp";
            else if (ua.Contains("Snapchat")) platform = "Snapchat";

            // --- منطق تحديد البلد (المطور) ---
            string country = Request.Headers["cf-ipcountry"].FirstOrDefault() ?? "Unknown";

            if (country == "Unknown" && !string.IsNullOrEmpty(realIp) && realIp != "::1")
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

            // --- تسجيل النقرة ---
            var click = new Click
            {
                LinkId = link.Id,
                ClickedAt = DateTime.UtcNow,
                UserAgent = ua,
                IpAddress = realIp,
                Country = country,
                Platform = platform // تخزين المنصة المحددة
            };

            link.ClickCount++;
            _context.Clicks.Add(click);
            await _context.SaveChangesAsync();

            // إعداد Open Graph
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
