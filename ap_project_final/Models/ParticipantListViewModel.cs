namespace ap_project_final.Models
{
    public class ManageParticipantsViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int CurrentProfessorId { get; set; }
        public string CurrentProfessorName { get; set; }

        public List<Professor> AllProfessors { get; set; }
        public List<Student> Students { get; set; }

        
    }
}