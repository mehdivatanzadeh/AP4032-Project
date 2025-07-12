using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ap_project_final.Models
{
    public class Course
    {
        public int Id { get; set; } 

        [Required]
        public string CourseCode { get; set; } 

        [Required]
        public string Title { get; set; } 

        public int Units { get; set; } 

        public string Description { get; set; } 

        public DateTime ExamDate { get; set; } 

        public int ProfessorId { get; set; } 

        [ForeignKey("ProfessorId")]
        public Professor Professor { get; set; } 

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}