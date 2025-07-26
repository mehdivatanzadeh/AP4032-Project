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
            if (_context.Professors.Any(s => s.Email == professor.Email || s.ProfessorId == professor.ProfessorId))
            {
                ViewBag.Error = "Email or Professor Id already exists.";
                return View();
            }
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
            
                var instructor = await _context.Professors.FirstOrDefaultAsync(i => i.ProfessorId == instructorId);
                
                    _context.Professors.Remove(instructor);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Instructor removed successfully.";
                
                
            
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
            
                if(_context.Students.Any(s => s.Email==student.Email || s.StudentId == student.StudentId))
            {
                ViewBag.Error = "Email or Student Id already exists.";
                return View();
            }
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




            course.Enrollments = null;
            course.Professor = _context.Professors.First(p => p.Id == course.ProfessorId);
            if (course.Professor == null) return NotFound();
            // Save course with selected professor and classroom
            if (_context.Courses.Any(c => c.Professor == course.Professor && c.StartTime == course.StartTime && c.ClassDay==course.ClassDay))
            {
                ViewBag.Error = "Time conflict detected for the professor.";
                return View();

            }
            if (_context.Courses.Any(c => c.ClassroomId == course.ClassroomId && c.StartTime == course.StartTime && c.ClassDay==course.ClassDay))
            {
                ViewBag.Error = "Time conflict detected for the classroom.";
                return View();

            }
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Redirect to list or confirmation page
            return RedirectToAction("CoursesList");
        }

        public IActionResult EditCourse(int id)
        {
            var course = _context.Courses
                .Include(c => c.Professor)
                .Include(c => c.Classroom)
                // include other navigations if needed
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
                return NotFound();

            // Prepare ViewBag items for dropdowns, e.g.,
            ViewBag.Professors = new SelectList(_context.Professors, "Id", "FullName", course.ProfessorId);
            ViewBag.Classrooms = new SelectList(_context.Classrooms, "Id", "Name", course.ClassroomId);

            return View(course); // Pass course as model
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCourse(int id, Course model)
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    _context.SaveChanges();
                    return RedirectToAction("ManageParticipants", new { id = model.Id });
                }
                catch (Exception ex)
                {
                    // handle errors
                    ViewBag.Error = "Error saving data.";
                }
            }

            // If validation fails, redisplay with dropdowns
            ViewBag.Professors = new SelectList(_context.Professors, "Id", "FullName", model.ProfessorId);
            ViewBag.Classrooms = new SelectList(_context.Classrooms, "Id", "Name", model.ClassroomId);
            return View(model);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClassroom(int id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom != null)
            {
                _context.Classrooms.Remove(classroom);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ClassroomsList");
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
                return RedirectToAction($"ManageParticipants/{courseId}");

            var student = _context.Students.FirstOrDefault(s => s.StudentId == studentId);
            if (student == null)
                return RedirectToAction($"ManageParticipants/{courseId}");

            if (!_context.Enrollments.Any(e => e.CourseId == courseId && e.Student.StudentId == studentId))
            {
               
                var enrollment = new Enrollment { CourseId = courseId, Student = student, Course = _context.Courses.First(c => c.Id == courseId)  };
                if (enrollment == null) return NotFound();
                var course = _context.Courses.First(c => c.Id == courseId);
                if (course == null) return NotFound();
                if (course.Enrollments != null)
                {
                    if (_context.Classrooms.First(c => c.Id == course.ClassroomId).Capacity <= _context.Enrollments.Select(e => e.CourseId==courseId).ToList().Count())
                    {
                        ViewBag.Error = "Classroom is full.";
                        return RedirectToAction("ManageParticipants", new { id = courseId });

                    }
                }
                if(_context.Enrollments.Any(e => e.Student.StudentId == studentId && e.Course.StartTime == course.StartTime))
                {
                    ViewBag.Error = "Time conflict detected for the student.";
                    return RedirectToAction("ManageParticipants", new { id=courseId });
                }
                
                _context.SaveChanges();
                _context.Enrollments.Add(enrollment);
                _context.SaveChanges();
            }
            


                return RedirectToAction("ManageParticipants", new { id = courseId });
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
            return RedirectToAction("ManageParticipants", new { id = courseId });
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
