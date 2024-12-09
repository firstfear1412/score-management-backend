using Microsoft.Data.SqlClient;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Model.Table;
using ScoreManagement.Services.Encrypt;
using ScoreManagement.Common;
using System.Text;

namespace ScoreManagement.Query
{
    public class UserQuery : IUserQuery
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public UserQuery(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("scoreDb")!;
        }
        public async Task<User?> GetUser(UserResource resource)
        {
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

            using (SqlConnection connection = new SqlConnection(_connectionString))
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

        public async Task<bool> UpdateUser(User resource, string query)
        {
            bool flg = false;
            int i = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                #region old code;
                //sbSQL.Append("update MasterJobDetail ");
                //sbSQL.Append("set  UpdateBy = @UpdateBy ,  ");
                //sbSQL.Append("     UpdateDate = GETDATE() ,  ");
                //sbSQL.AppendFormat("  {0}  ", query);
                //sbSQL.Append("where sysJobLineNo = @sysJobLineNo  ");
                //sbSQL.Append("and SysMasterJobNo = @sysMasterJobNo ");
                //sbSQL.Append("and  UpdateToEPOD =  '1'  ");
                //sbSQL.Append("and  JobLineStatus !=  'INACTIVE'  ");
                #endregion old code;

                #region field;
                //[row_id]
                //,[username]
                //,[password]
                //,[role]
                //,[teacher_code]
                //,[prefix]
                //,[firstname]
                //,[lastname]
                //,[email]
                //,[total_failed]
                //,[date_login]
                //,[active_status]
                //,[create_date]
                //,[create_by]
                //,[update_date]
                //,[update_by]
                #endregion field;
                string mainQuery = $@"
                        UPDATE [User]
                        SET
                            {query}
                        WHERE [username] = @username
                    ";
                using (SqlCommand cmd = new SqlCommand(mainQuery, conn))
                {

                    cmd.Transaction = tran;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@username", resource.username ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@password", resource.password ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@role", resource.role ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@teacher_code", resource.teacher_code ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@prefix", resource.prefix ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@firstname", resource.firstname ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@lastname", resource.lastname ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@email", resource.email ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@total_failed", resource.total_failed ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@date_login", resource.date_login ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@active_status", resource.active_status ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@create_date", resource.create_date ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@create_by", resource.create_by ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@update_date", resource.update_date ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@update_by", resource.update_by ?? (object)DBNull.Value));

                    i = await cmd.ExecuteNonQueryAsync();
                    flg = i == 0 ? false : true;
                    if (flg)
                    {
                        tran.Commit();
                    }
                    else
                    {
                        tran.Rollback();
                    }
                    // tran.Rollback();
                }
                conn.Close();
            }
            return flg;
        }
    }
}
