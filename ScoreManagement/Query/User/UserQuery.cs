using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Model.Table;

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

        public async Task<User?> GetUserInfo(UserResource resource) {
            try {
                {
                    User? userInfo = null;
                    string query = @"
                    SELECT
                             u.[username],
                             u.[email],
                             u.[teacher_code],
                             spp.[byte_desc_th] AS prefix_description_th,
                             spp.[byte_desc_en] AS prefix_description_en,
                             u.[firstname],
                             u.[lastname],
                             u.[active_status],
                             u.role,
                             spr.[byte_desc_th] as role_description_th,
                             spr.[byte_desc_en] as role_description_en
                       FROM [ScoreManagement].[dbo].[User] u
                       LEFT JOIN [ScoreManagement].[dbo].[SystemParam] spr 
                              ON u.role = spr.byte_code AND spr.byte_reference = 'role'
                       LEFT JOIN [ScoreManagement].[dbo].[SystemParam] spp 
                              ON u.prefix = spp.byte_code AND spp.byte_reference = 'prefix'
                       WHERE u.username = @username
                         AND u.active_status = 'active';
               ";

                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@username", resource.username);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.HasRows)
                                {
                                    if (reader.Read())
                                    {
                                        int col = 0;
                                        int colNull = 0;
                                        User result = new User();

                                        result.username = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                                        result.email = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                                        result.teacher_code = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                                        result.prefix_description_th = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                                        result.prefix_description_en = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                                        result.firstname = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                                        result.lastname = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                                        result.active_status = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                                        result.role = !reader.IsDBNull(colNull++) ? reader.GetInt32(col) : default; col++;
                                        result.role_description_th = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                                        result.role_description_en = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;

                                        col = 0;
                                        colNull = 0;
                                        userInfo = result;
                                    }
                                }
                            }
                        }
                        await connection.CloseAsync();
                    }
                    return userInfo;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error insert data.", ex);
            }
        }

        public async Task<bool> updateUserByConditionQuery(UserResource resource)
        {
            bool flg = false;
            int i = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                string mainQuery = $@"
                        UPDATE [User]
                            SET [password] = @password
                            WHERE username = @username
                    ";
                using (SqlCommand cmd = new SqlCommand(mainQuery, conn))
                {

                    cmd.Transaction = tran;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@username", resource.username ?? (object)DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@password", resource.newPassword ?? (object)DBNull.Value));
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
                }
                conn.Close();
            }
            return flg;
        }

        public async Task<List<UserResource>> GetAllUsers()
        {
            var users = new List<UserResource>();
            string query = @"
                            SELECT 
                                u.row_id, 
                                u.username, 
                                spr.byte_desc_th AS role, 
                                u.teacher_code, 
                                spp.byte_desc_th AS prefix,
                                u.firstname, 
                                u.lastname, 
                                u.email, 
                                u.date_login, 
                                u.active_status
                            FROM 
                                [User] u
                            LEFT JOIN 
                                SystemParam spr ON u.role = spr.byte_code AND spr.byte_reference = 'role' 
                            LEFT JOIN 
                                SystemParam spp ON u.prefix = spp.byte_code AND spp.byte_reference = 'prefix' ";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return new List<UserResource>(); // คืนค่ารายการว่างแทน null
                        }

                        while (reader.Read())
                        {
                            // แปลงข้อมูลจาก DataReader เป็น User
                            UserResource md = new UserResource();
                            int col = 0;
                            int colNull = 0;
                            md.row_id = !reader.IsDBNull(colNull++) ? reader.GetInt32(col) : default; col++;
                            md.username = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            //md.password = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.role = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.teacher_code = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.prefix = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.firstname = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.lastname = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.email = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            //md.total_failed = !reader.IsDBNull(colNull++) ? reader.GetInt32(col) : default; col++;
                            md.date_login = !reader.IsDBNull(colNull++) ? reader.GetDateTime(col) : (DateTime?)null; col++;
                            md.active_status = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            //md.create_date = !reader.IsDBNull(colNull++) ? reader.GetDateTime(col) : (DateTime?)null; col++;
                            //md.create_by = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            //md.update_date = !reader.IsDBNull(colNull++) ? reader.GetDateTime(col) : (DateTime?)null; col++;
                            //md.update_by = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            col = 0;
                            colNull = 0;

                            users.Add(md); // เพิ่ม User เข้าในรายการ
                        }
                    }
                }
                await connection.CloseAsync();
            }
            return users;
        }

        public async Task<bool> CheckEmailExist(string email)
        {
            const string query_Email = @"SELECT COUNT(1) FROM [User] WHERE email = @Email";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query_Email, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result) > 0; // ถ้ามีอีเมลอยู่แล้ว, คืนค่า true
                }
            }
        }

        public async Task<bool> CheckTeacherCodeExist(string teacher_code)
        {
            const string query_TeacherCode = @"SELECT COUNT(1) FROM [User] WHERE teacher_code = @TeacherCode";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query_TeacherCode, connection))
                {
                    command.Parameters.AddWithValue("@TeacherCode", teacher_code);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }

        public async Task<bool> InsertUser(UserResource resource)
        {
            const string queryPrefix = @"SELECT byte_code FROM SystemParam
                                        WHERE 1=1
                                        AND byte_desc_en = @Prefix
                                        OR byte_desc_th = @Prefix
                                        ";

            const string queryRole = @"SELECT byte_code FROM SystemParam
                                       WHERE 1=1
                                       AND byte_desc_en = @Role
                                       OR byte_desc_th = @Role";

            const string query = @"
                INSERT INTO [User] 
                (username, email, password, role, teacher_code, prefix, firstname, lastname, active_status, create_date, create_by) 
                VALUES (@Username, @Email, @password, @Role, @TeacherCode, @Prefix, @Firstname, @Lastname, @ActiveStatus, @CreateDate, @CreateBy)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string prefixValue = null;
                string roleValue = null;

                using (SqlCommand commandSelectPrefix = new SqlCommand(queryPrefix, connection))
                {
                    commandSelectPrefix.Parameters.AddWithValue("@Prefix", resource.prefix);
                    var result = await commandSelectPrefix.ExecuteScalarAsync();
                    if (result != null)
                    {
                        prefixValue = result.ToString();
                    }
                }

                using (SqlCommand commandSelectRole = new SqlCommand(queryRole, connection))
                {
                    commandSelectRole.Parameters.AddWithValue("@Role", resource.role);
                    var result = await commandSelectRole.ExecuteScalarAsync();
                    if (result != null)
                    {
                        roleValue = result.ToString();
                    }
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    string username = resource.email?.Split('@')[0] ?? string.Empty;

                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Email", resource.email);
                    command.Parameters.AddWithValue("@Password", resource.password);
                    command.Parameters.AddWithValue("@Role", roleValue);
                    command.Parameters.AddWithValue("@TeacherCode", resource.teacher_code);
                    command.Parameters.AddWithValue("@Prefix", prefixValue);
                    command.Parameters.AddWithValue("@Firstname", resource.firstname);
                    command.Parameters.AddWithValue("@Lastname", resource.lastname);
                    command.Parameters.AddWithValue("@ActiveStatus", resource.active_status);
                    command.Parameters.AddWithValue("@CreateDate", resource.create_date);
                    command.Parameters.AddWithValue("@CreateBy", resource.create_by);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
        }

        public async Task<bool> UpdateUserById(UserResource resource, string query)
        {

            const string queryPrefix = @"SELECT byte_code FROM SystemParam
                                        WHERE 1=1
                                        AND byte_desc_en = @Prefix
                                        OR byte_desc_th = @Prefix
                                        ";

            const string queryRole = @"SELECT byte_code FROM SystemParam
                                       WHERE 1=1
                                       AND byte_desc_en = @Role
                                       OR byte_desc_th = @Role";

            bool isSuccess = false;

            bool flg = false;
            int i = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    // ดึงค่าจาก queryPrefix
                    string prefixCode = null;
                    if (!string.IsNullOrWhiteSpace(resource.prefix))
                    {
                        using (SqlCommand cmd = new SqlCommand(queryPrefix, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Prefix", resource.prefix);
                            prefixCode = (string)await cmd.ExecuteScalarAsync();
                        }
                    }

                    // ดึงค่าจาก queryRole
                    string roleCode = null;
                    if (!string.IsNullOrWhiteSpace(resource.role))
                    {
                        using (SqlCommand cmd = new SqlCommand(queryRole, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Role", resource.role);
                            roleCode = (string)await cmd.ExecuteScalarAsync();
                        }
                    }


                    string UpdateQuery = $@"
                                           UPDATE [User]
                                           SET {query}
                                           WHERE 1=1
                                           AND [row_id] = @row_id
                                           AND [email] = @email
                                           ";

                    using (SqlCommand cmd = new SqlCommand(UpdateQuery, conn))
                    {
                        cmd.Transaction = tran;
                        cmd.Parameters.AddWithValue("row_id", resource.row_id);
                        cmd.Parameters.AddWithValue("@password", resource.password ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@role", roleCode ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@teacher_code", resource.teacher_code ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@prefix", prefixCode ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@firstname", resource.firstname ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@lastname", resource.lastname ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@email", resource.email ?? (object)DBNull.Value);
                        //cmd.Parameters.AddWithValue("@username", resource.username ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@active_status", resource.active_status ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@update_date", resource.update_date ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@update_by", resource.update_by ?? (object)DBNull.Value);

                        i = await cmd.ExecuteNonQueryAsync();
                        flg = i > 0;

                        if (flg)
                        {
                            tran.Commit();
                        }
                        else
                        {
                            tran.Rollback();
                        }
                    }
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
                finally
                {
                    await conn.CloseAsync();
                }
            }
            return flg;
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
