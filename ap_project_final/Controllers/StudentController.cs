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
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Profile()
        {
            // Get the ID from claim
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");

            // Convert to int, as primary key is likely int
            if (int.TryParse(userIdStr, out int userId))
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
                if (student == null)
                    return NotFound("Student not found");

                return View(student);
            }

            // If not int, handle accordingly
            return NotFound("Invalid user ID");
        }

        [Authorize]
        public async Task<IActionResult> ManageCourses()
        {
            var StudentIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            

            var enrollments = await _context.Enrollments
                .Where(c => c.Student.StudentId == StudentIdStr)
                .ToListAsync();
            var courses = enrollments.Select(e => e.Course).Distinct().ToList();

            return View(courses);
            

            
        }
    }
}