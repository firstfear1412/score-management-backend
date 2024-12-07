using Microsoft.Data.SqlClient;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Model.Table;
using ScoreManagement.Services.Encrypt;
using ScoreManagement.Common;

namespace ScoreManagement.Query
{
    public class UserQuery : IUserQuery
    {
        private readonly IConfiguration _configuration;
        public UserQuery(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<User?> GetUser(UserResource resource)
        {
            string connectionString = _configuration.GetConnectionString("scoreDb")!;
            // คำสั่ง SQL
            User? user = null;
            string query = @"
                        SELECT TOP 1
                            [row_id]
                            ,[username]
                            ,[password]
                            ,[role]
                            ,[teacher_code]
                            ,[prefix]
                            ,[firstname]
                            ,[lastname]
                            ,[email]
                            ,[total_failed]
                            ,[date_login]
                            ,[active_status]
                            ,[create_date]
                            ,[create_by]
                            ,[update_date]
                            ,[update_by]
                        FROM [User]
                        WHERE username = @username
                            AND active_status = 'active'
                    ";
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // เปิดการเชื่อมต่อ
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // เพิ่มพารามิเตอร์เพื่อป้องกัน SQL Injection
                    command.Parameters.AddWithValue("@username", resource.username);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return null;
                        }
                        // อ่านผลลัพธ์
                        if (reader.Read())
                        {
                            // แปลงข้อมูลจาก DataReader เป็น User
                            User md = new User();
                            int col = 0;
                            int colNull = 0;
                            md.row_id = !reader.IsDBNull(colNull++) ? reader.GetInt32(col) : default; col++;
                            md.username = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.password = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.role = !reader.IsDBNull(colNull++) ? reader.GetInt32(col) : default; col++;
                            md.teacher_code = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.prefix = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.firstname = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.lastname = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.email = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.total_failed = !reader.IsDBNull(colNull++) ? reader.GetInt32(col) : default; col++;
                            md.date_login = !reader.IsDBNull(colNull++) ? reader.GetDateTime(col) : (DateTime?)null; col++;
                            md.active_status = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.create_date = !reader.IsDBNull(colNull++) ? reader.GetDateTime(col) : (DateTime?)null; col++;
                            md.create_by = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.update_date = !reader.IsDBNull(colNull++) ? reader.GetDateTime(col) : (DateTime?)null; col++;
                            md.update_by = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            col = 0;
                            colNull = 0;
                            user = md;
                        }
                        
                    }
                }
                await connection.CloseAsync();
            }
            return user;
        }
    }
}
