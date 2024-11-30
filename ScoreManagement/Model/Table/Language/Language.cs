using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table.Language
{
    public class Language
    {
        [Key]
        public int row_id { get; set; }

        public string? message_key { get; set; }

        public string? message_th { get; set; }

        public string? message_en { get; set; }

    }
}
