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

        public string NewStudentId { get; set; } // for adding student
        public string RemoveStudentId { get; set; } // for removing student
        public int SelectedProfessorId { get; set; } // for changing professor
    }
}