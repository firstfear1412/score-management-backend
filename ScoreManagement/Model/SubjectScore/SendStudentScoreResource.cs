namespace ScoreManagement.Model
{
    public class SendStudentScoreResource
    {
        public SubjectDetail SubjectDetail { get; set; }
        public EmailDetail EmailDetail { get; set; }
        public string username { get; set; }
        public List<string> student_id { get; set; }  // Changed to List<string>
    }
    public class SubjectDetail
    {
        public string subject_id { get; set; }
        public int academic_year { get; set; }
        public int semester { get; set; }
        public int section { get; set; }
    }

    public class EmailDetail
    {
        public string SubjectEmail { get; set; }
        public string ContentEmail { get; set; }
    }
}
