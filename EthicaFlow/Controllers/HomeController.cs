using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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

            var existingUser = _context.Users.FirstOrDefault(u => u.Email == "researcher@uni.edu");

            if (existingUser == null)
            {
                var user = new User
                {
                    Name = "Dr. Natali Researcher",
                    Email = "researcher@uni.edu",
                    Password = "123",
                    Role = "Researcher"
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return Content("Success! User 'Dr. Natali Researcher' has been created. Password: 123");
            }

            return Content("User already exists! You can go log in.");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}