using System.Security.Claims;
using EthicaFlow.Data;
using EthicaFlow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EthicaFlow.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var submissions = _context.EthicsSubmissions
                .Include(s => s.Researcher)
                .Include(s => s.Reviewer)
                .Include(s => s.Documents)
                .OrderByDescending(s => s.CreatedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                submissions = submissions.Where(s => 
                    s.Title.Contains(searchString) || 
                    s.Description.Contains(searchString) ||
                    s.Researcher.Name.Contains(searchString));
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

        public async Task<IActionResult> AssignReviewer(int id)
        {
            var submission = await _context.EthicsSubmissions
                .Include(s => s.Researcher)
                .Include(s => s.Reviewer)
                .FirstOrDefaultAsync(s => s.SubmissionId == id);

            if (submission == null)
            {
                return NotFound();
            }

            var reviewers = await _context.Users
                .Where(u => u.Role == "Reviewer")
                .ToListAsync();

            ViewBag.Reviewers = reviewers;
            return View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignReviewer(int submissionId, int reviewerId)
        {
            var submission = await _context.EthicsSubmissions
                .Include(s => s.Researcher)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                return NotFound();
            }

            var reviewer = await _context.Users.FindAsync(reviewerId);
            if (reviewer == null || reviewer.Role != "Reviewer")
            {
                TempData["Error"] = "Invalid reviewer selected.";
                return RedirectToAction("AssignReviewer", new { id = submissionId });
            }

            submission.ReviewerId = reviewerId;
            
            if (submission.Status == "Submitted")
            {
                submission.Status = "Under Review";
            }

            var notification = new Notification
            {
                UserId = reviewerId,
                Message = $"You have been assigned to review submission '{submission.Title}'.",
                SubmissionId = submissionId
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Reviewer assigned successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Name)
                .ToListAsync();

            return View(users);
        }
    }
}

