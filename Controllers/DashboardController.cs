using LinkManagerPro.Data;
using LinkManagerPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkManagerPro.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var links = await _context.Links
                .Include(l => l.Clicks)
                .ToListAsync();

            ViewData["TotalLinks"] = links.Count;
            ViewData["TotalClicks"] = links.Sum(l => l.ClickCount);

            return View(links);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Link link)
        {
            // حذف التحقق من User و Slug من ModelState
            ModelState.Remove("User");
            ModelState.Remove("Slug");
            ModelState.Remove("ClickCount");
            ModelState.Remove("Clicks");
            if (ModelState.IsValid)
            {
                link.UserId = 1; // TODO: Get from current user
                link.CreatedAt = DateTime.UtcNow;
                link.UpdatedAt = DateTime.UtcNow;
                link.Slug = GenerateSlug();

                _context.Links.Add(link);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(link);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var link = await _context.Links.FindAsync(id);
            if (link == null)
            {
                return NotFound();
            }

            return View(link);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Link link)
        {
            if (id != link.Id)
                return NotFound();

            ModelState.Remove("User");
            ModelState.Remove("Slug");
            ModelState.Remove("ClickCount");
            ModelState.Remove("Clicks");

            if (ModelState.IsValid)
            {
                link.UpdatedAt = DateTime.UtcNow;
                link.CreatedAt = DateTime.SpecifyKind(
                    link.CreatedAt,DateTimeKind.Utc);

                _context.Update(link);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(link);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var link = await _context.Links.FindAsync(id);
            if (link != null)
            {
                _context.Links.Remove(link);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Analytics(int id)
        {
            var link = await _context.Links
                .Include(l => l.Clicks)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (link == null)
            {
                return NotFound();
            }

            return View(link);
        }

        private string GenerateSlug()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }

        public async Task<IActionResult> Stats(int id)
        {
            var link = await _context.Links
                .Include(l => l.Clicks)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (link == null) return NotFound();

            // إحصائيات الدول
            var countryStats = link.Clicks
                .GroupBy(c => c.Country ?? "Unknown")
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // إحصائيات المنصات
            var platformStats = link.Clicks
                .GroupBy(c => c.Platform ?? "Direct")
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.CountryStats = countryStats;
            ViewBag.PlatformStats = platformStats;

            return View(link);
        }

    }
}
