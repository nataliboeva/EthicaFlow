using System.Security.Claims;
using EthicaFlow.Data;
using EthicaFlow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EthicaFlow.Controllers
{
    [Authorize(Roles = "Reviewer,Admin")]
    public class ReviewerController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewerController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var submissions = _context.EthicsSubmissions
                .Where(s => s.ReviewerId == userId || User.IsInRole("Admin"))
                .Include(s => s.Researcher)
                .Include(s => s.Reviewer)
                .Include(s => s.Documents)
                .Include(s => s.ReviewDecisions)
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

        public async Task<IActionResult> Details(int id)
        {
            var submission = await _context.EthicsSubmissions
                .Include(s => s.Researcher)
                .Include(s => s.Reviewer)
                .Include(s => s.Documents)
                .Include(s => s.ReviewDecisions)
                    .ThenInclude(rd => rd.Reviewer)
                .FirstOrDefaultAsync(s => s.SubmissionId == id);

            if (submission == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            if (!User.IsInRole("Admin") && submission.ReviewerId != userId)
            {
                return Forbid();
            }

            return View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeDecision(int submissionId, string decision, string comments)
        {
            var submission = await _context.EthicsSubmissions
                .Include(s => s.Researcher)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            if (!User.IsInRole("Admin") && submission.ReviewerId != userId)
            {
                return Forbid();
            }

            if (submission.Status != "Submitted" && submission.Status != "Under Review")
            {
                TempData["Error"] = "This submission is not in a reviewable state.";
                return RedirectToAction("Details", new { id = submissionId });
            }

            var reviewDecision = new ReviewDecision
            {
                SubmissionId = submissionId,
                ReviewerId = userId,
                Decision = decision,
                Comments = comments,
                DecisionDate = DateTime.Now
            };

            _context.ReviewDecisions.Add(reviewDecision);

            if (decision == "Approved")
            {
                submission.Status = "Approved";
                
                var notification = new Notification
                {
                    UserId = submission.ResearcherId,
                    Message = $"Your submission '{submission.Title}' has been approved.",
                    SubmissionId = submissionId
                };
                _context.Notifications.Add(notification);
            }
            else if (decision == "Revision Required")
            {
                submission.Status = "Revision Required";
                
                var notification = new Notification
                {
                    UserId = submission.ResearcherId,
                    Message = $"Your submission '{submission.Title}' requires revisions. Please review the comments and resubmit.",
                    SubmissionId = submissionId
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Decision recorded successfully.";
            return RedirectToAction("Details", new { id = submissionId });
        }
    }
}

