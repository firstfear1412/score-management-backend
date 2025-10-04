using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.Table
{
    public class PlaceholderMapping
    {
        [Key]
        public int row_id { get; set; }

        public string? placeholder_key { get; set; }

        public string? source_table { get; set; }

        public string? field_name { get; set; }

        public string? condition { get; set; }

        public string? desc { get; set; }

        public string? active_status { get; set; }

    }
}
