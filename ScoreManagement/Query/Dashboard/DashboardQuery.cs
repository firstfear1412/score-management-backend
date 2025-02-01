using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces.Dashboard;
using ScoreManagement.Model;
using ScoreManagement.Model.ScoreAnnoucement;

using static DashboardStatisticsResponse;

namespace ScoreManagement.Query.Dashboard
{
    public class DashboardQuery : IDashboardQuery
    {
        private readonly scoreDB _context;
        private readonly string _connectionString;

        public DashboardQuery(IConfiguration configuration, scoreDB context)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("scoreDb")!;
        }

        public async Task<object> GetDashboardStatistics(ScoreAnnoucementDashboard resource)
        {
            Dictionary<string, string> scoreTypeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "คะแนนกลางภาค", "midterm_score" },
                { "คะแนนปลายภาค", "final_score" },
                { "คะแนนระหว่างเรียน", "accumulated_score" },
                { "คะแนนนิสิต" , "student_score"},
                { "คะแนนรวม", "total_score" }
            };

            Dictionary<string, string> queries = new Dictionary<string, string>
            {
                { "final_score", @"
                                 SELECT
                                    ROUND(MAX(ss.final_score), 2) AS max_final_score, 
                                    ROUND(MIN(ss.final_score),2) AS min_final_score, 
                                    ROUND(CONVERT(DECIMAL(10,2), AVG(ss.final_score)), 2) AS avg_final_score, 
                                    ROUND(STDEV(ss.final_score), 2) AS std_final_score, 
                                    COUNT(DISTINCT ss.student_id) AS number_student
                                FROM 
                                    (SELECT 
                                        sys_subject_no, 
                                        student_id, 
                                        seat_no, 
                                        accumulated_score, 
                                        midterm_score, 
                                        final_score
                                    FROM subjectscore
                                    WHERE active_status = 'active') ss
                                JOIN 
                                    (SELECT 
                                        sh.sys_subject_no, 
                                        sh.subject_id, 
                                        yrs.byte_desc_th AS academic_year,
                                        sem.byte_desc_th AS semester, 
                                        sec.byte_desc_th AS section
                                    FROM subjectheader sh
                                    JOIN systemparam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                    JOIN systemparam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                    JOIN systemparam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section'
                                    WHERE sh.active_status = 'active') shh
                                    ON ss.sys_subject_no = shh.sys_subject_no
                                JOIN 
                                    (SELECT subject_id, subject_name 
                                    FROM subject
                                    WHERE active_status = 'active') s
                                    ON s.subject_id = shh.subject_id
                                WHERE 1=1
                                AND (@subject_id IS NULL OR s.subject_id = NULLIF(@subject_id, ''))
                                AND (@academic_year IS NULL OR shh.academic_year = NULLIF(@academic_year, ''))
                                AND (@semester IS NULL OR shh.semester = NULLIF(@semester, ''))
                                AND (@section IS NULL OR shh.section = NULLIF(@section, ''))
                                AND (@teacher_code IS NULL OR @teacher_code = '' OR EXISTS (
                                          SELECT 1 
                                          FROM SubjectLecturer sl 
                                          WHERE sl.sys_subject_no = shh.sys_subject_no 
                                            AND sl.teacher_code = NULLIF(@teacher_code, '')
                                            AND sl.active_status = 'active'
                                      )) " },

                { "midterm_score", @"
                    SELECT  ROUND(MAX(ss.midterm_score),2) AS MAX_MIDTERM_SCORE, 
                            ROUND(MIN(ss.midterm_score),2) AS MIN_MIDTERM_SCORE, 
                            ROUND(CONVERT(DECIMAL(10,2), AVG(ss.midterm_score)),2) AS AVG_MIDTERM_SCORE, 
                            ROUND(STDEV(ss.midterm_score), 2) AS STD_MIDTERM_SCORE, 
                                 COUNT(DISTINCT ss.student_id) AS number_student
                                FROM 
                                    (SELECT 
                                        sys_subject_no, 
                                        student_id, 
                                        seat_no, 
                                        accumulated_score, 
                                        midterm_score, 
                                        final_score
                                    FROM subjectscore
                                    WHERE active_status = 'active') ss
                                JOIN 
                                    (SELECT 
                                        sh.sys_subject_no, 
                                        sh.subject_id, 
                                        yrs.byte_desc_th AS academic_year,
                                        sem.byte_desc_th AS semester, 
                                        sec.byte_desc_th AS section
                                    FROM subjectheader sh
                                    JOIN systemparam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                    JOIN systemparam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                    JOIN systemparam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section'
                                    WHERE sh.active_status = 'active') shh
                                    ON ss.sys_subject_no = shh.sys_subject_no
                                JOIN 
                                    (SELECT subject_id, subject_name 
                                    FROM subject
                                    WHERE active_status = 'active') s
                                    ON s.subject_id = shh.subject_id
                                WHERE 1=1
                                AND (@subject_id IS NULL OR s.subject_id = NULLIF(@subject_id, ''))
                                AND (@academic_year IS NULL OR shh.academic_year = NULLIF(@academic_year, ''))
                                AND (@semester IS NULL OR shh.semester = NULLIF(@semester, ''))
                                AND (@section IS NULL OR shh.section = NULLIF(@section, ''))
                                AND (@teacher_code IS NULL OR @teacher_code = '' OR EXISTS (
                                          SELECT 1 
                                          FROM SubjectLecturer sl 
                                          WHERE sl.sys_subject_no = shh.sys_subject_no 
                                            AND sl.teacher_code = NULLIF(@teacher_code, '')
                                            AND sl.active_status = 'active'
                                      ))" },

                { "accumulated_score", @"
                    SELECT ROUND(MAX(ss.accumulated_score),2) AS MAX_ACCUMULATED_SCORE, 
                           ROUND(MIN(ss.accumulated_score),2) AS MIN_ACCUMULATED_SCORE, 
                           ROUND(CONVERT(DECIMAL(10,2), AVG(ss.accumulated_score)),2) AS AVG_ACCUMULATED_SCORE, 
                           ROUND(STDEV(ss.accumulated_score), 2) AS STD_ACCUMULATED_SCORE, 
                                 COUNT(DISTINCT ss.student_id) AS number_student
                                FROM 
                                    (SELECT 
                                        sys_subject_no, 
                                        student_id, 
                                        seat_no, 
                                        accumulated_score, 
                                        midterm_score, 
                                        final_score
                                    FROM subjectscore
                                    WHERE active_status = 'active') ss
                                JOIN 
                                    (SELECT 
                                        sh.sys_subject_no, 
                                        sh.subject_id, 
                                        yrs.byte_desc_th AS academic_year,
                                        sem.byte_desc_th AS semester, 
                                        sec.byte_desc_th AS section
                                    FROM subjectheader sh
                                    JOIN systemparam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                    JOIN systemparam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                    JOIN systemparam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section'
                                    WHERE sh.active_status = 'active') shh
                                    ON ss.sys_subject_no = shh.sys_subject_no
                                JOIN 
                                    (SELECT subject_id, subject_name 
                                    FROM subject
                                    WHERE active_status = 'active') s
                                    ON s.subject_id = shh.subject_id
                                WHERE 1=1
                                AND (@subject_id IS NULL OR s.subject_id = NULLIF(@subject_id, ''))
                                AND (@academic_year IS NULL OR shh.academic_year = NULLIF(@academic_year, ''))
                                AND (@semester IS NULL OR shh.semester = NULLIF(@semester, ''))
                                AND (@section IS NULL OR shh.section = NULLIF(@section, ''))
                                AND (@teacher_code IS NULL OR @teacher_code = '' OR EXISTS (
                                          SELECT 1 
                                          FROM SubjectLecturer sl 
                                          WHERE sl.sys_subject_no = shh.sys_subject_no 
                                            AND sl.teacher_code = NULLIF(@teacher_code, '')
                                            AND sl.active_status = 'active'
                                      ))" },

                {
                    "student_score", @"SELECT 
                                        ss.sys_subject_no, 
                                        shh.subject_id, 
                                        s.subject_name, 
                                        shh.academic_year, 
                                        shh.semester, 
                                        shh.section, 
                                        ss.student_id, 
                                        ss.seat_no, 
                                        ss.accumulated_score, 
                                        ss.midterm_score, 
                                        ss.final_score
                                    FROM 
                                        (SELECT 
                                            sys_subject_no, 
                                            student_id, 
                                            seat_no, 
                                            accumulated_score, 
                                            midterm_score, 
                                            final_score, 
                                            active_status 
                                        FROM subjectscore
                                        WHERE active_status = 'active') ss
                                    JOIN 
                                        (SELECT 
                                            sh.sys_subject_no, 
                                            sh.subject_id, 
                                            yrs.byte_desc_th AS academic_year,
                                            sem.byte_desc_th AS semester, 
                                            sec.byte_desc_th AS section,
                                            sh.active_status
                                        FROM subjectheader sh
                                        JOIN systemparam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                        JOIN systemparam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                        JOIN systemparam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section'
                                        WHERE sh.active_status = 'active') shh
                                        ON ss.sys_subject_no = shh.sys_subject_no
                                    JOIN 
                                        (SELECT 
                                            subject_id, 
                                            subject_name, 
                                            active_status 
                                        FROM subject
                                        WHERE active_status = 'active') s
                                        ON s.subject_id = shh.subject_id
                                    LEFT JOIN 
                                        (SELECT 
                                            sys_subject_no, 
                                            teacher_code, 
                                            active_status 
                                        FROM SubjectLecturer
                                        WHERE active_status = 'active') sl 
                                        ON sl.sys_subject_no = shh.sys_subject_no
                                        AND (@teacher_code IS NULL OR @teacher_code = '' OR sl.teacher_code = NULLIF(@teacher_code, ''))
                                    WHERE 
                                        1=1
                                        AND (@subject_id IS NULL OR s.subject_id = NULLIF(@subject_id, ''))
                                        AND (@academic_year IS NULL OR shh.academic_year = NULLIF(@academic_year, ''))
                                        AND (@semester IS NULL OR shh.semester = NULLIF(@semester, ''))
                                        AND (@section IS NULL OR shh.section = NULLIF(@section, ''))
                                        AND (@teacher_code IS NULL OR @teacher_code = '' OR EXISTS (
                                            SELECT 1 
                                            FROM SubjectLecturer sl2
                                            WHERE 
                                                sl2.sys_subject_no = shh.sys_subject_no
                                                AND sl2.teacher_code = NULLIF(@teacher_code, '')
                                                AND sl2.active_status = 'active'
                                        ))
                                    GROUP BY 
                                        ss.sys_subject_no, 
                                        shh.subject_id, 
                                        s.subject_name, 
                                        shh.academic_year, 
                                        shh.semester, 
                                        shh.section, 
                                        ss.student_id, 
                                        ss.seat_no, 
                                        ss.accumulated_score, 
                                        ss.midterm_score, 
                                        ss.final_score"
                },

                { "total_score", @"SELECT
                                    ROUND(MAX(Total), 2) AS MAX_TOTAL,
                                    ROUND(MIN(Total), 2) AS MIN_TOTAL,
									ROUND(CONVERT(DECIMAL(10,2), AVG(Total)), 2) AS AVG_TOTAL,
                                    ROUND(STDEV(Total), 2) AS STD_TOTAL,
                                    COUNT(DISTINCT ss.student_id) AS number_student
                                FROM 
                                    (SELECT 
                                        sys_subject_no, 
                                        student_id,
                                        accumulated_score, 
                                        midterm_score, 
                                        final_score, 
										ROUND(COALESCE(accumulated_score, 0) + COALESCE(midterm_score, 0) + COALESCE(final_score, 0), 2) AS Total,
                                        active_status
                                    FROM SubjectScore
                                    WHERE active_status = 'active') ss
                                 JOIN 
                                    (SELECT 
                                        sh.sys_subject_no, 
                                        sh.subject_id, 
                                        yrs.byte_desc_th AS academic_year,
                                        sem.byte_desc_th AS semester, 
                                        sec.byte_desc_th AS section
                                    FROM subjectheader sh
                                    JOIN systemparam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                    JOIN systemparam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                    JOIN systemparam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section'
                                    WHERE sh.active_status = 'active') shh
                                    ON ss.sys_subject_no = shh.sys_subject_no
                                JOIN 
                                    (SELECT subject_id, subject_name 
                                    FROM subject
                                    WHERE active_status = 'active') s
                                    ON s.subject_id = shh.subject_id
                                WHERE 1=1
                                AND (@subject_id IS NULL OR s.subject_id = NULLIF(@subject_id, ''))
                                AND (@academic_year IS NULL OR shh.academic_year = NULLIF(@academic_year, ''))
                                AND (@semester IS NULL OR shh.semester = NULLIF(@semester, ''))
                                AND (@section IS NULL OR shh.section = NULLIF(@section, ''))
                                AND (@teacher_code IS NULL OR @teacher_code = '' OR EXISTS (
                                          SELECT 1 
                                          FROM SubjectLecturer sl 
                                          WHERE sl.sys_subject_no = shh.sys_subject_no 
                                            AND sl.teacher_code = NULLIF(@teacher_code, '')
                                            AND sl.active_status = 'active'
                                      ))"
                }
            };

            List<object> responseList = new List<object>();

            List<DashboardStudentScore> wrapper = new List<DashboardStudentScore>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                if (resource.score_type == "คะแนนรวม" || resource.score_type == "")
                {
                    using (SqlCommand command = new SqlCommand(queries["total_score"], connection))
                    {
                        command.Parameters.AddWithValue("@subject_id", string.IsNullOrEmpty(resource.subject_id) ? DBNull.Value : (object)resource.subject_id);
                        command.Parameters.AddWithValue("@academic_year", string.IsNullOrEmpty(resource.academic_year) ? DBNull.Value : (object)resource.academic_year);
                        command.Parameters.AddWithValue("@semester", string.IsNullOrEmpty(resource.semester) ? DBNull.Value : (object)resource.semester);
                        command.Parameters.AddWithValue("@section", string.IsNullOrEmpty(resource.section) ? DBNull.Value : (object)resource.section);
                        command.Parameters.AddWithValue("@teacher_code", string.IsNullOrEmpty(resource.teacher_code) ? DBNull.Value : (object)resource.teacher_code);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    responseList.Add(new
                                    {
                                        total_score = new DashboardTotalScore
                                        {
                                            MaxTotalScore = reader.IsDBNull(0) ? 0 : Convert.ToDecimal(reader.GetValue(0)),
                                            MinTotalScore = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1)),
                                            AvgTotalScore = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2)),
                                            StdTotalScore = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3)),
                                            NumberOfStudents = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                                        }
                                    });
                                }
                            }
                        }
                    }

                    using (SqlCommand command = new SqlCommand(queries["student_score"], connection))
                    {
                        command.Parameters.AddWithValue("@subject_id", string.IsNullOrEmpty(resource.subject_id) ? DBNull.Value : (object)resource.subject_id);
                        command.Parameters.AddWithValue("@academic_year", string.IsNullOrEmpty(resource.academic_year) ? DBNull.Value : (object)resource.academic_year);
                        command.Parameters.AddWithValue("@semester", string.IsNullOrEmpty(resource.semester) ? DBNull.Value : (object)resource.semester);
                        command.Parameters.AddWithValue("@section", string.IsNullOrEmpty(resource.section) ? DBNull.Value : (object)resource.section);
                        command.Parameters.AddWithValue("@teacher_code", string.IsNullOrEmpty(resource.teacher_code) ? DBNull.Value : (object)resource.teacher_code);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var data = new DashboardStudentScore
                                    {
                                        sys_subject_no = reader.IsDBNull(0) ? null : reader.GetInt32(0),
                                        subject_id = reader.IsDBNull(1) ? null : reader.GetString(1),
                                        subject_name = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        academic_year = reader.IsDBNull(3) ? null : reader.GetString(3),
                                        semester = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        section = reader.IsDBNull(5) ? null : reader.GetString(5),
                                        student_id = reader.IsDBNull(6) ? null : reader.GetString(6),
                                        seat_no = reader.IsDBNull(7) ? null : reader.GetString(7),
                                        accumulated_score = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                                        midterm_score = reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                                        final_score = reader.IsDBNull(10) ? null : reader.GetDecimal(10)
                                    };
                                    wrapper.Add(data);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // กรณีที่ score_type มีค่าเป็น midterm_score, final_score, หรือ accumulated_score
                    string? mappedScoreType = scoreTypeMapping.ContainsKey(resource.score_type) ? scoreTypeMapping[resource.score_type] : null;


                    if (mappedScoreType != null && queries.ContainsKey(mappedScoreType))
                    {
                        using (SqlCommand command = new SqlCommand(queries[mappedScoreType], connection))
                        {
                            command.Parameters.AddWithValue("@subject_id", string.IsNullOrEmpty(resource.subject_id) ? DBNull.Value : (object)resource.subject_id);
                            command.Parameters.AddWithValue("@academic_year", string.IsNullOrEmpty(resource.academic_year) ? DBNull.Value : (object)resource.academic_year);
                            command.Parameters.AddWithValue("@semester", string.IsNullOrEmpty(resource.semester) ? DBNull.Value : (object)resource.semester);
                            command.Parameters.AddWithValue("@section", string.IsNullOrEmpty(resource.section) ? DBNull.Value : (object)resource.section);
                            command.Parameters.AddWithValue("@teacher_code", string.IsNullOrEmpty(resource.teacher_code) ? DBNull.Value : (object)resource.teacher_code);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        switch (mappedScoreType)
                                        {
                                            case "final_score":
                                                responseList.Add(new
                                                {
                                                    //final_score = new DashboardFinalScore
                                                    //{
                                                    //    MaxFinalScore = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                                    //    MinFinalScore = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                                    //    AvgFinalScore = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                                    //    StdFinalScore = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                                                    //    NumberOfStudents = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                                                    //}
                                                    total_score = new DashboardTotalScore
                                                    {
                                                        MaxTotalScore = reader.IsDBNull(0) ? 0 : Convert.ToDecimal(reader.GetValue(0)),
                                                        MinTotalScore = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1)),
                                                        AvgTotalScore = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2)),
                                                        StdTotalScore = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3)),
                                                        NumberOfStudents = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                                                    }
                                                });
                                                break;

                                            case "midterm_score":
                                                responseList.Add(new
                                                {
                                                    //midterm_score = new DashboardMidtermScore
                                                    //{
                                                    //    MaxMidtermScore = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                                    //    MinMidtermScore = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                                    //    AvgMidtermScore = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                                    //    StdMidtermScore = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                                                    //    NumberOfStudents = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                                                    //}
                                                    total_score = new DashboardTotalScore
                                                    {
                                                        MaxTotalScore = reader.IsDBNull(0) ? 0 : Convert.ToDecimal(reader.GetValue(0)),
                                                        MinTotalScore = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1)),
                                                        AvgTotalScore = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2)),
                                                        StdTotalScore = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3)),
                                                        NumberOfStudents = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                                                    }
                                                });
                                                break;

                                            case "accumulated_score":
                                                responseList.Add(new
                                                {
                                                    //accumulated_score = new DashboardAccumulatedScore
                                                    //{
                                                    //    MaxAccumulatedScore = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                                    //    MinAccumulatedScore = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                                    //    AvgAccumulatedScore = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                                    //    StdAccumulatedScore = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                                                    //    NumberOfStudents = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                                                    //}
                                                    total_score = new DashboardTotalScore
                                                    {
                                                        MaxTotalScore = reader.IsDBNull(0) ? 0 : Convert.ToDecimal(reader.GetValue(0)),
                                                        MinTotalScore = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1)),
                                                        AvgTotalScore = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2)),
                                                        StdTotalScore = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3)),
                                                        NumberOfStudents = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                                                    }
                                                });
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    using (SqlCommand command = new SqlCommand(queries["student_score"], connection))
                    {
                        command.Parameters.AddWithValue("@subject_id", string.IsNullOrEmpty(resource.subject_id) ? DBNull.Value : (object)resource.subject_id);
                        command.Parameters.AddWithValue("@academic_year", string.IsNullOrEmpty(resource.academic_year) ? DBNull.Value : (object)resource.academic_year);
                        command.Parameters.AddWithValue("@semester", string.IsNullOrEmpty(resource.semester) ? DBNull.Value : (object)resource.semester);
                        command.Parameters.AddWithValue("@section", string.IsNullOrEmpty(resource.section) ? DBNull.Value : (object)resource.section);
                        command.Parameters.AddWithValue("@teacher_code", string.IsNullOrEmpty(resource.teacher_code) ? DBNull.Value : (object)resource.teacher_code);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var data = new DashboardStudentScore
                                    {
                                        sys_subject_no = reader.IsDBNull(0) ? null : reader.GetInt32(0),
                                        subject_id = reader.IsDBNull(1) ? null : reader.GetString(1),
                                        subject_name = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        academic_year = reader.IsDBNull(3) ? null : reader.GetString(3),
                                        semester = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        section = reader.IsDBNull(5) ? null : reader.GetString(5),
                                        student_id = reader.IsDBNull(6) ? null : reader.GetString(6),
                                        seat_no = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    };

                                    if (mappedScoreType == "accumulated_score")
                                    {
                                        data.accumulated_score = reader.IsDBNull(8) ? null : Convert.ToDecimal(reader.GetValue(8));
                                    }
                                    else if (mappedScoreType == "midterm_score")
                                    {
                                        data.midterm_score = reader.IsDBNull(9) ? null : reader.GetDecimal(9); reader.GetDecimal(9);
                                    }
                                    else if (mappedScoreType == "final_score")
                                    {
                                        data.final_score = reader.IsDBNull(10) ? null : Convert.ToDecimal(reader.GetValue(10));
                                    }
                                    else
                                    {
                                        data.accumulated_score = reader.IsDBNull(8) ? null : Convert.ToDecimal(reader.GetValue(8));
                                        data.midterm_score = reader.IsDBNull(9) ? null : Convert.ToDecimal(reader.GetValue(9));
                                        data.final_score = reader.IsDBNull(10) ? null : Convert.ToDecimal(reader.GetValue(10));
                                    }

                                    wrapper.Add(data);
                                }
                            }
                        }
                    }
                }
                responseList.Add(new { StudentScore = wrapper });
            }

            return responseList;
        }

        public async Task<List<SubjectResponse>> GetSubjectDashboard(string? teacher_code)
        {
            var subjects = new List<SubjectResponse>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string sqlQuery;
                    if (string.IsNullOrEmpty(teacher_code))
                    {
                        // ถ้าไม่มี teacher_code, ใช้ query ที่ไม่ใช้ JOIN
                        sqlQuery = @"
                                    SELECT s.subject_id, s.subject_name 
                                    FROM subject s";
                    }
                    else
                    {
                        // ถ้ามี teacher_code, ใช้ query ที่มี JOIN
                        sqlQuery = @"
                                    SELECT s.subject_id, s.subject_name 
                                    FROM subject s
                                    JOIN SubjectHeader sh ON s.subject_id = sh.subject_id
                                    JOIN SubjectLecturer sl ON sh.sys_subject_no = sl.sys_subject_no
                                    WHERE sl.teacher_code = NULLIF(@teacher_code, '')";
                    }

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        // เพิ่ม parameter ในกรณีที่มี teacher_code
                        if (!string.IsNullOrEmpty(teacher_code))
                        {
                            command.Parameters.AddWithValue("@teacher_code", teacher_code);
                        }

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                subjects.Add(new SubjectResponse
                                {
                                    subject_id = reader["subject_id"].ToString()!,
                                    subject_name = reader["subject_name"].ToString()!
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("An error occurred while fetching the subject data.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred.", ex);
            }
            return subjects;
        }
    }
}