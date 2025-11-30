using System.Security.Claims;
using EthicaFlow.Data;
using EthicaFlow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EthicaFlow.Controllers
{
    [Authorize]
    public class SubmissionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SubmissionController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var submissions = _context.EthicsSubmissions
                .Where(s => s.ResearcherId == userId)
                .Include(s => s.Documents)
                .Include(s => s.Reviewer)
                .Include(s => s.ReviewDecisions)
                .OrderByDescending(s => s.CreatedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                submissions = submissions.Where(s => 
                    s.Title.Contains(searchString) || 
                    s.Description.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                submissions = submissions.Where(s => s.Status == statusFilter);
            }

            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.Statuses = new[] { "Draft", "Submitted", "Under Review", "Revision Required", "Approved", "Rejected" };

            return View(await submissions.ToListAsync());
        }

        [Authorize(Roles = "Researcher")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> Create(EthicsSubmission submission)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            submission.ResearcherId = int.Parse(userIdString);

            submission.CreatedDate = DateTime.Now;
            submission.Status = "Draft";

            if (ModelState.IsValid)
            {
                _context.Add(submission);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Submission created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(submission);
        }

        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> Edit(int id)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var submission = await _context.EthicsSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == id && s.ResearcherId == userId);

            if (submission == null)
            {
                return NotFound();
            }

            if (submission.Status != "Draft" && submission.Status != "Revision Required")
            {
                TempData["Error"] = "You can only edit submissions that are in Draft or Revision Required status.";
                return RedirectToAction(nameof(Index));
            }

            return View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> Edit(int id, EthicsSubmission submission)
        {
            if (id != submission.SubmissionId)
            {
                return NotFound();
            }

            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var existingSubmission = await _context.EthicsSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == id && s.ResearcherId == userId);

            if (existingSubmission == null)
            {
                return NotFound();
            }

            if (existingSubmission.Status != "Draft" && existingSubmission.Status != "Revision Required")
            {
                TempData["Error"] = "You can only edit submissions that are in Draft or Revision Required status.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                existingSubmission.Title = submission.Title;
                existingSubmission.Description = submission.Description;
                existingSubmission.Methodology = submission.Methodology;
                existingSubmission.Participants = submission.Participants;
                existingSubmission.Risks = submission.Risks;

                if (existingSubmission.Status == "Revision Required")
                {
                    existingSubmission.Status = "Draft";
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Submission updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(submission);
        }

        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> Details(int id)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var submission = await _context.EthicsSubmissions
                .Include(s => s.Documents)
                .Include(s => s.Reviewer)
                .Include(s => s.ReviewDecisions)
                    .ThenInclude(rd => rd.Reviewer)
                .FirstOrDefaultAsync(s => s.SubmissionId == id && s.ResearcherId == userId);

            if (submission == null)
            {
                return NotFound();
            }

            return View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var submission = await _context.EthicsSubmissions
                .Include(s => s.Documents)
                .FirstOrDefaultAsync(s => s.SubmissionId == id && s.ResearcherId == userId);

            if (submission == null)
            {
                return NotFound();
            }

            if (submission.Status != "Draft")
            {
                TempData["Error"] = "Only draft submissions can be submitted for review.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (!submission.Documents.Any())
            {
                TempData["Error"] = "Please upload at least one document before submitting.";
                return RedirectToAction(nameof(Details), new { id });
            }

            submission.Status = "Submitted";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Submission submitted for review successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> UploadDocument(int submissionId, IFormFile file)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var submission = await _context.EthicsSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId && s.ResearcherId == userId);

            if (submission == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Details), new { id = submissionId });
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", submissionId.ToString());
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new Document
            {
                SubmissionId = submissionId,
                FileName = file.FileName,
                FilePath = $"/uploads/{submissionId}/{fileName}",
                UploadDate = DateTime.Now
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Document uploaded successfully.";
            return RedirectToAction(nameof(Details), new { id = submissionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Researcher")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var document = await _context.Documents
                .Include(d => d.Submission)
                .FirstOrDefaultAsync(d => d.DocumentId == id && d.Submission.ResearcherId == userId);

            if (document == null)
            {
                return NotFound();
            }

            var submissionId = document.SubmissionId;

            var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Document deleted successfully.";
            return RedirectToAction(nameof(Details), new { id = submissionId });
        }
    }
}