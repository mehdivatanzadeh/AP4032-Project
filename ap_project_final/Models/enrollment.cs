namespace ap_project_final.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int CourseId { get; set; }   
        public int StudentId { get; set; }  

        public double? Grade { get; set; }

        public Course Course { get; set; }
        public Student Student { get; set; }
    }
}