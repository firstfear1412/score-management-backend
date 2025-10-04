namespace ScoreManagement.Model
{
    public class SubjectResponse
    {

        public string? subject_id { get; set; }
        public string? subject_name { get; set; }

    }

    public class SubjectRequest
    {
        public string? teacher_code { get; set; }
    }
}