using ap_project_final.Data;
using ap_project_final.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace ap_project_final.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DropCourse(int enrollmentId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
            {
                return NotFound();
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["Message"] = "You have successfully dropped the course.";
            return RedirectToAction("MyCourses");
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");
            int studentId = int.Parse(userIdStr);
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                return RedirectToAction("Login", "Account");
            return View(student);
        }

        public async Task<IActionResult> Profile()
        {
            // Here, get current logged-in student's ID
            // Assuming you use session, claim, or some auth method
            // For example, if using Identity:
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            System.Diagnostics.Debug.WriteLine($"Claim userId: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account"); // Or show error
            }

            // Find student by their id (StudentId or User ID, based on your setup)
            var student = await _context.Students
                //.Where(s => s.UserId == userId) // if you have UserId foreign key
                .FirstOrDefaultAsync(s => s.StudentId.ToString() == userId);

            if (student == null)
            {
                return NotFound("Student not found");
            }

            return View(student);
        }
    }
}