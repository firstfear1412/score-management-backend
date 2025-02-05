using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;


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
            GetLovQuery("academic_year");

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
        public async Task<List<SubjectResource>> GetSubjectByConditionQuery(SubjectResource resource)
        {
            var subjectList = new List<SubjectResource>();
            string sqlContext = @"SELECT subject_id,subject_name
                FROM Subject WHERE CONCAT(subject_id,' ',subject_name) LIKE '%' + @inputSearh + '%'";
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sqlContext, connection))
                    {
                        // Add parameters
                        command.Parameters.AddWithValue("@inputSearh", resource.subjectSearch);


                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var result = new SubjectResource
                                {
                                    subject_id = reader["subject_id"]?.ToString(),
                                    subject_name = reader["subject_name"]?.ToString(),

                                };
                                subjectList.Add(result);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (add your logging mechanism here)
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

            return subjectList;
        }
        public async Task<List<SubjectResource>> GetLovSubject(SubjectResource resource)
        {
            var subjectList = new List<SubjectResource>();
            string sqlContext = @"WITH SubjectRanked AS (
                                SELECT sj.row_id,sj.subject_id, 
                                       sj.subject_name,
                                       sl.teacher_code,
                                       ROW_NUMBER() OVER (PARTITION BY sj.subject_id ORDER BY sj.create_by DESC) AS row_num
                                FROM Subject sj
                                INNER JOIN SubjectHeader sh 
                                    ON sh.subject_id = sj.subject_id 
                                INNER JOIN (
                                    SELECT sys_subject_no, MIN(teacher_code) AS teacher_code
                                    FROM SubjectLecturer
                                    WHERE active_status = 'active'
                                    AND (@teacherCode IS NULL OR teacher_code = @teacherCode)
                                    GROUP BY sys_subject_no
                                ) sl 
                                ON sh.sys_subject_no = sl.sys_subject_no
                            )
                            SELECT row_id,subject_id, 
                                   subject_name,
                                   teacher_code
                            FROM SubjectRanked
                            WHERE row_num = 1
                            ORDER BY row_id DESC";
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sqlContext, connection))
                    {
                        // Add parameters
                        command.Parameters.AddWithValue("@teacherCode", resource.role == 2 ? (object)resource.teacher_code : DBNull.Value);


                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var result = new SubjectResource
                                {
                                    subject_id = reader["subject_id"]?.ToString(),
                                    subject_name = reader["subject_name"]?.ToString(),

                                };
                                subjectList.Add(result);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (add your logging mechanism here)
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

            return subjectList;
        }
    }

}
