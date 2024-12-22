using Microsoft.Data.SqlClient;
using ScoreManagement.Interfaces;
using ScoreManagement.Model.Table;
using ScoreManagement.Model;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Entity;
using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Model.SubjectScore;

namespace ScoreManagement.Query
{
    public class StudentScoreQuery : IStudentScoreQuery
    {
        private readonly scoreDB _context;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public StudentScoreQuery(IConfiguration configuration, scoreDB context)
        {
            _context = context;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("scoreDb")!;
        }
        public PlaceholderMapping GetPlaceholderMapping(string placeholderKey)
        {
            return _context.PlaceholderMappings
                             .FirstOrDefault(p => p.placeholder_key == placeholderKey && p.active_status == "active")!;
        }

        public async Task<string> GetFieldValue(SubjectDetail subjectDetail, string sourceTable, string fieldName, string condition, string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // สร้าง query SQL แบบ dynamic
                string query = $"SELECT {fieldName} " +
                               $"FROM {sourceTable} " +
                               $"WHERE 1=1 " +
                               $"{condition};";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@subject_id", subjectDetail.subject_id);
                    command.Parameters.AddWithValue("@student_id", subjectDetail.student_id);
                    command.Parameters.AddWithValue("@academic_year", subjectDetail.academic_year);
                    command.Parameters.AddWithValue("@section", subjectDetail.section);
                    command.Parameters.AddWithValue("@semester", subjectDetail.semester);
                    command.Parameters.AddWithValue("@username", username);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return string.Empty;
                        }
                        if (reader.Read())
                        {
                            // ดึงค่าจากฟิลด์ที่ระบุ
                            return reader.IsDBNull(0) ? string.Empty : reader.GetValue(0)?.ToString() ?? string.Empty;
                        }
                    }
                }
                await connection.CloseAsync();
            }

            return string.Empty; // คืนค่าเป็นค่าว่างหากไม่พบข้อมูล
        }

        public async Task<bool> UpdateTemplateEmail(EmailTemplateResource resource)
        {
            bool flg = false;
            int i = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // สร้าง query SQL แบบ dynamic
                string query = $@"
                    UPDATE et
                    SET [subject] = @subject, body = @body
                    FROM [EmailTemplate] et
                    JOIN [UserEmailTemplate] ut ON ut.template_id = et.template_id
                    WHERE et.template_id = @template_id AND ut.username = @username
                ";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@template_id", resource.template_id);
                    command.Parameters.AddWithValue("@subject", resource.subject);
                    command.Parameters.AddWithValue("@body", resource.body);
                    command.Parameters.AddWithValue("@username", resource.username);

                    i = await command.ExecuteNonQueryAsync();
                    flg = i == 0 ? false : true;
                }
                await connection.CloseAsync();
            }

            return flg;
        }

    }
}
