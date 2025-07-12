using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ap_project_final.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required] 
        public string StudentId { get; set; } 

        [Required]
        public string FirstName { get; set; } 

        [Required]
        public string LastName { get; set; } 

        [Required]
        [EmailAddress] 
        public string Email { get; set; } 

        [Required]
        public string Password { get; set; } 

        public DateTime EntryDate { get; set; } 

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}