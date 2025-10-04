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
            WHERE sp.byte_reference = '{byteReference}'
            AND sp.active_status = 'active';
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
            string sqlContext;
            if (resource.role == 2)
            {
                sqlContext = @"						   
	                         SELECT s.subject_id, s.subject_name FROM SubjectHeader sh
	                            JOIN Subject s ON sh.subject_id = s.subject_id
	                            JOIN SubjectLecturer sl ON sh.sys_subject_no = sl.sys_subject_no
	                            WHERE 1=1
								AND s.active_status = 'active'
	                            AND sh.active_status = 'active'
								AND sl.active_status = 'active'
	                            AND sl.teacher_code = NULLIF(@teacherCode, '')
	                            GROUP BY s.subject_id, s.subject_name";
            }
            else
            {
                sqlContext = @"		                 
                            SELECT s.subject_id, s.subject_name FROM SubjectHeader sh
                                JOIN Subject s ON sh.subject_id = s.subject_id
	                            WHERE 1=1
							    AND s.active_status = 'active'
								AND sh.active_status = 'active'
								GROUP BY s.subject_id, s.subject_name";
            }
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
