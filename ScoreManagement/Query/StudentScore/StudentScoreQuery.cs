using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
using ScoreManagement.Model.ScoreAnnoucement;

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
        public async Task<PlaceholderMappingResponse> GetPlaceholderMapping(string placeholderKey)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT  source_table, field_name, condition FROM PlaceholderMapping WHERE placeholder_key = @placeholderKey AND active_status = 'active'";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@placeholderKey", placeholderKey);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            var md = new PlaceholderMappingResponse();
                            int col = 0;
                            int colNull = 0;
                            md.source_table = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.field_name = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            md.condition = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                            col = 0;
                            colNull = 0;
                            return md;
                        }
                    }
                }
            }
            return null; // คืนค่าเป็น null ถ้าไม่พบข้อมูล
        }

        public async Task<string> GetFieldValue(SubjectDetail subjectDetail, string studentId, string sourceTable, string fieldName, string condition, string username)
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
                    command.Parameters.AddWithValue("@student_id", studentId);
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

        public async Task<string> GetEmailStudent(string student_id)
        {
            string result = string.Empty;
            bool flg = false;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    string checkStudentQuery = @"
                                        SELECT email
                                        FROM Student
                                        WHERE [student_id] = @student_id
                                            AND [active_status] = 'active';
                                    ";

                    using (var checkStudentCommand = new SqlCommand(checkStudentQuery, connection))
                    {
                        checkStudentCommand.Parameters.AddWithValue("@student_id", student_id);
                        var email = await checkStudentCommand.ExecuteScalarAsync();
                        result = email.ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to excute query : {ex.Message}");
                }

                await connection.CloseAsync();
            }
            return result;
        }
        public async Task<bool> UpdateSendEmail(SubjectDetail resource, string studentId, string username, int send_status, string send_desc = "")
        {
            bool flg = false;
            int i = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    // สร้าง query SQL แบบ dynamic
                    string updateSubjectScoreQuery = @"
                        UPDATE ss
                        SET ss.send_status = @send_status,
                            ss.send_desc = @send_desc,
                            ss.update_date = GETDATE(),
                            ss.update_by = @username
                        FROM [SubjectScore] ss
                        INNER JOIN [SubjectHeader] sh
                            ON ss.[sys_subject_no] = sh.[sys_subject_no]
                        WHERE ss.[student_id] = @student_id
                          AND ss.[active_status] = 'active'
                          AND sh.[academic_year] = @academic_year
                          AND sh.[semester] = @semester
                          AND sh.[section] = @section
                          AND sh.[subject_id] = @subject_id
                          AND sh.[active_status] = 'active';
                    ";

                    using (var updateSubjectScoreCommand = new SqlCommand(updateSubjectScoreQuery, connection))
                    {
                        updateSubjectScoreCommand.Parameters.AddWithValue("@subject_id", resource.subject_id);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@academic_year", resource.academic_year);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@semester", resource.semester);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@section", resource.section);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@student_id", studentId);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@username", username);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@send_status", send_status);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@send_desc", string.IsNullOrWhiteSpace(send_desc) ? DBNull.Value : send_desc);

                        i = await updateSubjectScoreCommand.ExecuteNonQueryAsync();
                        flg = i > 0;
                        if (!flg)
                        {
                            throw new Exception("Failed to update SubjectScore");
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
                        if (templateId == 0)
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
                catch (Exception ex)
                {
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

                            using (var insertCommand = new SqlCommand(insertQuery, connection))
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

                            using (var updateCommand = new SqlCommand(updateQuery, connection))
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
        public async Task<(bool isSuccess, List<string> failedStudentIds)> UploadStudentScore(UploadScoreResource resource, string username)
        {
            List<string> failedStudentIds = new List<string>();
            object lockObject = new object(); // ใช้สำหรับล็อกการเข้าถึง failedStudentIds เมื่อใช้ Task whenAll
            bool flg = false;
            bool hasSuccess = false; // ตัวแปรเพื่อตรวจสอบว่า มี student ที่สำเร็จบ้างหรือไม่
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(); // เปิด Connection ครั้งเดียว
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // เรียกฟังก์ชัน TableQuery
                        await ExcuteSubjectQuery(connection, transaction, resource.subject, username);
                        int sysSubjectNo = await ExcuteSubjectHeaderQuery(connection, transaction, resource.subject, username);
                        await ExcuteSubjectLecturerQuery(connection, transaction, resource.subject, sysSubjectNo, username);

                        var tasks = resource.data.Select(async student =>
                        {
                            try
                            {
                                await ExcuteStudentQuery(connection, transaction, student, username);
                                await ExcuteSubjectScoreQuery(connection, transaction, student, sysSubjectNo, username);

                                // ถ้าทำงานสำเร็จ ให้ตั้งค่า flag ว่ามีการสำเร็จ
                                lock (lockObject)
                                {
                                    hasSuccess = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                lock (lockObject)
                                {
                                    failedStudentIds.Add(student.student_id);
                                }
                            }
                        });

                        await Task.WhenAll(tasks);

                        // ถ้าไม่มี student ที่สำเร็จเลย ให้ทำการ Rollback
                        if (!hasSuccess)
                        {
                            transaction.Rollback();
                            return (false, failedStudentIds);
                        }

                        // ถ้าทุกอย่างสำเร็จ ให้ Commit Transaction
                        transaction.Commit(); // Commit Transaction
                        flg = true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Rollback ถ้ามีข้อผิดพลาด
                        Console.WriteLine("Transaction Failed: " + ex.Message);
                        throw;
                    }
                }
            }
            return (flg, failedStudentIds);
        }
        private async Task ExcuteSubjectQuery(SqlConnection connection, SqlTransaction transaction, SubjectDetailUpload subject, string username)
        {
            int i = 0;
            bool flg = false;
            string checkSubjectQuery = @"
                SELECT subject_name
                FROM Subject
                WHERE subject_id = @subject_id
            ";
            using (var checkSubjectCommand = new SqlCommand(checkSubjectQuery, connection, transaction))
            {
                checkSubjectCommand.Parameters.AddWithValue("@subject_id", subject.subject_id);
                var existingSubjectName = await checkSubjectCommand.ExecuteScalarAsync() as string;
                if (existingSubjectName != null)
                {
                    if (existingSubjectName != subject.subject_name)
                    {
                        string updateSubjectQuery = @"
                            UPDATE Subject
                            SET subject_name = @subject_name,
                                active_status = 'active',
                                update_date = GETDATE(),
                                update_by = @username
                            WHERE subject_id = @subject_id
                        ";

                        using (var updateSubjectCommand = new SqlCommand(updateSubjectQuery, connection, transaction))
                        {
                            updateSubjectCommand.Parameters.AddWithValue("@subject_id", subject.subject_id);
                            updateSubjectCommand.Parameters.AddWithValue("@subject_name", subject.subject_name);
                            updateSubjectCommand.Parameters.AddWithValue("@username", username);

                            int rowsAffected = await updateSubjectCommand.ExecuteNonQueryAsync();
                            flg = rowsAffected > 0;
                            if (!flg)
                            {
                                throw new Exception($"Failed to update Subject with subject_id '{subject.subject_id}'");
                            }
                        }
                    }
                    else
                    {
                        //skip function
                    }
                }
                else // ถ้าไม่มีข้อมูลในฐานข้อมูลให้ทำการ INSERT
                {
                    string insertSubjectQuery = @"
                        INSERT INTO Subject (
                            [subject_id], 
                            [subject_name], 
                            [active_status], 
                            [create_date], 
                            [create_by], 
                            [update_date], 
                            [update_by]
                        )
                        VALUES (
                            @subject_id, 
                            @subject_name, 
                            'active', 
                            GETDATE(), 
                            @username, 
                            GETDATE(), 
                            @username
                        );
                    ";

                    using (var insertSubjectCommand = new SqlCommand(insertSubjectQuery, connection, transaction))
                    {
                        insertSubjectCommand.Parameters.AddWithValue("@subject_id", subject.subject_id);
                        insertSubjectCommand.Parameters.AddWithValue("@subject_name", subject.subject_name);
                        insertSubjectCommand.Parameters.AddWithValue("@username", username);

                        i = await insertSubjectCommand.ExecuteNonQueryAsync();
                        flg = i > 0;
                        if (!flg)
                        {
                            throw new Exception($"Failed to insert into Subject with subject_id '{subject.subject_id}'");
                        }
                    }
                }
            }
        }
        private async Task<int> ExcuteSubjectHeaderQuery(SqlConnection connection, SqlTransaction transaction, SubjectDetailUpload subject, string username)
        {
            int? sysSubjectNo = null;
            string checkSubjectHeaderQuery = @"
                            SELECT sh.[sys_subject_no]
                            FROM SubjectHeader sh
                            WHERE sh.[subject_id] = @subject_id
                                AND sh.[semester] = @semester
                                AND sh.[section] = @section
                                AND sh.[academic_year] = @academic_year
                                AND sh.[active_status] = 'active';
                        ";
            using (var checkCommand = new SqlCommand(checkSubjectHeaderQuery, connection, transaction))
            {
                checkCommand.Parameters.AddWithValue("@subject_id", subject.subject_id);
                checkCommand.Parameters.AddWithValue("@semester", subject.semester);
                checkCommand.Parameters.AddWithValue("@section", subject.section);
                checkCommand.Parameters.AddWithValue("@academic_year", subject.academic_year);
                using (var reader = await checkCommand.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        sysSubjectNo = reader["sys_subject_no"] as int?;
                    }
                }

                if (sysSubjectNo == null)
                {
                    string insertSubjectHeaderQuery = @"
                                    INSERT INTO SubjectHeader(
                                        [subject_id]
                                        ,[academic_year]
                                        ,[semester]
                                        ,[section]
                                        ,[active_status]
                                        ,[create_date]
                                        ,[create_by]
                                        ,[update_date]
                                        ,[update_by] 
                                    )  
                                    VALUES  
                                    (  
                                        @subject_id,
                                        @academic_year,
                                        @semester,
                                        @section,
                                        'active',
                                        GETDATE(),  
                                        @username,  
                                        GETDATE(),  
                                        @username
                                    );
                                    SELECT SCOPE_IDENTITY();
                                "
                    ;

                    using (var insertSubjectHeaderCommand = new SqlCommand(insertSubjectHeaderQuery, connection, transaction))
                    {
                        insertSubjectHeaderCommand.Parameters.AddWithValue("@subject_id", subject.subject_id);
                        insertSubjectHeaderCommand.Parameters.AddWithValue("@academic_year", subject.academic_year);
                        insertSubjectHeaderCommand.Parameters.AddWithValue("@semester", subject.semester);
                        insertSubjectHeaderCommand.Parameters.AddWithValue("@section", subject.section);
                        insertSubjectHeaderCommand.Parameters.AddWithValue("@username", username);

                        var result = await insertSubjectHeaderCommand.ExecuteScalarAsync();
                        sysSubjectNo = Convert.ToInt32(result);
                        if (sysSubjectNo == 0)
                        {
                            throw new Exception("Failed to retrive the generated sys_subject_no or insert into SubjectHeader failed");
                        }
                    }
                }
                else
                {
                    // มี sysSubjectNo เดิมอยู่แล้วไม่ต้องทำอะไร
                }
            }
            return (int)sysSubjectNo;
        }
        private async Task ExcuteSubjectLecturerQuery(SqlConnection connection, SqlTransaction transaction, SubjectDetailUpload subject, int sysSubjectNo, string username)
        {
            int i = 0;
            bool flg = false;
            // Step 1: SELECT teacher_code ของ sys_subject_no
            var existingTeachers = new List<string>();
            string existingTeachersQuery = @"
                            SELECT teacher_code
                            FROM SubjectLecturer
                            WHERE [sys_subject_no] = @sys_subject_no;
                        ";
            using (var existingTeachersCommand = new SqlCommand(existingTeachersQuery, connection, transaction))
            {
                existingTeachersCommand.Parameters.AddWithValue("@sys_subject_no", sysSubjectNo);
                using (var reader = await existingTeachersCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int col = 0;
                        existingTeachers.Add(reader.IsDBNull(col) ? null : reader.GetString(col++));
                    }
                }
            }
            // Step 2: UPDATE active_status เป็น inactive สำหรับ sys_subject_no ในกรณีที่มี teacher เดิมอยู่แล้ว
            if (existingTeachers.Any()) // ตรวจสอบว่ามี existingTeachers หรือไม่
            {
                string updateSubjectLecturerQuery = @"
                                UPDATE SubjectLecturer
                                SET active_status = 'inactive'
                                WHERE sys_subject_no = @sys_subject_no
                            ";
                using (var updateSubjectLecturerCommand = new SqlCommand(updateSubjectLecturerQuery, connection, transaction))
                {
                    updateSubjectLecturerCommand.Parameters.AddWithValue("@sys_subject_no", sysSubjectNo);
                    i = await updateSubjectLecturerCommand.ExecuteNonQueryAsync();
                    flg = i > 0;
                    if (!flg)
                    {
                        throw new Exception("Failed to update SubjectLecturer for old teacher_code");
                    }
                }
            }
            else
            {
                // กรณีไม่มี existingTeachers ข้ามการทำงาน
            }
            // Step 3: INSERT ค่า teacher_code ใหม่ที่ยังไม่มี
            string insertSubjectLecturerQuery = @"
                            INSERT INTO SubjectLecturer(
                                        [sys_subject_no]
                                        ,[teacher_code]
                                        ,[active_status]
                                        ,[create_date]
                                        ,[create_by]
                                        ,[update_date]
                                        ,[update_by] 
                                    )  
                                    VALUES  
                                    (  
                                        @sys_subject_no,
                                        @teacher_code,
                                        'active',
                                        GETDATE(),  
                                        @username,  
                                        GETDATE(),  
                                        @username
                                    );
                        ";

            string updateExistingSubjectLecturerQuery = @"
                                    UPDATE SubjectLecturer
                                    SET active_status = 'active'
                                    WHERE sys_subject_no = @sys_subject_no
                                        AND teacher_code = @teacher_code
                                        AND teacher_code = @teacher_code
                                ";

            foreach (var teacherCode in subject.teacher)
            {
                if (!existingTeachers.Contains(teacherCode))
                {
                    using (var insertSubjectHeaderCommand = new SqlCommand(insertSubjectLecturerQuery, connection, transaction))
                    {
                        insertSubjectHeaderCommand.Parameters.AddWithValue("@sys_subject_no", sysSubjectNo);
                        insertSubjectHeaderCommand.Parameters.AddWithValue("@teacher_code", teacherCode);
                        insertSubjectHeaderCommand.Parameters.AddWithValue("@username", username);

                        i = await insertSubjectHeaderCommand.ExecuteNonQueryAsync();
                        flg = i > 0;
                        if (!flg)
                        {
                            throw new Exception($"Failed to insert into SubjectLecturer for teacher_code '{teacherCode}'.");
                        }
                    }
                }
                else
                {

                    using (var updateExistingSubjectLecturerCommand = new SqlCommand(updateExistingSubjectLecturerQuery, connection, transaction))
                    {
                        updateExistingSubjectLecturerCommand.Parameters.AddWithValue("@sys_subject_no", sysSubjectNo);
                        updateExistingSubjectLecturerCommand.Parameters.AddWithValue("@teacher_code", teacherCode);
                        i = await updateExistingSubjectLecturerCommand.ExecuteNonQueryAsync();
                        flg = i > 0;
                        if (!flg)
                        {
                            throw new Exception($"Failed to update SubjectLecturer for teacher_code '{teacherCode}' to 'active'.");
                        }
                    }
                }
            }

        }
        private async Task ExcuteStudentQuery(SqlConnection connection, SqlTransaction transaction, ScoreStudent student, string username)
        {
            int i = 0;
            bool flg = false;
            string checkStudentQuery = @"
                            SELECT COUNT(*)
                            FROM Student
                            WHERE [student_id] = @student_id;
                        ";

            using (var checkStudentCommand = new SqlCommand(checkStudentQuery, connection, transaction))
            {
                checkStudentCommand.Parameters.AddWithValue("@student_id", student.student_id);
                int countStudent = (int)await checkStudentCommand.ExecuteScalarAsync();

                if (countStudent == 0)
                {
                    string insertStudentQuery = @"
                                    INSERT INTO Student ( 
                                        [student_id], 
                                        [prefix],  
                                        [firstname],  
                                        [lastname],  
                                        [major_code],  
                                        [email],
                                        [active_status],
                                        [create_date],
                                        [create_by],
                                        [update_date],
                                        [update_by]  
                                    )  
                                    VALUES  
                                    (  
                                        @student_id,  
                                        (SELECT byte_code 
                                        FROM SystemParam 
                                        WHERE byte_reference = 'prefix' 
                                            AND byte_desc_th = @prefix 
                                            AND active_status = 'active'),
                                        @firstname,
                                        @lastname,
                                        (SELECT byte_code 
                                        FROM SystemParam 
                                        WHERE byte_reference = 'major_code' 
                                            AND byte_desc_th = @major_code 
                                            AND active_status = 'active'),
                                        @email,
                                        'active',
                                        GETDATE(),  
                                        @username,  
                                        GETDATE(),  
                                        @username
                                    );
                                ";

                    using (var insertStudentCommand = new SqlCommand(insertStudentQuery, connection, transaction))
                    {
                        insertStudentCommand.Parameters.AddWithValue("@student_id", student.student_id);
                        insertStudentCommand.Parameters.AddWithValue("@prefix", student.prefix);
                        insertStudentCommand.Parameters.AddWithValue("@firstname", student.firstname);
                        insertStudentCommand.Parameters.AddWithValue("@lastname", student.lastname);
                        insertStudentCommand.Parameters.AddWithValue("@major_code", student.major_code);
                        insertStudentCommand.Parameters.AddWithValue("@email", student.email);
                        insertStudentCommand.Parameters.AddWithValue("@username", username);

                        i = await insertStudentCommand.ExecuteNonQueryAsync();
                        flg = i > 0;
                        if (!flg)
                        {
                            throw new Exception($"Failed to insert into Student for StudentID '{student.student_id}'");
                        }
                    }
                }
                else
                {
                    string updateStudentQuery = @"
                                    UPDATE  Student
                                    SET 
                                        [prefix] = (
                                            SELECT byte_code
                                            FROM SystemParam
                                            WHERE byte_reference = 'prefix'
                                            AND byte_desc_th = @prefix
                                            AND active_status = 'active'
                                        ),
                                        [firstname] = @firstname,
                                        [lastname] = @lastname, 
                                        [major_code] = (
                                            SELECT byte_code
                                            FROM SystemParam
                                            WHERE byte_reference = 'major_code'
                                            AND byte_desc_th = @major_code
                                            AND active_status = 'active'
                                        ),  
                                        [email] = @email,
                                        [active_status] = 'active',
                                        [update_date] = GETDATE(),
                                        [update_by] = @username
                                    WHERE
                                        [student_id] = @student_id;
                                ";

                    using (var updateStudentCommand = new SqlCommand(updateStudentQuery, connection, transaction))
                    {
                        updateStudentCommand.Parameters.AddWithValue("@student_id", student.student_id);
                        updateStudentCommand.Parameters.AddWithValue("@prefix", student.prefix);
                        updateStudentCommand.Parameters.AddWithValue("@firstname", student.firstname);
                        updateStudentCommand.Parameters.AddWithValue("@lastname", student.lastname);
                        updateStudentCommand.Parameters.AddWithValue("@major_code", student.major_code);
                        updateStudentCommand.Parameters.AddWithValue("@email", student.email);
                        updateStudentCommand.Parameters.AddWithValue("@username", username);

                        i = await updateStudentCommand.ExecuteNonQueryAsync();
                        flg = i > 0;
                        if (!flg)
                        {
                            throw new Exception($"Failed to update Student for StudentID '{student.student_id}'");
                        }
                    }
                }
            }
        }
        private async Task ExcuteSubjectScoreQuery(SqlConnection connection, SqlTransaction transaction, ScoreStudent student, int sysSubjectNo, string username)
        {
            int i = 0;
            bool flg = false;
            string checkSubjectScoreQuery = @"
                            SELECT COUNT(*)
                            FROM SubjectScore
                            WHERE [student_id] = @student_id
                                AND [sys_subject_no] = @sys_subject_no
                                AND [active_status] = 'active';
                        ";

            using (var checkSubjectScoreCommand = new SqlCommand(checkSubjectScoreQuery, connection, transaction))
            {
                checkSubjectScoreCommand.Parameters.AddWithValue("@student_id", student.student_id);
                checkSubjectScoreCommand.Parameters.AddWithValue("@sys_subject_no", sysSubjectNo);
                int countStudent = (int)await checkSubjectScoreCommand.ExecuteScalarAsync();

                if (countStudent == 0)
                {
                    string insertSubjectScoreQuery = @"
                                    INSERT INTO SubjectScore ( 
                                        [sys_subject_no], 
                                        [student_id],  
                                        [seat_no],  
                                        [accumulated_score],
                                        [midterm_score],
                                        [final_score],
                                        [active_status],
                                        [create_date],
                                        [create_by],
                                        [update_date],
                                        [update_by]  
                                    )  
                                    VALUES  
                                    (  
                                        @sys_subject_no,
                                        @student_id,
                                        @seat_no,
                                        @accumulated_score,
                                        @midterm_score,
                                        @final_score,
                                        'active',
                                        GETDATE(),  
                                        @username,  
                                        GETDATE(),  
                                        @username
                                    );
                                ";

                    using (var insertSubjectCommand = new SqlCommand(insertSubjectScoreQuery, connection, transaction))
                    {
                        insertSubjectCommand.Parameters.AddWithValue("@sys_subject_no", sysSubjectNo);
                        insertSubjectCommand.Parameters.AddWithValue("@student_id", student.student_id);
                        insertSubjectCommand.Parameters.AddWithValue("@seat_no", student.seat_no);
                        insertSubjectCommand.Parameters.AddWithValue("@accumulated_score", (object)student.accumulated_score! ?? DBNull.Value);
                        insertSubjectCommand.Parameters.AddWithValue("@midterm_score", (object)student.midterm_score! ?? DBNull.Value);
                        insertSubjectCommand.Parameters.AddWithValue("@final_score", (object)student.final_score! ?? DBNull.Value);
                        insertSubjectCommand.Parameters.AddWithValue("@username", username);

                        i = await insertSubjectCommand.ExecuteNonQueryAsync();
                        flg = i > 0;
                        if (!flg)
                        {
                            throw new Exception($"Failed to insert into SubjectScore for sys_subject_no '{sysSubjectNo}'");
                        }
                    }
                }
                else
                {
                    string updateSubjectScoreQuery = @"
                                    UPDATE [SubjectScore]
                                    SET seat_no = @seat_no,
                                        accumulated_score = @accumulated_score,
                                        midterm_score = @midterm_score,
                                        final_score = @final_score,
                                        create_date = GETDATE(),
                                        create_by = @username,
                                        update_date = GETDATE(),
                                        update_by = @username
                                    WHERE [student_id] = @student_id
                                        AND [sys_subject_no] = @sys_subject_no
                                        AND [active_status] = 'active'
                                ";

                    using (var updateSubjectScoreCommand = new SqlCommand(updateSubjectScoreQuery, connection, transaction))
                    {
                        updateSubjectScoreCommand.Parameters.AddWithValue("@sys_subject_no", sysSubjectNo);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@student_id", student.student_id);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@seat_no", student.seat_no);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@accumulated_score", (object)student.accumulated_score! ?? DBNull.Value);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@midterm_score", (object)student.midterm_score! ?? DBNull.Value);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@final_score", (object)student.final_score! ?? DBNull.Value);
                        updateSubjectScoreCommand.Parameters.AddWithValue("@username", username);

                        i = await updateSubjectScoreCommand.ExecuteNonQueryAsync();
                        flg = i > 0;
                        if (!flg)
                        {
                            throw new Exception($"Failed to update SubjectScore for sys_subject_no '{sysSubjectNo}'");
                        }
                    }
                }
            }
        }
        public async Task<List<ScoreAnnoucementResource>> GetScoreAnnoucementByConditionQuery(ScoreAnnoucementResource resource)
        {
            var scoreAnnoucementList = new List<ScoreAnnoucementResource>();

            string query = @"
                            SELECT
                                ss.seat_no, 
                                sh.sys_subject_no, 
                                sh.subject_id, 
                                sh.academic_year, 
                                sh.semester, 
                                sh.[section],
                                ss.student_id,
                                s.prefix as prefix_code,
                                spp.byte_desc_th as prefix_desc_th,
                                spp.byte_desc_en as prefix_desc_en,
                                s.firstname,
                                s.lastname,
                                (
                                SELECT byte_desc_th 
                                FROM SystemParam sp 
                                WHERE sp.byte_reference = 'major_code' 
                                    AND sp.byte_code = s.major_code 
                                    AND sp.active_status = 'active') AS major_code ,
                                ss.accumulated_score, 
                                ss.midterm_score, 
                                ss.final_score, 
                                ss.send_status as send_status_code,                        
                                sps.byte_desc_th as send_status_code_desc_th,
                                sps.byte_desc_en as send_status_code_desc_en,
                                ss.send_desc,
                                s.email
                            FROM SubjectHeader sh
                            INNER JOIN (
	                            SELECT sys_subject_no, MIN(teacher_code) AS teacher_code
	                            FROM SubjectLecturer
	                            WHERE active_status = 'active'
	                            AND (@teacherCode IS NULL OR teacher_code = @teacherCode)
	                            GROUP BY sys_subject_no
                            ) sl
                            ON sh.sys_subject_no = sl.sys_subject_no
                            INNER JOIN SubjectScore ss
                                ON sh.sys_subject_no = ss.sys_subject_no
                            INNER JOIN Student s
                                ON ss.student_id = s.student_id
                            INNER JOIN Subject sj
                                ON sj.subject_id = sh.subject_id
                            LEFT JOIN SystemParam spp 
                                ON s.prefix = spp.byte_code AND spp.byte_reference = 'prefix'
                            LEFT JOIN SystemParam sps
                                ON ss.send_status = sps.byte_code AND sps.byte_reference = 'send_status'
    ";

            // List to dynamically store conditions
            var conditions = new List<string>();

            if (resource.role == 2)
            {
                if (!string.IsNullOrEmpty(resource.teacher_code))
                {
                    conditions.Add("sl.teacher_code = @teacherCode");
                }
            }
            // Add conditions based on the provided inputs
            if (!string.IsNullOrEmpty(resource.subjectSearch))
            {
                conditions.Add("CONCAT(sh.subject_id,' ', sj.subject_name ) LIKE '%' + @subjectSearch + '%'");
            }

            if (!string.IsNullOrEmpty(resource.section))
            {
                conditions.Add("sh.[section] = @section");
            }

            if (resource.semester != null)
            {
                conditions.Add("sh.semester = @semester");
            }

            if (!string.IsNullOrEmpty(resource.academic_year))
            {
                conditions.Add("sh.academic_year = @academic_year");
            }

            if (!string.IsNullOrEmpty(resource.send_status_code))
            {
                conditions.Add("ss.send_status = @send_status_code");
            }

            if (!string.IsNullOrEmpty(resource.studentSearch))
            {
                conditions.Add("CONCAT(spp.byte_desc_th, ' ', s.firstname, ' ', s.lastname, ss.student_id) COLLATE DATABASE_DEFAULT LIKE '%' + @studentSearch + '%'");
            }

            // Only add WHERE if there are conditions
            if (conditions.Any())
            {
                query += " WHERE " + string.Join(" AND ", conditions);
            }
            query += " ORDER BY LEFT(ss.seat_no, PATINDEX('%[0-9]%', ss.seat_no + '0') - 1),TRY_CAST(SUBSTRING(ss.seat_no, PATINDEX('%[0-9]%', ss.seat_no + '0'), LEN(ss.seat_no)) AS INT)";
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        command.Parameters.AddWithValue("@subjectSearch", (object)resource.subjectSearch ?? DBNull.Value);
                        command.Parameters.AddWithValue("@teacherCode", resource.role == 2 ? (object)resource.teacher_code : DBNull.Value);
                        command.Parameters.AddWithValue("@section", (object)resource.section ?? DBNull.Value);
                        command.Parameters.AddWithValue("@semester", (object)resource.semester ?? DBNull.Value);
                        command.Parameters.AddWithValue("@academic_year", (object)resource.academic_year ?? DBNull.Value);
                        command.Parameters.AddWithValue("@send_status_code", (object)resource.send_status_code ?? DBNull.Value);
                        command.Parameters.AddWithValue("@studentSearch", (object)resource.studentSearch ?? DBNull.Value);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var result = new ScoreAnnoucementResource
                                {
                                    sys_subject_no = reader["sys_subject_no"] as int? ?? default,
                                    subject_id = reader["subject_id"]?.ToString(),
                                    academic_year = reader["academic_year"]?.ToString(),
                                    semester = reader["semester"] as int? ?? default,
                                    section = reader["section"]?.ToString(),
                                    student_id = reader["student_id"]?.ToString(),
                                    prefix_code = reader["prefix_code"]?.ToString(),
                                    prefix_desc_th = reader["prefix_desc_th"]?.ToString(),
                                    prefix_desc_en = reader["prefix_desc_en"]?.ToString(),
                                    firstname = reader["firstname"]?.ToString(),
                                    lastname = reader["lastname"]?.ToString(),
                                    major_code = reader["major_code"]?.ToString(),
                                    seat_no = reader["seat_no"]?.ToString(),
                                    accumulated_score = reader["accumulated_score"] as decimal? ?? null,
                                    midterm_score = reader["midterm_score"] as decimal? ?? null,
                                    final_score = reader["final_score"] as decimal? ?? null,
                                    send_status_code = reader["send_status_code"]?.ToString(),
                                    send_status_code_desc_th = reader["send_status_code_desc_th"]?.ToString(),
                                    send_status_code_desc_en = reader["send_status_code_desc_en"]?.ToString(),
                                    send_desc = reader["send_desc"]?.ToString(),
                                    email = reader["email"]?.ToString()
                                };
                                scoreAnnoucementList.Add(result);
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

            return scoreAnnoucementList;
        }
        public async Task<bool> DeleteScoreQuery(ScoreAnnoucementResource resource)
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
                        DELETE FROM ScoreManagement.dbo.SubjectScore
                        WHERE sys_subject_no = @sysSubjectNo AND student_id = @studendId;
                    ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sysSubjectNo", resource.sys_subject_no);
                        command.Parameters.AddWithValue("@studendId", resource.student_id);

                        i = await command.ExecuteNonQueryAsync();
                        flg = i == 0 ? false : true;
                        if (!flg)
                        {
                            throw new Exception("Failed to Delete");
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
        public async Task<int> InsertNotification(NotificationResource resource)
        {
            bool flg = false;
            //int i = 0;
            var notifyId = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    string insertNotifyQuery = @"
                                        INSERT INTO Notification ( 
                                            [username],
                                            [template_id],
                                            [data],
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
                                            @data,
                                            'active',
                                            GETDATE(),  
                                            @username,  
                                            GETDATE(),  
                                            @username
                                        );
                                        SELECT SCOPE_IDENTITY();
                                    ";

                    using (var insertNotifyCommand = new SqlCommand(insertNotifyQuery, connection))
                    {
                        insertNotifyCommand.Parameters.AddWithValue("@username", resource.username);
                        insertNotifyCommand.Parameters.AddWithValue("@template_id", resource.templateId);
                        insertNotifyCommand.Parameters.AddWithValue("@data", resource.data);

                        var result = await insertNotifyCommand.ExecuteScalarAsync();
                        notifyId = Convert.ToInt32(result);
                        if (notifyId == 0)
                        {
                            throw new Exception("Failed to retrive the generated notification_id or insert into Notification failed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                return notifyId;
            }
        }
        public async Task<NotificationResponse<string>> GetLatestNotification(int notificationId)
        {
            //string result = string.Empty;
            //bool flg = false;
            NotificationResponse<string> notification = new NotificationResponse<string>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    string LatestNotifyQuery = @"
                                        SELECT TOP 1 [notification_id]
                                          ,[template_id]
                                          ,[data]
                                          ,[create_date]
                                        FROM Notification
                                        WHERE [notification_id] = @notification_id
                                            AND [active_status] = 'active';
                                    ";

                    using (var checkCommand = new SqlCommand(LatestNotifyQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@notification_id", notificationId);
                        using (var reader = await checkCommand.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                int col = 0;
                                int colNull = 0;
                                notification.notificationId = !reader.IsDBNull(colNull++) ? reader.GetInt32(col) : default; col++;
                                notification.templateId = !reader.IsDBNull(colNull++) ? reader.GetInt32(col) : default; col++;
                                notification.data = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; col++;
                                notification.createDate = !reader.IsDBNull(colNull++) ? reader.GetDateTime(col) : (DateTime?)null; col++;
                                col = 0;
                                colNull = 0;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to excute query : {ex.Message}");
                }

                await connection.CloseAsync();
            }
            return notification;
        }
    }

}
