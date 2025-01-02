using Microsoft.Data.SqlClient;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Entity;


namespace ScoreManagement.Query
{
    public class LovContantQuery : ILovContantQuery
    {
        private readonly scoreDB _context;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public LovContantQuery(IConfiguration configuration, scoreDB context)
        {
            _context = context;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("scoreDb")!;
        }

        // Reusable method for querying the database
        private async Task<List<LovContantsResource>> GetLovQuery(string byteReference)
        {
            var resultList = new List<LovContantsResource>();
            string query = $@"
            SELECT byte_code, byte_desc_th, byte_desc_en 
            FROM SystemParam sp 
            WHERE sp.byte_reference = '{byteReference}';
        ";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var result = new LovContantsResource
                        {
                            byte_code = reader["byte_code"]?.ToString(),
                            byte_desc_th = reader["byte_desc_th"]?.ToString(),
                            byte_desc_en = reader["byte_desc_en"]?.ToString()
                        };
                        resultList.Add(result);
                    }
                }

                await connection.CloseAsync();
            }

            return resultList;
        }

        // Method to fetch 'send_status'
        public Task<List<LovContantsResource>> GetLovSendStatusQuery() =>
            GetLovQuery("send_status");

        // Method to fetch 'major_code'
        public Task<List<LovContantsResource>> GetLovMajorCodeQuery() =>
            GetLovQuery("major_code");

        // Method to fetch 'role'
        public Task<List<LovContantsResource>> GetLovRoleQuery() =>
            GetLovQuery("role");

        // Method to fetch 'acedemic_year'
        public Task<List<LovContantsResource>> GetLovAcedemicYearQuery() =>
            GetLovQuery("acedemic_year");

        // Method to fetch 'score_type'
        public Task<List<LovContantsResource>> GetLovScoreTypeQuery() =>
            GetLovQuery("score_type");

        // Method to fetch 'semester'
        public Task<List<LovContantsResource>> GetLovSemesterQuery() =>
            GetLovQuery("semester");

        // Method to fetch 'section'
        public Task<List<LovContantsResource>> GetLovSectionQuery() =>
            GetLovQuery("section");

        // Method to fetch 'active_status'
        public Task<List<LovContantsResource>> GetLovActiveStatusQuery() =>
            GetLovQuery("active_status");
    }

}
