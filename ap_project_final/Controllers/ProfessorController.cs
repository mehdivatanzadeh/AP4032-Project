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
        public async Task<IActionResult> ManageParticipants(int classroomId)
        {
            var classroom = await _context.Classrooms.FindAsync(classroomId);
            if (classroom == null)
                return NotFound();

            var students = await _context.Enrollments
                .Where(e => e.Course.Id == classroomId)
                .Include(e => e.Student)
                .Select(e => e.Student)
                .ToListAsync();

            var instructors = await _context.Professors
                .Where(i => i.Courses.Any(c => c.Id == classroomId))
                .ToListAsync();

            var model = new ParticipantListViewModel
            {
                ClassroomName = classroom.Building,
                ClassroomId = classroom.Id,
                Students = students,
                Instructors = instructors
            };
            return View(model);
        }

        [Authorize]
    public async Task<IActionResult> ManageCourses()
    {
        // Get current instructor's user ID (assume it's stored in User claims)
        // Adjust based on your authentication setup
        var userName = User.FindFirstValue(ClaimTypes.Name);
        
        // Find the instructor record based on user ID or username
        // Assuming your Instructor has a UserId property linking to auth user
        var instructor = await _context.Professors
            .FirstOrDefaultAsync(i => i.ProfessorId == userName);

        if (instructor == null)
        {
            return Unauthorized(); // or redirect appropriately
        }

        // Get courses where ProfessorId matches this instructor
        var courses = await _context.Courses
            .Where(c => c.ProfessorId == instructor.Id)
            .ToListAsync();

        return View(courses);
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
