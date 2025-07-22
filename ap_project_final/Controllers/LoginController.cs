using ap_project_final.Data;
using ap_project_final.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace ap_project_final.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            // For Admin: hardcoded check
            if (role == "Admin")
            {
                if (username == "admin" && password == "adminpass")
                {
                    HttpContext.Session.SetString("UserRole", "Admin");
                    return RedirectToAction("Index", "Admin");
                }
            }
            else if (role == "Instructor")
            {
                // Match professor by ID and password
                if (int.TryParse(username, out int profId))
                {
                    var instructor = await _context.Professors.FindAsync(profId);
                    if (instructor != null && instructor.Password == password)
                    {
                        HttpContext.Session.SetString("UserRole", "Instructor");
                        HttpContext.Session.SetInt32("UserId", instructor.Id);
                        return RedirectToAction("Index", "Instructor");
                    }
                }
            }
            else if (role == "Student")
            {
                // Match student by ID and password
                if (int.TryParse(username, out int studentId))
                {
                    var student = await _context.Students.FindAsync(studentId);
                    if (student != null && student.Password == password)
                    {
                        HttpContext.Session.SetString("UserRole", "Student");
                        HttpContext.Session.SetInt32("UserId", student.Id);
                        return RedirectToAction("Index", "Student");
                    }
                }
            }

            ViewBag.Error = "Invalid credentials or role.";
            return View();
        }


    }
}
