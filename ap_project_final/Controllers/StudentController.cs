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
        public async Task<IActionResult> DropCourse(int CourseId)
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Course.Id==CourseId);
            if (enrollment == null)
            {
                return NotFound();
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["Message"] = "You have successfully dropped the course.";
            return RedirectToAction("ManageCourses");
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

        public async Task<IActionResult> ManageCourses()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Account");
            }
            int loggedInStudentId = int.Parse(userIdStr);

            // به جای لیست دروس، لیست ثبت‌نامی‌ها را می‌خوانیم که شامل تمام اطلاعات است
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == loggedInStudentId)
                .Include(e => e.Course)
                .ThenInclude(c => c.Professor)
                .ToListAsync();

            return View(enrollments);
        }

        public async Task<IActionResult> CourseDetails(int id) // id is course ID
        {
            // Get student's user ID from claims
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
            {
                return NotFound(); // or redirect to login
            }

            // Find the student
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
            if (student == null)
            {
                return NotFound();
            }

            // Fetch course with related data: Classroom, Instructor, Enrollments, etc.
            var course = await _context.Courses
                .Include(c => c.Classroom) // assuming Classroom navigation property
                .Include(c => c.Professor) // assuming Professor navigation property
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Calculate student's score (example, depends on your schema)
            var enrollment = course.Enrollments.FirstOrDefault(e => e.Student.Id == student.Id);
            var score = enrollment?.Grade ?? 0; // or null, depending

            // Prepare the ViewModel
            var viewModel = new CourseDetailsViewModel
            {
                ClassroomNumber = course.Classroom?.RoomNumber, // adjust property names
                Building = course.Classroom?.Building,
                InstructorName = course.Professor != null ? $"{course.Professor.FirstName} {course.Professor.LastName}" : "N/A",
                ClassDay = course.ClassDay.ToString(),
                ClassTime = course.StartTime.ToString("hh\\:mm"),
                ExamTime = course.ExamDate.ToString("MM/dd/yyyy"),
                Score = (int)score
            };

            return View(viewModel);
        }
        // GET: برای نمایش فرم ارسال پیام
        public async Task<IActionResult> ComposeMessage(int professorId)
        {
            var professor = await _context.Professors.FindAsync(professorId);
            if (professor == null) return NotFound();
            ViewBag.ReceiverProfessor = professor;
            return View();
        }

        // POST: برای ارسال پیام
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ComposeMessage(int receiverProfessorId, string content)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int senderStudentId = int.Parse(userIdStr);

            var message = new Message
            {
                Content = content,
                SenderStudentId = senderStudentId,
                ReceiverProfessorId = receiverProfessorId
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Message sent successfully!";
            return RedirectToAction("ManageCourses"); // بازگشت به لیست دروس
        }
        // GET: برای نمایش فرم اعتراض
        public async Task<IActionResult> SubmitAppeal(int enrollmentId)
        {
            var enrollment = await _context.Enrollments.Include(e => e.Course)
                                             .FirstOrDefaultAsync(e => e.Id == enrollmentId);
            if (enrollment == null) return NotFound();
            return View(enrollment);
        }

        // POST: برای ثبت اعتراض
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAppeal(int enrollmentId, string studentMessage)
        {
            var appeal = new GradeAppeal
            {
                EnrollmentId = enrollmentId,
                StudentMessage = studentMessage
            };
            _context.GradeAppeals.Add(appeal);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Your appeal has been submitted.";
            return RedirectToAction("ManageCourses");
        }
    }
}