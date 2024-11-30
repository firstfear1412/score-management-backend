using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model.SystemParam
{
    public class SystemParamResource
    {
        [Key]
        public int row_id { get; set; }

        public string? byte_reference { get; set; }

        public string? byte_code { get; set; }

        public string? byte_desc_th { get; set; }

        public string? byte_desc_en { get; set; }

        public string? active_status { get; set; }

        public DateTime? create_date { get; set; }

        public string? create_by { get; set; }

        public DateTime? update_date { get; set; }

        public string? update_by { get; set; }

    }
}
