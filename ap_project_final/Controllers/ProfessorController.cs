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
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdateGrade(int enrollmentId, double? grade)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null) return NotFound();

            if (grade.HasValue && (grade < 0 || grade > 20))
            {
                TempData["Error"] = "Grade must be between 0 and 20.";
                return RedirectToAction("CourseDetails", new { courseId = enrollment.CourseId });
            }

            enrollment.Grade = grade;
            await _context.SaveChangesAsync();
            TempData["Message"] = "Grade updated successfully.";
            return RedirectToAction("CourseDetails", new { courseId = enrollment.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudentFromCourse(int enrollmentId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null) return NotFound();

            int courseId = enrollment.CourseId;
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Student removed from class successfully.";
            return RedirectToAction("CourseDetails", new { courseId = courseId });
        }
        public async Task<IActionResult> ManageParticipants(int courseId)
        {
            // Get current instructor ID from claims
            var instructorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(instructorIdStr) || !int.TryParse(instructorIdStr, out int instructorId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Verify that the instructor manages this course
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.ProfessorId == instructorId);
            if (course == null)
            {
                return NotFound("Course not found or you do not manage this course");
            }

            // Fetch enrollments for the course, including student info
            var enrollments = await _context.Enrollments
                .Include(e => e.Student) // include student info
                .Where(e => e.CourseId == courseId)
                .ToListAsync();

            var viewModel = new ParticipantListViewModel
            {
                
                Enrollments = enrollments
            };

            return View(viewModel);
        }

        [Authorize]
        public async Task<IActionResult> ManageCourses()
        {
            var instructorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(instructorIdStr))
            {
                return RedirectToAction("Login", "Account");
            }

            if (int.TryParse(instructorIdStr, out int instructorId))
            {
                var courses = await _context.Courses
                    .Where(c => c.ProfessorId == instructorId)
                    .ToListAsync();

                return View(courses);
            }

            return RedirectToAction("Login", "Account");
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");
            int instructorId = int.Parse(userIdStr);
            var instructor = await _context.Professors.FindAsync(instructorId);
            if (instructor == null)
                return RedirectToAction("Login", "Account");
            return View(instructor);
        }
    }
}
