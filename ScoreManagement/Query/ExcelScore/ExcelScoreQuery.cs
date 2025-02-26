using ScoreManagement.Interfaces.ExcelScore;
using ScoreManagement.Model.ExcelScore;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Cmp;

namespace ScoreManagement.Query.ExcelScore
{
    public class ExcelScoreQuery : IExcelScore
    {
        private readonly string _connectionString;
        private readonly ILogger<ExcelScoreQuery> _logger;

        public ExcelScoreQuery(IConfiguration configuration, ILogger<ExcelScoreQuery> logger)
        {
            _connectionString = configuration.GetConnectionString("scoreDb")!;
            _logger = logger;
        }

        public async Task<List<ExcelScoreModel>> GetScoreReportAsync(ExcelScoreRequest request)
        {
            var results = new List<ExcelScoreModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                                    DECLARE @subject_id NVARCHAR(MAX) = @p_subject_id,
	                                    @academic_year NVARCHAR(MAX) = @p_academic_year,
	                                    @semester NVARCHAR(MAX) = @p_semester,
                                        @section NVARCHAR(MAX) = @p_section,
	                                    @score_type NVARCHAR(50) = @p_score_type;

                                    SELECT
                                       s.subject_id AS รหัสวิชา,
                                       s.subject_name AS ชื่อวิชา,
                                       shh.academic_year AS ปีการศึกษา,
                                       shh.semester AS ภาคเรียน,
                                       shh.section AS หมู่เรียน,
                                       CASE 
                                           WHEN @score_type = 'คะแนนรวม' THEN 'คะแนนรวม'
                                           WHEN @score_type = 'คะแนนปลายภาค' THEN 'คะแนนปลายภาค'
                                           WHEN @score_type = 'คะแนนกลางภาค' THEN 'คะแนนกลางภาค'
                                           WHEN @score_type = 'คะแนนระหว่างเรียน' THEN 'คะแนนระหว่างเรียน'
                                       END AS ประเภทคะแนน,
                                       COUNT(DISTINCT ss.student_id) AS จำนวนนิสิต,  -- นับจำนวนของนิสิตที่ไม่ซ้ำ
                                       ROUND(CONVERT(Decimal(10,2), AVG(score)), 2) AS คะแนนเฉลี่ย,
                                       ROUND(MIN(score), 2) AS คะแนนต่ำสุด,
                                       ROUND(MAX(score), 2) AS คะแนนสูงสุด,
                                       ROUND(STDEV(score), 2) AS ค่าเบี่ยงเบนมาตรฐาน,
                                       COUNT(DISTINCT CASE WHEN ss.score >= 0 AND ss.score < 40 THEN ss.student_id END) AS sum_0_39,  -- นับนิสิตในช่วงคะแนน 0-39
                                       COUNT(DISTINCT CASE WHEN ss.score >= 40 AND ss.score < 50 THEN ss.student_id END) AS sum_40_49,  -- นับนิสิตในช่วงคะแนน 40-49
                                       COUNT(DISTINCT CASE WHEN ss.score >= 50 AND ss.score < 60 THEN ss.student_id END) AS sum_50_59,  -- นับนิสิตในช่วงคะแนน 50-59
                                       COUNT(DISTINCT CASE WHEN ss.score >= 60 AND ss.score < 70 THEN ss.student_id END) AS sum_60_69,  -- นับนิสิตในช่วงคะแนน 60-69
                                       COUNT(DISTINCT CASE WHEN ss.score >= 70 AND ss.score < 80 THEN ss.student_id END) AS sum_70_79,  -- นับนิสิตในช่วงคะแนน 70-79
                                       COUNT(DISTINCT CASE WHEN ss.score >= 80 THEN ss.student_id END) AS count_80_plus  -- นับนิสิตในช่วงคะแนน 80 ขึ้นไป
                                   FROM 
                                       (SELECT 
                                           sys_subject_no, 
                                           student_id, 
                                           CASE 
                                               WHEN @score_type = 'คะแนนปลายภาค' THEN COALESCE(final_score, 0)
                                               WHEN @score_type = 'คะแนนกลางภาค' THEN COALESCE(midterm_score, 0)
                                               WHEN @score_type = 'คะแนนระหว่างเรียน' THEN COALESCE(accumulated_score, 0)
                                               WHEN @score_type = 'คะแนนรวม' THEN 
                                                   COALESCE(accumulated_score, 0) + COALESCE(midterm_score, 0) + COALESCE(final_score, 0)
                                           END AS score
                                       FROM subjectscore
                                       WHERE active_status = 'active') ss
                                   JOIN 
                                       (SELECT 
                                           sh.sys_subject_no, 
                                           sh.subject_id, 
                                           yrs.byte_desc_th AS academic_year,
                                           sem.byte_desc_th AS semester, 
                                           sec.byte_desc_th AS section
                                       FROM SubjectHeader sh
                                       JOIN SystemParam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                       JOIN SystemParam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                       JOIN SystemParam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section') shh
                                   ON ss.sys_subject_no = shh.sys_subject_no
                                   JOIN 
                                       (SELECT subject_id, subject_name 
                                       FROM Subject
                                       WHERE active_status = 'active') s
                                   ON s.subject_id = shh.subject_id
                                   WHERE 1=1
                                       AND (NULLIF(@subject_id, '') IS NULL OR s.subject_id = @subject_id)
                                       AND (NULLIF(@academic_year, '') IS NULL OR shh.academic_year = @academic_year)
                                       AND (NULLIF(@semester, '') IS NULL OR shh.semester = @semester)
                                       AND (NULLIF(@section, '') IS NULL OR shh.section = @section)
                                   GROUP BY s.subject_id, s.subject_name, shh.academic_year, shh.semester, shh.section;
                ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@p_subject_id", string.IsNullOrEmpty(request.subject_id) ? DBNull.Value : (object)request.subject_id);
                        command.Parameters.AddWithValue("@p_academic_year", string.IsNullOrEmpty(request.academic_year) ? DBNull.Value : (object)request.academic_year);
                        command.Parameters.AddWithValue("@p_semester", string.IsNullOrEmpty(request.semester) ? DBNull.Value : (object)request.semester);
                        command.Parameters.AddWithValue("@p_section", string.IsNullOrEmpty(request.section) ? DBNull.Value : (object)request.section);
                        command.Parameters.AddWithValue("@p_score_type", string.IsNullOrEmpty(request.score_type) ? DBNull.Value : (object)request.score_type);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    results.Add(new ExcelScoreModel
                                    {
                                        SubjectId = reader.IsDBNull(0) ? null : reader.GetString(0),
                                        SubjectName = reader.IsDBNull(1) ? null : reader.GetString(1),
                                        AcademicYear = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        Semester = reader.IsDBNull(3) ? null : reader.GetString(3),
                                        Section = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        ScoreType = reader.IsDBNull(5) ? null : reader.GetString(5),
                                        StudentCount = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                                        AverageScore = reader.IsDBNull(7) ? null : Convert.ToDecimal(reader.GetValue(7)),
                                        MinScore = reader.IsDBNull(8) ? null : Convert.ToDecimal(reader.GetValue(8)),
                                        MaxScore = reader.IsDBNull(9) ? null : Convert.ToDecimal(reader.GetValue(9)),
                                        StandardDeviation = reader.IsDBNull(10) ? null : reader.GetDouble(10),
                                        Sum0_39 = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                                        Sum40_49 = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                                        Sum50_59 = reader.IsDBNull(13) ? null : reader.GetInt32(13),
                                        Sum60_69 = reader.IsDBNull(14) ? null : reader.GetInt32(14),
                                        Sum70_79 = reader.IsDBNull(15) ? null : reader.GetInt32(15),
                                        Count80Plus = reader.IsDBNull(16) ? null : reader.GetInt32(16)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There is an error.");
                throw new Exception("There is an error.", ex);
            }
            return results;
        }
        public async Task<List<ExcelScoreModel_Other>> GetScoreReportAsync_Other(ExcelScoreRequest request)
        {
            var results = new List<ExcelScoreModel_Other>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                                    DECLARE @subject_id NVARCHAR(MAX) = @p_subject_id,
                                            @academic_year NVARCHAR(MAX) = @p_academic_year,
                                            @semester NVARCHAR(MAX) = @p_semester,
                                            @section NVARCHAR(MAX) = @p_section,
                                            @score_type NVARCHAR(50) = @p_score_type;

                                    WITH AverageScores AS (
                                        SELECT 
                                            sys_subject_no,
                                            AVG(
                                                CASE 
                                                    WHEN @score_type = 'คะแนนปลายภาค' THEN final_score
                                                    WHEN @score_type = 'คะแนนกลางภาค' THEN midterm_score
                                                    WHEN @score_type = 'คะแนนระหว่างเรียน' THEN accumulated_score
                                                    WHEN @score_type = 'คะแนนรวม' OR @score_type = '' THEN 
                                                        COALESCE(accumulated_score, 0) + 
                                                        COALESCE(midterm_score, 0) + 
                                                        COALESCE(final_score, 0)
                                                    ELSE NULL
                                                END
                                            ) AS avg_score
                                        FROM subjectscore
                                        WHERE active_status = 'active'
                                        GROUP BY sys_subject_no
                                    )
                                    SELECT
                                        s.subject_id AS รหัสวิชา,
                                        s.subject_name AS ชื่อวิชา,
                                        shh.academic_year AS ปีการศึกษา,
                                        shh.semester AS ภาคเรียน,
                                        shh.section AS หมู่เรียน,
                                        CASE 
                                            WHEN @score_type = '' THEN 'คะแนนรวม'
                                            ELSE @score_type 
                                        END AS ประเภทคะแนน,
                                        COUNT(DISTINCT ss.student_id) AS จำนวนนิสิต,
                                        ROUND(CONVERT(Decimal(10,2), AVG(ss.score)), 2) AS คะแนนเฉลี่ย,
                                        ROUND(MIN(ss.score), 2) AS คะแนนต่ำสุด,
                                        ROUND(MAX(ss.score), 2) AS คะแนนสูงสุด,
                                        ROUND(STDEV(ss.score), 2) AS ค่าเบี่ยงเบนมาตรฐาน,
                                        SUM(CASE WHEN ss.score > avg.avg_score THEN 1 ELSE 0 END) AS คะแนนมากกว่าค่าเฉลี่ย,
                                        SUM(CASE WHEN ss.score < avg.avg_score THEN 1 ELSE 0 END) AS คะแนนน้อยกว่าค่าเฉลี่ย
                                    FROM 
                                        (SELECT 
                                            sys_subject_no, 
                                            student_id, 
                                            CASE
                                                WHEN @score_type = 'คะแนนปลายภาค' THEN final_score
                                                WHEN @score_type = 'คะแนนกลางภาค' THEN midterm_score
                                                WHEN @score_type = 'คะแนนระหว่างเรียน' THEN accumulated_score
                                                WHEN @score_type = 'คะแนนรวม' OR @score_type = '' THEN 
                                                    COALESCE(accumulated_score, 0) + 
                                                    COALESCE(midterm_score, 0) + 
                                                    COALESCE(final_score, 0)
                                                ELSE NULL
                                            END AS score
                                        FROM subjectscore
                                        WHERE active_status = 'active'
                                        ) ss
                                    JOIN 
                                        (SELECT 
                                            sh.sys_subject_no, 
                                            sh.subject_id, 
                                            yrs.byte_desc_th AS academic_year,
                                            sem.byte_desc_th AS semester, 
                                            sec.byte_desc_th AS section
                                        FROM SubjectHeader sh
                                        JOIN SystemParam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                        JOIN SystemParam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                        JOIN SystemParam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section') shh
                                    ON ss.sys_subject_no = shh.sys_subject_no
                                    JOIN 
                                        (SELECT subject_id, subject_name 
                                        FROM Subject
                                        WHERE active_status = 'active') s
                                    ON s.subject_id = shh.subject_id
                                    LEFT JOIN AverageScores avg ON ss.sys_subject_no = avg.sys_subject_no
                                    WHERE 1=1
                                        AND (NULLIF(@subject_id, '') IS NULL OR s.subject_id = @subject_id)
                                        AND (NULLIF(@academic_year, '') IS NULL OR shh.academic_year = @academic_year)
                                        AND (NULLIF(@semester, '') IS NULL OR shh.semester = @semester)
                                        AND (NULLIF(@section, '') IS NULL OR shh.section = @section)
                                    GROUP BY s.subject_id, s.subject_name, shh.academic_year, shh.semester, shh.section, avg.avg_score;

                ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@p_subject_id", string.IsNullOrEmpty(request.subject_id) ? DBNull.Value : (object)request.subject_id);
                        command.Parameters.AddWithValue("@p_academic_year", string.IsNullOrEmpty(request.academic_year) ? DBNull.Value : (object)request.academic_year);
                        command.Parameters.AddWithValue("@p_semester", string.IsNullOrEmpty(request.semester) ? DBNull.Value : (object)request.semester);
                        command.Parameters.AddWithValue("@p_section", string.IsNullOrEmpty(request.section) ? DBNull.Value : (object)request.section);
                        command.Parameters.AddWithValue("@p_score_type", string.IsNullOrEmpty(request.score_type) ? DBNull.Value : (object)request.score_type);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    results.Add(new ExcelScoreModel_Other
                                    {
                                        SubjectId = reader.IsDBNull(0) ? null : reader.GetString(0),
                                        SubjectName = reader.IsDBNull(1) ? null : reader.GetString(1),
                                        AcademicYear = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        Semester = reader.IsDBNull(3) ? null : reader.GetString(3),
                                        Section = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        ScoreType = reader.IsDBNull(5) ? null : reader.GetString(5),
                                        StudentCount = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                                        AverageScore = reader.IsDBNull(7) ? null : Convert.ToDecimal(reader.GetValue(7)),
                                        MinScore = reader.IsDBNull(8) ? null : Convert.ToDecimal(reader.GetValue(8)),
                                        MaxScore = reader.IsDBNull(9) ? null : Convert.ToDecimal(reader.GetValue(9)),
                                        StandardDeviation = reader.IsDBNull(10) ? null : reader.GetDouble(10),
                                        Greater_than_avg = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                                        Lower_than_avg = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There is an error.");
                throw new Exception("There is an error.", ex);
            }
            return results;
        }
    }
}