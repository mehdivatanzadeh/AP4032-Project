using System;
using System.ComponentModel.DataAnnotations;

namespace ap_project_final.Models
{
    public class Message
    {
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;

        // Sender Info
        public int? SenderStudentId { get; set; }
        public Student SenderStudent { get; set; }
        public int? SenderProfessorId { get; set; }
        public Professor SenderProfessor { get; set; }

        // Receiver Info
        public int? ReceiverStudentId { get; set; }
        public Student ReceiverStudent { get; set; }
        public int? ReceiverProfessorId { get; set; }
        public Professor ReceiverProfessor { get; set; }
    }
}