using ap_project_final.Data;
using ap_project_final.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
           
                _context.Add(professor);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageUsers");
            
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
        public async Task<IActionResult> RemoveInstructor(string instructorId)
        {
            if (int.TryParse(instructorId, out int id))
            {
                var instructor = await _context.Professors.FirstOrDefaultAsync(i => i.ProfessorId == instructorId);
                if (instructor != null)
                {
                    _context.Professors.Remove(instructor);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Instructor removed successfully.";
                }
                else
                {
                    TempData["Error"] = "Instructor not found.";
                }
            }
            else
            {
                TempData["Error"] = "Invalid Instructor ID.";
            }
            return RedirectToAction("ManageUsers"); // Adjust based on your view
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
        public async Task<IActionResult> RemoveStudent(string studentId)
        {
            
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId==studentId);
                if (student != null)
                {
                    _context.Students.Remove(student);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Student removed successfully.";
                }
                else
                {
                    TempData["Error"] = "Student not found.";
                }
            
            
            return RedirectToAction("ManageUsers"); // Adjust based on your view
        }

        // --- Course Management ---

        public async Task<IActionResult> CoursesList()
        {
            return View(await _context.Courses.ToListAsync());
        }

        public IActionResult AddCourse()
        {
            // Populate Professors
            ViewBag.Professors = _context.Professors
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.LastName // or p.FirstName + " " + p.LastName
                }).ToList();

            // Populate Classrooms
            ViewBag.Classrooms = _context.Classrooms
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Building} - Room {c.RoomNumber} (Capacity: {c.Capacity})"
                }).ToList();

            return View(new Course());
        }

        // POST: Add new course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(Course course)
        {
            // Server-side validation
            
                // Repopulate dropdowns if validation fails
               

               
            

            // Save course with selected professor and classroom
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Redirect to list or confirmation page
            return RedirectToAction("CoursesList");
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
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("CoursesList");
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
        public IActionResult ManageParticipants(int courseId)
        {
            var course = _context.Courses
                .Include(c => c.Professor)    // Assumed navigation property
                .Include(c => c.Enrollments).ThenInclude(e => e.Student) // List of enrolled students
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null)
                return NotFound();

            var viewModel = new ManageParticipantsViewModel
            {
                CourseId = course.Id,
                CourseName = course.CourseCode,
                CurrentProfessorId = course.Professor?.Id ?? 0,
                CurrentProfessorName = course.Professor?.LastName ?? "None",
                AllProfessors = _context.Professors.ToList(),
                Students = course.Enrollments.Select(e => e.Student).ToList()
            };

            return View(viewModel);
        }

        // Change professor
        [HttpPost]
        public IActionResult ChangeProfessor(int courseId, int newProfessorId)
        {
            var course = _context.Courses.Include(c => c.Professor).FirstOrDefault(c => c.Id == courseId);
            if (course == null)
                return NotFound();

            var professor = _context.Professors.Find(newProfessorId);
            if (professor == null)
                return NotFound();

            course.Professor = professor;
            _context.SaveChanges();

            return RedirectToAction("ManageParticipants", new { courseId });
        }

        // Add student by ID
        [HttpPost]
        public IActionResult AddStudentToCourse(int courseId, string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
                return RedirectToAction("ManageParticipants", new { courseId });

            var student = _context.Students.FirstOrDefault(s => s.StudentId == studentId);
            if (student == null)
                return RedirectToAction("ManageParticipants", new { courseId });

            if (!_context.Enrollments.Any(e => e.CourseId == courseId && e.Student.StudentId == studentId))
            {
                var enrollment = new Enrollment { CourseId = courseId, Student = student };
                _context.Enrollments.Add(enrollment);
                _context.SaveChanges();
            }

            return RedirectToAction("ManageParticipants", new { courseId });
        }

        // Remove student by ID
        [HttpPost]
        public IActionResult RemoveStudentFromCourse(int courseId, string studentId)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.CourseId == courseId && e.Student.StudentId == studentId);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageParticipants", new { courseId });
        }
        [HttpPost]
        public IActionResult UpdateInstructor(int courseId, int InstructorId)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == courseId);
                
            if (course == null)
                return NotFound();

            var instructor = _context.Professors.Find(InstructorId);
            if (instructor == null)
                return NotFound();

            // Update instructor
            course.Professor = instructor;
            _context.SaveChanges();

            return RedirectToAction("ManageParticipants", new { courseId = courseId });
        }
        public IActionResult AddClassroom()
        {
            return View();
        }

        // POST: Handle form submission to add classroom
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClassroom(Classroom classroom)
        {

            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            // Redirect to list of classrooms or other page
            return RedirectToAction("ClassroomsList");
        }
        public IActionResult ClassroomsList()
        {
            var classrooms = _context.Classrooms.ToList();
            return View(classrooms);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
