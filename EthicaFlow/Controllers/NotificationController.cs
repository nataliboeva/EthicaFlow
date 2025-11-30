using System.Security.Claims;
using EthicaFlow.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EthicaFlow.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.Submission)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        public async Task<IActionResult> GetUnreadCount()
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Json(new { count });
        }
    }
}

