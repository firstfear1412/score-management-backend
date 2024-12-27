using Microsoft.Data.SqlClient;
using ScoreManagement.Interfaces;
using ScoreManagement.Model.Table;
using ScoreManagement.Model;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Entity;
using Microsoft.AspNetCore.Mvc;
using ScoreManagement.Model.SubjectScore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
                try
                {
                    // สร้าง query SQL แบบ dynamic
                    string query = $@"
                        UPDATE et
                        SET [subject] = @subject, body = @body
                        FROM [EmailTemplate] et
                        JOIN [UserEmailTemplate] ut ON ut.template_id = et.template_id AND ut.active_status = 'active'
                        WHERE et.template_id = @template_id AND ut.username = @username AND et.active_status = 'active'
                    ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@template_id", resource.template_id);
                        command.Parameters.AddWithValue("@subject", resource.subject);
                        command.Parameters.AddWithValue("@body", resource.body);
                        command.Parameters.AddWithValue("@username", resource.username);

                        i = await command.ExecuteNonQueryAsync();
                        flg = i == 0 ? false : true;
                        if (!flg)
                        {
                            throw new Exception("Failed to update EmailTemplate");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                
                await connection.CloseAsync();
            }

            return flg;
        }

        public async Task<bool> CreateTemplateEmail(EmailTemplateResource resource)
        {
            bool flg = false;
            int i = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                SqlTransaction tran = connection.BeginTransaction();
                try
                {
                    // สร้าง query SQL แบบ dynamic
                    string query1 = $@"
                        INSERT INTO [dbo].[EmailTemplate]
                           (
                           [template_name]
                           ,[is_private]
                           ,[subject]
                           ,[body]
                           ,[active_status]
                           ,[create_date]
                           ,[create_by]
                           ,[update_date]
                           ,[update_by])
                        VALUES(
                            @templateName
                            ,@isPrivate
                            ,@subject
                            ,@body
                            ,@activity_status
                            ,@createDate
                            ,@createBy
                            ,@updateDate
                            ,@updateBy
                        );
                        SELECT SCOPE_IDENTITY();
                    ";
                    string query2 = $@"
                        INSERT INTO [dbo].[UserEmailTemplate]
                                   ([username]
                                   ,[template_id]
                                   ,[active_status]
                                   ,[create_date]
                                   ,[create_by]
                                   ,[update_date]
                                   ,[update_by])
                        VALUES(
                            @username
                            ,@templateId
                            ,@activity_status
                            ,@createDate
                            ,@createBy
                            ,@updateDate
                            ,@updateBy
                        );
                    ";

                    using (var cmd1 = new SqlCommand(query1, connection, tran))
                    {
                        //cmd1.Parameters.AddWithValue("@templateId", resource.template_id);
                        cmd1.Parameters.AddWithValue("@templateName", resource.template_name);
                        cmd1.Parameters.AddWithValue("@isPrivate", true);
                        cmd1.Parameters.AddWithValue("@subject", resource.subject);
                        cmd1.Parameters.AddWithValue("@body", resource.body);
                        cmd1.Parameters.AddWithValue("@activity_status", "active");
                        cmd1.Parameters.AddWithValue("@createDate", DateTime.Now);
                        cmd1.Parameters.AddWithValue("@createBy", resource.username);
                        cmd1.Parameters.AddWithValue("@updateDate", DateTime.Now);
                        cmd1.Parameters.AddWithValue("@updateBy", resource.username);

                        //i = await cmd1.ExecuteNonQueryAsync();
                        //flg = i == 0 ? false : true;
                        //if (!flg)
                        //{
                        //    throw new Exception("Failed to insert into EmailTemplate");
                        //}
                        var result = await cmd1.ExecuteScalarAsync();
                        int templateId = Convert.ToInt32(result);
                        if(templateId == 0)
                        {
                            throw new Exception("Failed to retrive the generated template_id or insert into EmailTemplate failed");
                        }

                        using (var cmd2 = new SqlCommand(query2, connection, tran))
                        {
                            cmd2.Parameters.AddWithValue("@username", resource.username);
                            cmd2.Parameters.AddWithValue("@templateId", templateId);
                            cmd2.Parameters.AddWithValue("@activity_status", "active");
                            cmd2.Parameters.AddWithValue("@createDate", DateTime.Now);
                            cmd2.Parameters.AddWithValue("@createBy", resource.username);
                            cmd2.Parameters.AddWithValue("@updateDate", DateTime.Now);
                            cmd2.Parameters.AddWithValue("@updateBy", resource.username);

                            i = await cmd2.ExecuteNonQueryAsync();
                            flg = i == 0 ? false : true;
                            if (!flg)
                            {
                                throw new Exception("Failed to insert into UserEmailTemplate");
                            }
                        }
                    }
                    
                    await tran.CommitAsync();
                    return true;
                }
                catch (Exception ex) {
                    await tran.RollbackAsync();
                    throw;
                }
            }
        }
        public async Task<bool> DeleteTemplateEmail(EmailTemplateResource resource)
        {
            bool flg = false;
            int i = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    // สร้าง query SQL แบบ dynamic
                    string query = $@"
                        UPDATE et
                        SET active_status = @active_status
                        FROM [EmailTemplate] et
                        JOIN [UserEmailTemplate] ut ON ut.template_id = et.template_id AND ut.active_status = 'active'
                        WHERE et.template_id = @template_id AND ut.username = @username AND et.active_status = 'active'
                    ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@active_status", "inactive");
                        command.Parameters.AddWithValue("@template_id", resource.template_id);
                        command.Parameters.AddWithValue("@username", resource.username);

                        i = await command.ExecuteNonQueryAsync();
                        flg = i == 0 ? false : true;
                        if (!flg)
                        {
                            throw new Exception("Failed to delete EmailTemplate");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }

                await connection.CloseAsync();
            }

            return flg;
        }
        public async Task<bool> SetDefaultTemplateEmail(EmailTemplateResource resource)
        {
            bool flg = false;
            int i = 0;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    string checkQuery = @"
                        SELECT COUNT(*)
                        FROM UserDefaultEmailTemplate
                        WHERE [username] = @username AND [active_status] = 'active';
                    ";

                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@username", resource.username);
                        int count = (int)await checkCommand.ExecuteScalarAsync();

                        if (count == 0)
                        {
                            string insertQuery = @"
                                INSERT INTO UserDefaultEmailTemplate ( 
                                    [username], 
                                    [template_id],  
                                    [active_status],  
                                    [create_date],  
                                    [create_by],  
                                    [update_date],  
                                    [update_by]  
                                )  
                                VALUES  
                                (  
                                    @username,  
                                    @template_id,  
                                    'active',  
                                    GETDATE(),  
                                    @username,  
                                    GETDATE(),  
                                    @username
                                );
                            ";

                            using (var  insertCommand = new SqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@username", resource.username);
                                insertCommand.Parameters.AddWithValue("@template_id", resource.template_id);

                                i = await insertCommand.ExecuteNonQueryAsync();
                                flg = i > 0;
                            }
                        }
                        else
                        {
                            string updateQuery = @"
                                UPDATE UserDefaultEmailTemplate
                                SET template_id = @template_id,
                                    update_date = GETDATE(),
                                    update_by = @username
                                WHERE [username] = @username AND [active_status] = 'active';
                            ";

                            using(var updateCommand = new SqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@template_id", resource.template_id);
                                updateCommand.Parameters.AddWithValue("@username", resource.username);

                                i = await updateCommand.ExecuteNonQueryAsync();
                                flg = i > 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                await connection.CloseAsync();
            }
            return flg;
        }
        //public async Task<bool> SetDefaultTemplateEmail(EmailTemplateResource resource)
        //{
        //    bool flg = false;
        //    int i = 0;

        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        await connection.OpenAsync();
        //        try
        //        {
        //            // สร้าง query SQL แบบ dynamic
        //            string query = $@"
        //                UPDATE UserEmailTemplate
        //                SET is_default = CASE
        //                    WHEN template_id = @template_id THEN 1
        //                    ELSE 0
        //                END
        //                WHERE [username] = @username AND [active_status] = 'active'
        //            ";

        //            using (var command = new SqlCommand(query, connection))
        //            {
        //                command.Parameters.AddWithValue("@template_id", resource.template_id);
        //                command.Parameters.AddWithValue("@username", resource.username);

        //                i = await command.ExecuteNonQueryAsync();
        //                flg = i == 0 ? false : true;
        //                if (!flg)
        //                {
        //                    throw new Exception("Failed to update UserEmailTemplate");
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw;
        //        }

        //        await connection.CloseAsync();
        //    }

        //    return flg;
        //}

    }
}
