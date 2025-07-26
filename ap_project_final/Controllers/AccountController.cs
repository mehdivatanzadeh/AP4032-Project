using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ap_project_final.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using ap_project_final.Data;
using Microsoft.EntityFrameworkCore;

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
            ViewBag.Error = "Please fill all fields.";
            return View();
        }

        if (role == "Admin")
        {
            // Hardcoded admin credentials
            if (username == "admin" && password == "adminpass")
            {
                await SignInUser("0", "Admin", role);
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                ViewBag.Error = "Invalid admin credentials.";
                return View();
            }
        }
        else if (role == "Instructor")
        {
           
                var Professor = await _context.Professors.FirstOrDefaultAsync(i => i.ProfessorId ==username);
            if (Professor != null && Professor.Password == password)
            {
                await SignInUser(Professor.Id.ToString(), Professor.FirstName + " " + Professor.LastName, role);
                return RedirectToAction("Index", "Instructor");
            }
            ViewBag.Error = "Invalid credentials.";
            return View();

        }
        else if (role == "Student")
        {
            
                var student = await _context.Students.FirstOrDefaultAsync(i => i.StudentId == username);
            if (student != null && student.Password == password)
            {
                await SignInUser(student.Id.ToString(), student.FirstName + " " + student.LastName, role);
                return RedirectToAction("Index", "Student");

            }
            ViewBag.Error = "Invalid credentials.";
            return View();
        }

        // If here, login failed
        ViewBag.Error = "Invalid credentials.";
        return View();
    }

    private async Task SignInUser(string userId, string userName, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
        await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity));
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("MyCookieAuth");
        return RedirectToAction("Login");
    }
}