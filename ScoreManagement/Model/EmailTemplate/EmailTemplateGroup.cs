﻿namespace ScoreManagement.Model
{
    public class EmailTemplateGroup
    {
        public List<TemplateCollection> PrivateTemplates { get; set; } = new List<TemplateCollection>();
        public List<TemplateCollection> BasicTemplates { get; set; } = new List<TemplateCollection>();
        public Dictionary<string, int?> DefaultTemplates { get; set; } = new Dictionary<string, int?>();
    }
    public class TemplateDetail
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class TemplateCollection
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public TemplateDetail Detail { get; set; }
    }
}
