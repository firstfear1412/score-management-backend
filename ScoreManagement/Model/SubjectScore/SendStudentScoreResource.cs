namespace ScoreManagement.Model
{
    public class SendStudentScoreResource
    {
        public SubjectDetail SubjectDetail { get; set; }
        public EmailDetail EmailDetail { get; set; }
        public string username { get; set; }
    }
    public class SubjectDetail
    {
        public string subject_id { get; set; }
        public string subject_name { get; set; } // Added SubjectName for mapping
        public string academic_year { get; set; }
        public string semester { get; set; }
        public string section { get; set; }
        public string student_id { get; set; }
    }

    public class EmailDetail
    {
        public string SubjectEmail { get; set; }
        public string ContentEmail { get; set; }
    }
}
