using ap_project_final.Data;
using ap_project_final.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> CourseDetails(int courseId)
        {
            var studentId = /* retrieve logged-in student Id */;

            var course = await _context.Courses
                .Include(c => c.Classroom)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound();

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            var model = new CourseDetailsViewModel
            {
                ClassroomNumber = course.Classroom.Number,
                Building = course.Classroom.Building,
                InstructorName = course.Instructor.FirstName + " " + course.Instructor.LastName,
                ClassTime = course.ClassTime.ToString(),
                ExamTime = course.ExamTime.ToString(),
                Score = enrollment?.Grade
            };

            return View(model);
        }
    }
}