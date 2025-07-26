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
                ViewBag.Error = "Grade must be between 0 and 20.";
                return RedirectToAction("CourseDetails", new { courseId = enrollment.CourseId });
            }

            enrollment.Grade = grade;
            await _context.SaveChangesAsync();
            TempData["Message"] = "Grade updated successfully.";
            return RedirectToAction("CourseDetails", new { courseId = enrollment.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveStudentFromCourse(int courseId, string studentId)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.CourseId == courseId && e.Student.StudentId == studentId);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageParticipants", new { id = courseId });
        }
        /*public async Task<IActionResult> ManageParticipants(int courseId)
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
        }*/

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
        public IActionResult ManageParticipants(int id)
        {
            var course = _context.Courses.Include(c => c.Professor)
                .Include(c => c.Enrollments).ThenInclude(c => c.Student)
                .FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost]
        public IActionResult UpdateStudentGrade(int enrollmentId, int grade)
        {
            var enrollment = _context.Enrollments.Find(enrollmentId);
            if (enrollment == null)
                return NotFound();
            if(grade<0 || grade > 20)
            {
                TempData["Error"] = "Grade must be between 0 and 20.";
                return RedirectToAction("ManageParticipants", new { id = enrollment.CourseId });
            }
            enrollment.Grade = grade;
            _context.SaveChanges();

            // Redirect back to the same page, or adjust as needed
            return RedirectToAction("ManageParticipants", new { id = enrollment.CourseId });
        }
        // GET: برای نمایش صندوق پیام‌ها
        [HttpPost]
        public async Task<IActionResult> Inbox()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int professorId = int.Parse(userIdStr);

            var messages = await _context.Messages
                .Where(m => m.ReceiverProfessorId == professorId)
                .Include(m => m.SenderStudent)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
            return View(messages);
        }
        // GET: برای نمایش لیست اعتراضات
        public async Task<IActionResult> Appeals()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int professorId = int.Parse(userIdStr);

            var appeals = await _context.GradeAppeals
                .Where(a => a.Enrollment.Course.ProfessorId == professorId && a.Status == AppealStatus.Pending)
                .Include(a => a.Enrollment.Student)
                .Include(a => a.Enrollment.Course)
                .ToListAsync();
            return View(appeals);
        }

        // POST: برای پاسخ به اعتراض
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondToAppeal(int appealId, string professorResponse)
        {
            var appeal = await _context.GradeAppeals.FindAsync(appealId);
            if (appeal == null) return NotFound();

            appeal.ProfessorResponse = professorResponse;
            appeal.Status = AppealStatus.Reviewed;
            _context.Update(appeal);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Response submitted.";
            return RedirectToAction(nameof(Appeals));
        }

    }
}
