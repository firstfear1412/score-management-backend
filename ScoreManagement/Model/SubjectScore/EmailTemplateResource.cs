namespace ScoreManagement.Model.SubjectScore
{
    public class EmailTemplateResource
    {
        public int? template_id { get; set; }
        public string? template_name { get; set; } = string.Empty;
        public string? subject { get; set; } = string.Empty;
        public string? body { get; set; } = string.Empty;
        public string? username { get; set; }
    }
}
