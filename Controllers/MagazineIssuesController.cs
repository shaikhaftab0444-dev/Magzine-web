using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MagazineIssuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MagazineIssuesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: MagazineIssues
        public async Task<IActionResult> Index()
        {
            var issues = await _context.MagazineIssues
                .Include(m => m.Magazine)
                .Include(m => m.Category)
                .Include(m => m.Articles)
                .OrderByDescending(m => m.PublishDate)
                .ToListAsync();

            return View(issues);
        }

        // GET: MagazineIssues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var magazineIssue = await _context.MagazineIssues
                .Include(m => m.Magazine)
                .Include(m => m.Category)
                .Include(m => m.Articles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (magazineIssue == null)
            {
                return NotFound();
            }

            return View(magazineIssue);
        }

        // GET: MagazineIssues/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();

            var defaultIssue = new MagazineIssue
            {
                PublishDate = DateTime.Today,
                SingleIssuePrice = 99.00m,
                Volume = 1
            };
            return View(defaultIssue);
        }

        // POST: MagazineIssues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Volume,PublishDate,CoverImageUrl,CoverImagePath,SingleIssuePrice,IsPublished,MagazineId,CategoryId,Status")] MagazineIssue magazineIssue,
            IFormFile? coverFile,
            IFormFile? pdfFile,
            string? isFreeToggle)
        {
            if (ModelState.IsValid)
            {
                // Handle Cover Image file upload
                if (coverFile != null && coverFile.Length > 0)
                {
                    var coversDir = Path.Combine(_env.WebRootPath, "images", "covers");
                    if (!Directory.Exists(coversDir))
                    {
                        Directory.CreateDirectory(coversDir);
                    }

                    var uniqueCoverName = $"{Guid.NewGuid()}_{Path.GetFileName(coverFile.FileName)}";
                    var coverPath = Path.Combine(coversDir, uniqueCoverName);

                    using (var fileStream = new FileStream(coverPath, FileMode.Create))
                    {
                        await coverFile.CopyToAsync(fileStream);
                    }

                    magazineIssue.CoverImagePath = $"/images/covers/{uniqueCoverName}";
                    magazineIssue.CoverImageUrl = magazineIssue.CoverImagePath;
                }

                // Handle PDF file upload
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "pdfs");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(pdfFile.FileName)}";
                    var filePath = Path.Combine(uploadsDir, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(fileStream);
                    }

                    magazineIssue.PdfFilePath = $"/pdfs/{uniqueFileName}";
                }

                // Handle Free Pricing toggle
                if (isFreeToggle is "on" or "true" || magazineIssue.SingleIssuePrice <= 0)
                {
                    magazineIssue.SingleIssuePrice = 0;
                }

                // Maintain publication status
                magazineIssue.Status = magazineIssue.IsPublished ? "Published" : "Draft";

                // If CategoryId was not specified, inherit from parent Magazine
                if (!magazineIssue.CategoryId.HasValue && magazineIssue.MagazineId > 0)
                {
                    var parentMag = await _context.Magazines.FindAsync(magazineIssue.MagazineId);
                    if (parentMag != null)
                    {
                        magazineIssue.CategoryId = parentMag.CategoryId;
                    }
                }

                _context.Add(magazineIssue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Magazine Issue '{magazineIssue.Title}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(magazineIssue.MagazineId, magazineIssue.CategoryId);
            return View(magazineIssue);
        }

        // GET: MagazineIssues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var magazineIssue = await _context.MagazineIssues.FindAsync(id);
            if (magazineIssue == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync(magazineIssue.MagazineId, magazineIssue.CategoryId);
            return View(magazineIssue);
        }

        // POST: MagazineIssues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Title,Volume,PublishDate,CoverImageUrl,CoverImagePath,SingleIssuePrice,IsPublished,MagazineId,CategoryId,PdfFilePath,Status")] MagazineIssue magazineIssue,
            IFormFile? coverFile,
            IFormFile? pdfFile,
            string? isFreeToggle)
        {
            if (id != magazineIssue.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingIssue = await _context.MagazineIssues.FindAsync(id);
                    if (existingIssue == null)
                    {
                        return NotFound();
                    }

                    // Handle Cover Image file upload
                    if (coverFile != null && coverFile.Length > 0)
                    {
                        var coversDir = Path.Combine(_env.WebRootPath, "images", "covers");
                        if (!Directory.Exists(coversDir))
                        {
                            Directory.CreateDirectory(coversDir);
                        }

                        var uniqueCoverName = $"{Guid.NewGuid()}_{Path.GetFileName(coverFile.FileName)}";
                        var coverPath = Path.Combine(coversDir, uniqueCoverName);

                        using (var fileStream = new FileStream(coverPath, FileMode.Create))
                        {
                            await coverFile.CopyToAsync(fileStream);
                        }

                        existingIssue.CoverImagePath = $"/images/covers/{uniqueCoverName}";
                        existingIssue.CoverImageUrl = existingIssue.CoverImagePath;
                    }

                    // Handle new PDF upload if provided
                    if (pdfFile != null && pdfFile.Length > 0)
                    {
                        var uploadsDir = Path.Combine(_env.WebRootPath, "pdfs");
                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(pdfFile.FileName)}";
                        var filePath = Path.Combine(uploadsDir, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await pdfFile.CopyToAsync(fileStream);
                        }

                        existingIssue.PdfFilePath = $"/pdfs/{uniqueFileName}";
                    }

                    // Handle Free Pricing toggle
                    if (isFreeToggle is "on" or "true" || magazineIssue.SingleIssuePrice <= 0)
                    {
                        existingIssue.SingleIssuePrice = 0;
                    }
                    else
                    {
                        existingIssue.SingleIssuePrice = magazineIssue.SingleIssuePrice;
                    }

                    existingIssue.Title = magazineIssue.Title;
                    existingIssue.Volume = magazineIssue.Volume;
                    existingIssue.PublishDate = magazineIssue.PublishDate;
                    existingIssue.IsPublished = magazineIssue.IsPublished;
                    existingIssue.Status = magazineIssue.IsPublished ? "Published" : "Draft";
                    existingIssue.MagazineId = magazineIssue.MagazineId;
                    existingIssue.CategoryId = magazineIssue.CategoryId;

                    _context.Update(existingIssue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Magazine Issue '{existingIssue.Title}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MagazineIssueExists(magazineIssue.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(magazineIssue.MagazineId, magazineIssue.CategoryId);
            return View(magazineIssue);
        }

        // GET: MagazineIssues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var magazineIssue = await _context.MagazineIssues
                .Include(m => m.Magazine)
                .Include(m => m.Category)
                .Include(m => m.Articles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (magazineIssue == null)
            {
                return NotFound();
            }

            return View(magazineIssue);
        }

        // POST: MagazineIssues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var magazineIssue = await _context.MagazineIssues.FindAsync(id);
            if (magazineIssue != null)
            {
                _context.MagazineIssues.Remove(magazineIssue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Magazine Issue deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(int? selectedMagId = null, int? selectedCatId = null)
        {
            var magazines = await _context.Magazines.OrderBy(m => m.Title).ToListAsync();
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            ViewBag.Magazines = new SelectList(magazines, "Id", "Title", selectedMagId);
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCatId);
        }

        private bool MagazineIssueExists(int id)
        {
            return _context.MagazineIssues.Any(e => e.Id == id);
        }
    }
}
