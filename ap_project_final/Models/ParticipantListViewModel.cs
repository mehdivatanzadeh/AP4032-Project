namespace ap_project_final.Models
{
    public class ParticipantListViewModel
    {
        public string ClassroomName { get; set; }
        public int ClassroomId { get; set; }
        public List<Student> Students { get; set; }
        public List<Professor> Instructors { get; set; } 
    }
}