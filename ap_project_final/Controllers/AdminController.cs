using ap_project_final.Data;
using ap_project_final.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ap_project_final.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        // --- Professor Management ---

        public async Task<IActionResult> InstructorsList()
        {
            return View(await _context.Professors.ToListAsync());
        }

        public IActionResult AddInstructor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInstructor(Professor professor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(professor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(InstructorsList));
            }
            return View(professor);
        }

        public async Task<IActionResult> EditInstructor(int? id)
        {
            if (id == null) return NotFound();
            var instructor = await _context.Professors.FindAsync(id);
            if (instructor == null) return NotFound();
            return View(instructor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInstructor(int id, Professor professor)
        {
            if (id != professor.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(professor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(InstructorsList));
            }
            return View(professor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            var instructor = await _context.Professors.FindAsync(id);
            if (instructor == null) return NotFound();
            _context.Professors.Remove(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(InstructorsList));
        }

        // --- Student Management ---

        public async Task<IActionResult> StudentsList()
        {
            return View(await _context.Students.ToListAsync());
        }

        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(Student student)
        {
            
                TempData["Message"] = "Student added successfully!";
                _context.Add(student);
                await _context.SaveChangesAsync();
                
                return RedirectToAction("ManageUsers");
                
                
            
            return View(student);
        }

        public async Task<IActionResult> EditStudent(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int id, Student student)
        {
            if (id != student.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(StudentsList));
            }
            return View(student);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageUsers");
        }

        // --- Course Management ---

        public async Task<IActionResult> CoursesList()
        {
            return View(await _context.Courses.ToListAsync());
        }

        public IActionResult AddCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CoursesList));
            }
            return View(course);
        }

        public async Task<IActionResult> EditCourse(int? id)
        {
            if (id == null) return NotFound();
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Course course)
        {
            if (id != course.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CoursesList));
            }
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CoursesList));
        }

        // --- Complex Logic ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudentToCourse(int studentId, int courseId)
        {
            var student = await _context.Students.FindAsync(studentId);
            var newCourse = await _context.Courses.FindAsync(courseId);

            if (student == null || newCourse == null) return NotFound();

            var studentCourses = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.Course)
                .ToListAsync();

            bool hasConflict = studentCourses.Any(existingCourse =>
                existingCourse.ClassDay == newCourse.ClassDay &&
                existingCourse.StartTime == newCourse.StartTime);

            if (hasConflict)
            {
                TempData["Error"] = "Time conflict detected for the student.";
                return RedirectToAction("ManageCourse", new { id = courseId });
            }

            var enrollment = new Enrollment { StudentId = studentId, CourseId = courseId };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Student assigned successfully.";
            return RedirectToAction("ManageCourse", new { id = courseId });
        }

        
      
        
        public async Task<IActionResult> ManageUsers()
        {
            var model = new UserManagementViewModel
            {
                Students = await _context.Students.ToListAsync(),
                Professors = await _context.Professors.ToListAsync()
            };
            return View(model);
        }
        public async Task<IActionResult> ManageParticipants(int classroomId)
        {
            // Fetch classroom info
            var classroom = await _context.Classrooms.FindAsync(classroomId);
            if (classroom == null)
                return NotFound();

            // Fetch students enrolled in this classroom
            var students = await _context.Enrollments
                .Where(e => e.Course.Id == classroomId)
                .Include(e => e.Student)
                .Select(e => e.Student)
                .ToListAsync();

            // If instructors are involved, fetch them similarly
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
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
