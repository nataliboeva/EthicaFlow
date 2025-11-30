using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EthicaFlow.Models;
using EthicaFlow.Data;

namespace EthicaFlow.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (User.IsInRole("Reviewer"))
                {
                    return RedirectToAction("Index", "Reviewer");
                }
                else if (User.IsInRole("Researcher"))
                {
                    return RedirectToAction("Index", "Submission");
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SeedUser()
        {
            var users = new List<User>
            {
                new User
                {
                    Name = "Dr. Natali Researcher",
                    Email = "researcher@uni.edu",
                    Password = "123",
                    Role = "Researcher"
                },
                new User
                {
                    Name = "Dr. John Reviewer",
                    Email = "reviewer@uni.edu",
                    Password = "123",
                    Role = "Reviewer"
                },
                new User
                {
                    Name = "Admin User",
                    Email = "admin@uni.edu",
                    Password = "123",
                    Role = "Admin"
                }
            };

            var createdUsers = new List<string>();

            foreach (var user in users)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existingUser == null)
                {
                    _context.Users.Add(user);
                    createdUsers.Add($"{user.Name} ({user.Email}) - Password: 123");
                }
            }

            _context.SaveChanges();

            if (createdUsers.Any())
            {
                return Content($"Success! Created users:\n{string.Join("\n", createdUsers)}\n\nYou can now log in with any of these accounts.");
            }

            return Content("All users already exist! You can go log in.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}