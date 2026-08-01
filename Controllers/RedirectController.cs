using LinkManagerPro.Data;
using LinkManagerPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            // 1. ??? ??? IP ??????? (????? ???? ???? ????? Program.cs)
            string realIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // 2. ????? ??? User Agent ?????? ??????? (??????/????????)
            string ua = Request.Headers["User-Agent"].ToString();
            string platformInfo = "";
            if (ua.Contains("FB4A") || ua.Contains("FB_IAB")) platformInfo = " [Facebook App]";
            else if (ua.Contains("Instagram")) platformInfo = " [Instagram App]";

            // 3. ????? ??????
            var click = new Click
            {
                LinkId = link.Id,
                ClickedAt = DateTime.UtcNow,
                // ????? ??? UA ?????? ????? ?????? ??? ??????? ???????
                UserAgent = ua + platformInfo,
                IpAddress = realIp
            };

            link.ClickCount++;
            _context.Clicks.Add(click);
            await _context.SaveChangesAsync();

            // ????? ?????? Open Graph
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
}
