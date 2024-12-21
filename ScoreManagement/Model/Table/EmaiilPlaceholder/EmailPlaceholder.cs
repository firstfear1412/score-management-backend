using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table 
{
    public class EmailPlaceholder
    {
        [Key]
        public int row_id { get; set; }

        public string placeholder_key { get; set; } = string.Empty;

        public string desc_th { get; set; } = string.Empty;

        public string desc_en { get; set; } = string.Empty;

        public string active_status { get; set; } = string.Empty;

    }
}
