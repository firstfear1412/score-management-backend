using Microsoft.Data.SqlClient;
using ScoreManagement.Interfaces.Dashboard;
using ScoreManagement.Model.ScoreAnnoucement;
using Microsoft.Extensions.Configuration;
using ScoreManagement.Model;
using ScoreManagement.Model.Dashboard;

namespace ScoreManagement.Query.Dashboard
{
    public class DashboardQuery : IDashboardQuery
    {
        private readonly string _connectionString;

        public DashboardQuery(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("scoreDb")!;
        }

        public async Task<DashboardStatisticsResponse?> GetDashboardStatistics(ScoreAnnoucementDashboard resource)
        {
            // คำสั่ง SQL ที่ได้รับการปรับปรุง
            string query = @"
                            SELECT 
                                MAX(ss.midterm_score) AS MAX_MIDTERM_SCORE, 
                                MIN(ss.midterm_score) AS MIN_MIDTERM_SCORE, 
                                AVG(ss.midterm_score) AS AVG_MIDTERM_SCORE, 
                                ROUND(STDEV(ss.midterm_score), 1) AS STD_MIDTERM_SCORE, 
                                COUNT(*) AS NUMBER_STUDENT
                            FROM (SELECT 
                                        sys_subject_no, 
                                        student_id, 
                                        seat_no, 
                                        accumulated_score, 
                                        midterm_score, 
                                        final_score, 
                                        active_status 
                                FROM SubjectScore
                                WHERE active_status = 'active') ss
                            JOIN (SELECT 
                                        sh.sys_subject_no, 
                                        sh.subject_id, 
                                        yrs.byte_desc_th AS academic_year,
                                        sem.byte_desc_th AS semester, 
                                        sec.byte_desc_th AS section,
                                        sh.active_status
                                  FROM SubjectHeader sh
                                  JOIN SystemParam yrs ON sh.semester = yrs.byte_code AND yrs.byte_reference = 'acedemic_year'
                                  JOIN SystemParam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                  JOIN SystemParam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section') shh ON ss.sys_subject_no = shh.sys_subject_no
                            JOIN (SELECT subject_id, subject_name, active_status 
                                  FROM Subject
                                  WHERE active_status = 'active') s ON s.subject_id = shh.subject_id
                                WHERE 1 = 1
                                AND ss.sys_subject_no = @sys_subject_no
                                AND (@subject_name IS NULL OR s.subject_name = @subject_name)
                                AND (@academic_year IS NULL OR shh.academic_year = @academic_year)
                                AND (@semester IS NULL OR shh.semester = @semester)
                                AND (@section IS NULL OR shh.section = @section)
                                AND ss.active_status = 'active'
    ";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // เพิ่มพารามิเตอร์ในคำสั่ง SQL
                    command.Parameters.AddWithValue("@sys_subject_no", (object?)resource.sys_subject_no ?? DBNull.Value);
                    command.Parameters.AddWithValue("@subject_name", (object?)resource.subject_name ?? DBNull.Value);
                    command.Parameters.AddWithValue("@academic_year", (object?)resource.academic_year ?? DBNull.Value);
                    command.Parameters.AddWithValue("@semester", (object?)resource.semester ?? DBNull.Value);
                    command.Parameters.AddWithValue("@section", (object?)resource.section ?? DBNull.Value);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return null;
                        }

                        DashboardStatisticsResponse dashboardStats = null;

                        if (reader.Read())
                        {
                            dashboardStats = new DashboardStatisticsResponse
                            {
                                MaxAccumulatedScore = reader.IsDBNull(0) ? null : reader.GetInt32(0),
                                MinAccumulatedScore = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                                AvgAccumulatedScore = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                                StdAccumulatedScore = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                                NumberOfStudents = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                            };
                        }

                        return dashboardStats;
                    }
                }
            }
        }
    }
}