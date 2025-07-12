using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ap_project_final.Models
{
    public class Professor
    {
        public int Id { get; set; } 

        [Required]
        public string ProfessorId { get; set; }

        [Required]
        public string FirstName { get; set; } 

        [Required]
        public string LastName { get; set; } 

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; } 

        public DateTime HireDate { get; set; } 

        public decimal Salary { get; set; }

        public ICollection<Course> Courses { get; set; }
    }
}