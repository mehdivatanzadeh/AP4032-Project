using System;
using System.ComponentModel.DataAnnotations;

namespace ap_project_final.Models
{
    public enum AppealStatus { Pending, Reviewed }

    public class GradeAppeal
    {
        public int Id { get; set; }
        [Required]
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; }
        [Required]
        public string StudentMessage { get; set; }
        public string ProfessorResponse { get; set; }
        public DateTime AppealDate { get; set; } = DateTime.Now;
        public AppealStatus Status { get; set; } = AppealStatus.Pending;
    }
}