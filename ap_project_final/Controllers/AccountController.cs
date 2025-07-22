using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ap_project_final.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using ap_project_final.Data;

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
            if (int.TryParse(username, out int instructorId))
            {
                var instructor = await _context.Professors.FindAsync(instructorId);
                if (instructor != null && instructor.Password == password)
                {
                    await SignInUser(instructor.Id.ToString(), instructor.FirstName + " " + instructor.LastName, role);
                    return RedirectToAction("Index", "Instructor");
                }
            }
        }
        else if (role == "Student")
        {
            if (int.TryParse(username, out int studentId))
            {
                var student = await _context.Students.FindAsync(studentId);
                if (student != null && student.Password == password)
                {
                    await SignInUser(student.Id.ToString(), student.FirstName + " " + student.LastName, role);
                    return RedirectToAction("Index", "Student");
                }
            }
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