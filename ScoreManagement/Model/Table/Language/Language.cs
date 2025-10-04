using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table
{
    public class Language
    {
        [Key]
        public int row_id { get; set; }

        public string message_key { get; set; } = string.Empty;
        public string language_code { get; set; } = string.Empty;
        public string message_content { get; set; } = string.Empty;

        //public string? message_th { get; set; }

        //public string? message_en { get; set; }

    }
}
