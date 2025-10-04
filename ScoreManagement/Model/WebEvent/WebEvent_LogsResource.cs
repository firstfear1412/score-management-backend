using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ScoreManagement.Model
{
    public class WebEvent_LogsResource
    {
        public int event_id { get; set; }

        public DateTime? event_time { get; set; }

        public string? event_type { get; set; }

        public string? event_code { get; set; }

        public string? event_detail_code { get; set; }

        public string? message { get; set; }

        public string? application_path { get; set; }

        public string? machine_name { get; set; }

        public string? ip_address { get; set; }

        public string? request_url { get; set; }

        public string? details { get; set; }

        public string? user_id { get; set; }

    }
}
