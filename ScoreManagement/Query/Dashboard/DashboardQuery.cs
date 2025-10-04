using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces.Dashboard;
using ScoreManagement.Model;
using ScoreManagement.Model.ExcelScore;
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
                                                            COUNT(ss.student_id) AS number_student
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
                                                                sec.byte_desc_th AS section,
					                                                     sh.active_status
                                                            FROM SubjectHeader sh
                                                            JOIN SystemParam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                                            JOIN SystemParam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                                            JOIN SystemParam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section'
				                                                     WHERE sh.active_status = 'active') shh
                                                        ON ss.sys_subject_no = shh.sys_subject_no
                                                        JOIN 
                                                            (SELECT subject_id, subject_name 
                                                            FROM Subject
                                                            WHERE active_status = 'active') s
                                                        ON s.subject_id = shh.subject_id
														INNER JOIN (
															  SELECT sys_subject_no, MIN(teacher_code) AS teacher_code
															  FROM SubjectLecturer
															  WHERE active_status = 'active'
															  AND (@teacher_code IS NULL OR teacher_code = @teacher_code)
															  GROUP BY sys_subject_no
														  ) sl		
														ON shh.sys_subject_no = sl.sys_subject_no
                                                        WHERE 1=1
                                                        AND (NULLIF(@subject_id, '') IS NULL OR s.subject_id = @subject_id)
                                                        AND (NULLIF(@academic_year, '') IS NULL OR shh.academic_year = @academic_year)
                                                        AND (NULLIF(@semester, '') IS NULL OR shh.semester = @semester)
                                                        AND (NULLIF(@section, '') IS NULL OR shh.section = @section)
			                                            --AND sl.active_status = 'active'
			                                            AND (NULLIF(@teacher_code, '') IS NULL OR sl.teacher_code = @teacher_code)
				                                        --AND sl.teacher_code = NULLIF(@teacher_code, '')
			                                            --GROUP BY s.subject_id, s.subject_name, shh.academic_year, shh.semester, shh.section;" },

                { "midterm_score", @"
                                            SELECT  ROUND(MAX(ss.midterm_score),2) AS MAX_MIDTERM_SCORE, 
                                                    ROUND(MIN(ss.midterm_score),2) AS MIN_MIDTERM_SCORE, 
                                                    ROUND(CONVERT(DECIMAL(10,2), AVG(ss.midterm_score)),2) AS AVG_MIDTERM_SCORE, 
                                                    ROUND(STDEV(ss.midterm_score), 2) AS STD_MIDTERM_SCORE, 
                                                    COUNT(ss.student_id) AS number_student
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
                                                                sec.byte_desc_th AS section,
					                                                     sh.active_status
                                                            FROM SubjectHeader sh
                                                            JOIN SystemParam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                                            JOIN SystemParam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                                            JOIN SystemParam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section'
				                                                     WHERE sh.active_status = 'active') shh
                                                        ON ss.sys_subject_no = shh.sys_subject_no
                                                        JOIN 
                                                            (SELECT subject_id, subject_name 
                                                            FROM Subject
                                                            WHERE active_status = 'active') s
                                                        ON s.subject_id = shh.subject_id
														INNER JOIN (
															  SELECT sys_subject_no, MIN(teacher_code) AS teacher_code
															  FROM SubjectLecturer
															  WHERE active_status = 'active'
															  AND (@teacher_code IS NULL OR teacher_code = @teacher_code)
															  GROUP BY sys_subject_no
														  ) sl		
														ON shh.sys_subject_no = sl.sys_subject_no
                                                        WHERE 1=1
                                                        AND (NULLIF(@subject_id, '') IS NULL OR s.subject_id = @subject_id)
                                                        AND (NULLIF(@academic_year, '') IS NULL OR shh.academic_year = @academic_year)
                                                        AND (NULLIF(@semester, '') IS NULL OR shh.semester = @semester)
                                                        AND (NULLIF(@section, '') IS NULL OR shh.section = @section)
			                                            --AND sl.active_status = 'active'
			                                            AND (NULLIF(@teacher_code, '') IS NULL OR sl.teacher_code = @teacher_code)
				                                        --AND sl.teacher_code = NULLIF(@teacher_code, '')
			                                            --GROUP BY s.subject_id, s.subject_name, shh.academic_year, shh.semester, shh.section;" },

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
                                                                sec.byte_desc_th AS section,
					                                                     sh.active_status
                                                            FROM SubjectHeader sh
                                                            JOIN SystemParam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                                            JOIN SystemParam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                                            JOIN SystemParam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section'
				                                                     WHERE sh.active_status = 'active') shh
                                                        ON ss.sys_subject_no = shh.sys_subject_no
                                                        JOIN 
                                                            (SELECT subject_id, subject_name 
                                                            FROM Subject
                                                            WHERE active_status = 'active') s
                                                        ON s.subject_id = shh.subject_id
														INNER JOIN (
															  SELECT sys_subject_no, MIN(teacher_code) AS teacher_code
															  FROM SubjectLecturer
															  WHERE active_status = 'active'
															  AND (@teacher_code IS NULL OR teacher_code = @teacher_code)
															  GROUP BY sys_subject_no
														  ) sl		
														ON shh.sys_subject_no = sl.sys_subject_no
                                                        WHERE 1=1
                                                        AND (NULLIF(@subject_id, '') IS NULL OR s.subject_id = @subject_id)
                                                        AND (NULLIF(@academic_year, '') IS NULL OR shh.academic_year = @academic_year)
                                                        AND (NULLIF(@semester, '') IS NULL OR shh.semester = @semester)
                                                        AND (NULLIF(@section, '') IS NULL OR shh.section = @section)
			                                            --AND sl.active_status = 'active'
			                                            AND (NULLIF(@teacher_code, '') IS NULL OR sl.teacher_code = @teacher_code)
				                                        --AND sl.teacher_code = NULLIF(@teacher_code, '')
			                                            --GROUP BY s.subject_id, s.subject_name, shh.academic_year, shh.semester, shh.section;" },

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

                    { "total_score", @"
                                                        SELECT
                                                            ROUND(MAX(Total), 2) AS MAX_TOTAL,
                                                            ROUND(MIN(Total), 2) AS MIN_TOTAL,
									                        ROUND(CONVERT(DECIMAL(10,2), AVG(Total)), 2) AS AVG_TOTAL,
                                                            ROUND(STDEV(Total), 2) AS STD_TOTAL,
                                                            COUNT(ss.student_id) AS number_student
                                                        FROM 
                                                        (SELECT 
                                                              sys_subject_no, 
                                                              student_id,
                                                              accumulated_score, 
                                                              midterm_score, 
                                                              final_score,
								                               CASE 
										                            WHEN accumulated_score IS NULL AND midterm_score IS NULL AND final_score IS NULL 
										                            THEN NULL
										                            ELSE ROUND(COALESCE(accumulated_score, 0) + COALESCE(midterm_score, 0) + COALESCE(final_score, 0), 2) 
								                            END AS Total
                                                          FROM SubjectScore
                                                         WHERE active_status = 'active') ss
                                                            JOIN 
                                                            (SELECT 
                                                                sh.sys_subject_no, 
                                                                sh.subject_id, 
                                                                yrs.byte_desc_th AS academic_year,
                                                                sem.byte_desc_th AS semester, 
                                                                sec.byte_desc_th AS section,
					                                                     sh.active_status
                                                            FROM SubjectHeader sh
                                                            JOIN SystemParam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                                            JOIN SystemParam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                                            JOIN SystemParam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section'
				                                                     WHERE sh.active_status = 'active') shh
                                                        ON ss.sys_subject_no = shh.sys_subject_no
                                                        JOIN 
                                                            (SELECT subject_id, subject_name 
                                                            FROM Subject
                                                            WHERE active_status = 'active') s
                                                        ON s.subject_id = shh.subject_id
														INNER JOIN (
															  SELECT sys_subject_no, MIN(teacher_code) AS teacher_code
															  FROM SubjectLecturer
															  WHERE active_status = 'active'
															  AND (@teacher_code IS NULL OR teacher_code = @teacher_code)
															  GROUP BY sys_subject_no
														  ) sl		
														ON shh.sys_subject_no = sl.sys_subject_no
                                                        WHERE 1=1
                                                        AND (NULLIF(@subject_id, '') IS NULL OR s.subject_id = @subject_id)
                                                        AND (NULLIF(@academic_year, '') IS NULL OR shh.academic_year = @academic_year)
                                                        AND (NULLIF(@semester, '') IS NULL OR shh.semester = @semester)
                                                        AND (NULLIF(@section, '') IS NULL OR shh.section = @section)
			                                            --AND sl.active_status = 'active'
			                                            AND (NULLIF(@teacher_code, '') IS NULL OR sl.teacher_code = @teacher_code)
				                                        --AND sl.teacher_code = NULLIF(@teacher_code, '')
			                                            --GROUP BY s.subject_id, s.subject_name, shh.academic_year, shh.semester, shh.section;"
                }
            };

            List<object> responseList = new List<object>();

            List<DashboardStudentScore> wrapper = new List<DashboardStudentScore>();

            try
            {
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
                                            data.midterm_score = reader.IsDBNull(9) ? null : Convert.ToDecimal(reader.GetValue(9));
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
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
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
		                            SELECT s.subject_id, s.subject_name FROM SubjectHeader sh
	                                JOIN Subject s ON sh.subject_id = s.subject_id
	                                WHERE 1=1
									AND s.active_status = 'active'
									AND sh.active_status = 'active'
									GROUP BY s.subject_id, s.subject_name";
                    }
                    else
                    {
                        // ถ้ามี teacher_code, ใช้ query ที่มี JOIN
                        sqlQuery = @"
	                        SELECT s.subject_id, s.subject_name FROM SubjectHeader sh
	                            JOIN Subject s ON sh.subject_id = s.subject_id
	                            JOIN SubjectLecturer sl ON sh.sys_subject_no = sl.sys_subject_no
	                            WHERE s.active_status = 'active'
	                            AND sh.active_status = 'active'
								AND sl.active_status = 'active'
	                            AND sl.teacher_code = NULLIF(@teacher_code, '')
	                            GROUP BY s.subject_id, s.subject_name";
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
	                                    @score_type NVARCHAR(50) = @p_score_type,
                                        @teacher_code NVARCHAR(50) = @p_teacher_code;

                                    WITH AverageScores AS (
		                            SELECT 
		                            sys_subject_no,
                                    AVG(
		                            CASE 
		                            WHEN @score_type = 'คะแนนปลายภาค' THEN COALESCE(final_score, 0)
                                                                 WHEN @score_type = 'คะแนนกลางภาค' THEN COALESCE(midterm_score, 0)
                                                                 WHEN @score_type = 'คะแนนระหว่างเรียน' THEN COALESCE(accumulated_score, 0)
                                                                 WHEN @score_type = 'คะแนนรวม' THEN
                                                                 COALESCE(accumulated_score, 0) + COALESCE(midterm_score, 0) + COALESCE(final_score, 0)
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
                                                         WHEN @score_type = 'คะแนนรวม' THEN 'คะแนนรวม'
                                                         WHEN @score_type = 'คะแนนปลายภาค' THEN 'คะแนนปลายภาค'
                                                         WHEN @score_type = 'คะแนนกลางภาค' THEN 'คะแนนกลางภาค'
                                                         WHEN @score_type = 'คะแนนระหว่างเรียน' THEN 'คะแนนระหว่างเรียน'
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
                                                         sec.byte_desc_th AS section,
							                             sh.active_status
                                                     FROM SubjectHeader sh
                                                     JOIN SystemParam yrs ON sh.academic_year = yrs.byte_code AND yrs.byte_reference = 'academic_year'
                                                     JOIN SystemParam sem ON sh.semester = sem.byte_code AND sem.byte_reference = 'semester'
                                                     JOIN SystemParam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section'
						                             WHERE sh.active_status = 'active') shh
                                                 ON ss.sys_subject_no = shh.sys_subject_no
                                                 JOIN 
                                                     (SELECT subject_id, subject_name 
                                                     FROM Subject
                                                     WHERE active_status = 'active') s
                                                 ON s.subject_id = shh.subject_id
                                                 LEFT JOIN AverageScores avg ON ss.sys_subject_no = avg.sys_subject_no
					                             JOIN SubjectLecturer sl ON shh.sys_subject_no = sl.sys_subject_no
                                                 WHERE 1=1
                                                 AND (NULLIF(@subject_id, '') IS NULL OR s.subject_id = @subject_id)
                                                 AND (NULLIF(@academic_year, '') IS NULL OR shh.academic_year = @academic_year)
                                                 AND (NULLIF(@semester, '') IS NULL OR shh.semester = @semester)
                                                 AND (NULLIF(@section, '') IS NULL OR shh.section = @section)
					                             AND sl.active_status = 'active'
					                             AND (NULLIF(@teacher_code, '') IS NULL OR sl.teacher_code = @teacher_code)
						                            --AND sl.teacher_code = NULLIF(@teacher_code, '')
					                            GROUP BY s.subject_id, s.subject_name, shh.academic_year, shh.semester, shh.section, avg.avg_score
                                                ORDER BY shh.academic_year DESC;
                ";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@p_subject_id", string.IsNullOrEmpty(request.subject_id) ? DBNull.Value : (object)request.subject_id);
                        command.Parameters.AddWithValue("@p_academic_year", string.IsNullOrEmpty(request.academic_year) ? DBNull.Value : (object)request.academic_year);
                        command.Parameters.AddWithValue("@p_semester", string.IsNullOrEmpty(request.semester) ? DBNull.Value : (object)request.semester);
                        command.Parameters.AddWithValue("@p_section", string.IsNullOrEmpty(request.section) ? DBNull.Value : (object)request.section);
                        command.Parameters.AddWithValue("@p_score_type", string.IsNullOrEmpty(request.score_type) ? DBNull.Value : (object)request.score_type);
                        command.Parameters.AddWithValue("@p_teacher_code", string.IsNullOrEmpty(request.teacher_code) ? DBNull.Value : (object)request.teacher_code);

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
                throw new Exception("There is an error.", ex);
            }
            return results;
        }
    }
}