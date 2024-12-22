namespace ScoreManagement.Model
{
    public class EmailTemplateGroup
    {
        public List<TemplateCollection> PrivateTemplates { get; set; } = new List<TemplateCollection>();
        public List<TemplateCollection> DefaultTemplates { get; set; } = new List<TemplateCollection>();
    }
    public class TemplateDetail
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class TemplateCollection
    {
        public string TemplateName { get; set; }
        public TemplateDetail Detail { get; set; }
    }
}
