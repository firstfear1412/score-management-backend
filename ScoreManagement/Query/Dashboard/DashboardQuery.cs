using Microsoft.Data.SqlClient;
using ScoreManagement.Interfaces.Dashboard;
using ScoreManagement.Model.ScoreAnnoucement;

using static DashboardStatisticsResponse;

namespace ScoreManagement.Query.Dashboard
{
    public class DashboardQuery : IDashboardQuery
    {
        private readonly string _connectionString;

        public DashboardQuery(IConfiguration configuration)
        {
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
                    select max(ss.final_score) as max_final_score, min(ss.final_score) as min_final_score, avg(ss.final_score) as avg_final_score, round(stdev(ss.final_score), 1) as std_final_score, count(*) as number_student
                    from (select 
                        sys_subject_no, 
                        student_id, 
                        seat_no, 
                        accumulated_score, 
                        midterm_score, 
                        final_score, 
                        active_status 
                    from subjectscore
                    where 1=1
                    and active_status = 'active') ss,
                    (select 
                        sh.sys_subject_no, 
                        sh.subject_id, 
                        yrs.byte_desc_th as academic_year,
                        sem.byte_desc_th as semester, 
                        sec.byte_desc_th as section,
                        sh.active_status
                    from subjectheader sh
                    join systemparam yrs on sh.academic_year = yrs.byte_code and yrs.byte_reference = 'academic_year'
                    join systemparam sem on sh.semester = sem.byte_code and sem.byte_reference = 'semester'
                    join systemparam sec on sh.section = sec.byte_code and sec.byte_reference = 'section') shh,
                    (select subject_id, subject_name, active_status from subject
                    where 1=1
                    and active_status = 'active') s
                    where 1=1
                    and ss.sys_subject_no = shh.sys_subject_no
                    and s.subject_id = shh.subject_id
                    and (@subject_id is null or s.subject_id = nullif(@subject_id, ''))
                    and (@academic_year is null or shh.academic_year = nullif(@academic_year, ''))
                    and (@semester is null or shh.semester = nullif(@semester, ''))
                    and (@section is null or shh.section = nullif(@section, ''))" },

                { "midterm_score", @"
                    SELECT MAX(ss.midterm_score) AS MAX_MIDTERM_SCORE, MIN(ss.midterm_score) AS MIN_MIDTERM_SCORE, 
                           AVG(ss.midterm_score) AS AVG_MIDTERM_SCORE, ROUND(STDEV(ss.midterm_score), 1) AS STD_MIDTERM_SCORE, 
                           COUNT(*) AS NUMBER_STUDENT
                    from (select 
                        sys_subject_no, 
                        student_id, 
                        seat_no, 
                        accumulated_score, 
                        midterm_score, 
                        final_score, 
                        active_status 
                    from subjectscore
                    where 1=1
                    and active_status = 'active') ss,
                    (select 
                        sh.sys_subject_no, 
                        sh.subject_id, 
                        yrs.byte_desc_th as academic_year,
                        sem.byte_desc_th as semester, 
                        sec.byte_desc_th as section,
                        sh.active_status
                    from subjectheader sh
                    join systemparam yrs on sh.academic_year = yrs.byte_code and yrs.byte_reference = 'academic_year'
                    join systemparam sem on sh.semester = sem.byte_code and sem.byte_reference = 'semester'
                    join systemparam sec on sh.section = sec.byte_code and sec.byte_reference = 'section') shh,
                    (select subject_id, subject_name, active_status from subject
                    where 1=1
                    and active_status = 'active') s
                    where 1=1
                    and ss.sys_subject_no = shh.sys_subject_no
                    and s.subject_id = shh.subject_id
                    and (@subject_id is null or s.subject_id = nullif(@subject_id, ''))
                    and (@academic_year is null or shh.academic_year = nullif(@academic_year, ''))
                    and (@semester is null or shh.semester = nullif(@semester, ''))
                    and (@section is null or shh.section = nullif(@section, ''))" },

                { "accumulated_score", @"
                    SELECT MAX(ss.accumulated_score) AS MAX_ACCUMULATED_SCORE, MIN(ss.accumulated_score) AS MIN_ACCUMULATED_SCORE, 
                           AVG(ss.accumulated_score) AS AVG_ACCUMULATED_SCORE, ROUND(STDEV(ss.accumulated_score), 1) AS STD_ACCUMULATED_SCORE, 
                           COUNT(*) AS NUMBER_STUDENT
                    from (select 
                        sys_subject_no, 
                        student_id, 
                        seat_no, 
                        accumulated_score, 
                        midterm_score, 
                        final_score, 
                        active_status 
                    from subjectscore
                    where 1=1
                    and active_status = 'active') ss,
                    (select 
                        sh.sys_subject_no, 
                        sh.subject_id, 
                        yrs.byte_desc_th as academic_year,
                        sem.byte_desc_th as semester, 
                        sec.byte_desc_th as section,
                        sh.active_status
                    from subjectheader sh
                    join systemparam yrs on sh.academic_year = yrs.byte_code and yrs.byte_reference = 'academic_year'
                    join systemparam sem on sh.semester = sem.byte_code and sem.byte_reference = 'semester'
                    join systemparam sec on sh.section = sec.byte_code and sec.byte_reference = 'section') shh,
                    (select subject_id, subject_name, active_status from subject
                    where 1=1
                    and active_status = 'active') s
                    where 1=1
                    and ss.sys_subject_no = shh.sys_subject_no
                    and s.subject_id = shh.subject_id
                    and (@subject_id is null or s.subject_id = nullif(@subject_id, ''))
                    and (@academic_year is null or shh.academic_year = nullif(@academic_year, ''))
                    and (@semester is null or shh.semester = nullif(@semester, ''))
                    and (@section is null or shh.section = nullif(@section, ''))" },

                {
                    "student_score", @"SELECT ss.sys_subject_no, shh.subject_id, s.subject_name, shh.academic_year, shh.semester, shh.section, ss.accumulated_score, ss.midterm_score, ss.final_score
                    FROM (SELECT 
                        sys_subject_no, 
                        student_id, 
                        seat_no, 
                        accumulated_score, 
                        midterm_score, 
                        final_score, 
                        active_status 
                    FROM SubjectScore
                    WHERE 1=1
                    AND active_status = 'active') ss,
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
                    JOIN SystemParam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section') shh,
                    (SELECT subject_id, subject_name, active_status FROM Subject
                    WHERE 1=1
                    AND active_status = 'active') s
                    WHERE 1=1
                    AND ss.sys_subject_no = shh.sys_subject_no
                    AND s.subject_id = shh.subject_id
					AND (@subject_id is null or s.subject_id = nullif(@subject_id, ''))
                    AND (COALESCE(@academic_year, '') = '' OR shh.academic_year = @academic_year)
                    AND (COALESCE(@semester, '') = '' OR shh.semester = @semester)
                    AND (COALESCE(@section, '') = '' OR shh.section = @section)
                    AND ss.active_status = 'active'"
                },

                { "total_score", @"SELECT
                                    MAX(Total) AS MAX_TOTAL,
                                    MIN(Total) AS MIN_TOTAL,
                                    AVG(Total) AS AVG_TOTAL,
                                    ROUND(STDEV(Total), 1) AS STD_TOTAL,
                                    COUNT(*) AS NUMBER_OF_STUDENTS
                                FROM 
                                    (SELECT 
                                        sys_subject_no, 
                                        student_id, 
                                        accumulated_score, 
                                        midterm_score, 
                                        final_score, 
                                        (accumulated_score + midterm_score + final_score) AS Total,
                                        active_status 
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
                                    JOIN SystemParam sec ON sh.section = sec.byte_code AND sec.byte_reference = 'section') shh
                                    ON ss.sys_subject_no = shh.sys_subject_no
                                JOIN 
                                    (SELECT subject_id, subject_name, active_status 
                                    FROM Subject
                                    WHERE active_status = 'active') s
                                    ON ss.sys_subject_no = shh.sys_subject_no
                                    AND s.subject_id = shh.subject_id
                                WHERE 1=1
                                AND ss.sys_subject_no = shh.sys_subject_no
                                AND s.subject_id = shh.subject_id
                                AND (@subject_id is null or s.subject_id = nullif(@subject_id, ''))
                                AND (COALESCE(@academic_year, '') = '' OR shh.academic_year = @academic_year)
                                AND (COALESCE(@semester, '') = '' OR shh.semester = @semester)
                                AND (COALESCE(@section, '') = '' OR shh.section = @section)
                                AND ss.active_status = 'active'"
                }
            };

            List<object> responseList = new List<object>();

            List<DashboardStudentScore> wrapper = new List<DashboardStudentScore>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                if (string.IsNullOrEmpty(resource.score_type))
                {
                    using (SqlCommand command = new SqlCommand(queries["total_score"], connection))
                    {
                        command.Parameters.AddWithValue("@subject_id", string.IsNullOrEmpty(resource.subject_id) ? DBNull.Value : (object)resource.subject_id);
                        command.Parameters.AddWithValue("@academic_year", string.IsNullOrEmpty(resource.academic_year) ? DBNull.Value : (object)resource.academic_year);
                        command.Parameters.AddWithValue("@semester", string.IsNullOrEmpty(resource.semester) ? DBNull.Value : (object)resource.semester);
                        command.Parameters.AddWithValue("@section", string.IsNullOrEmpty(resource.section) ? DBNull.Value : (object)resource.section);

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
                                            MaxTotalScore = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                            MinTotalScore = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                            AvgTotalScore = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                            StdTotalScore = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
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
                                        accumulated_score = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                        midterm_score = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                        final_score = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                                    };
                                    wrapper.Add(data);
                                }
                            }
                        }
                    }
                }
                else
                {
                    string? mappedScoreType = scoreTypeMapping.ContainsKey(resource.score_type) ? scoreTypeMapping[resource.score_type] : null;


                    if (mappedScoreType != null && queries.ContainsKey(mappedScoreType))
                    {
                        using (SqlCommand command = new SqlCommand(queries[mappedScoreType], connection))
                        {
                            command.Parameters.AddWithValue("@subject_id", string.IsNullOrEmpty(resource.subject_id) ? DBNull.Value : (object)resource.subject_id);
                            command.Parameters.AddWithValue("@academic_year", string.IsNullOrEmpty(resource.academic_year) ? DBNull.Value : (object)resource.academic_year);
                            command.Parameters.AddWithValue("@semester", string.IsNullOrEmpty(resource.semester) ? DBNull.Value : (object)resource.semester);
                            command.Parameters.AddWithValue("@section", string.IsNullOrEmpty(resource.section) ? DBNull.Value : (object)resource.section);

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
                                                        MaxTotalScore = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                                        MinTotalScore = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                                        AvgTotalScore = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                                        StdTotalScore = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
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
                                                        MaxTotalScore = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                                        MinTotalScore = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                                        AvgTotalScore = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                                        StdTotalScore = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
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
                                                        MaxTotalScore = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                                        MinTotalScore = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                                        AvgTotalScore = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                                        StdTotalScore = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
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
                                        section = reader.IsDBNull(5) ? null : reader.GetString(5)
                                    };

                                    if (mappedScoreType == "accumulated_score")
                                    {
                                        data.accumulated_score = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                                    }
                                    else if (mappedScoreType == "midterm_score")
                                    {
                                        data.midterm_score = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                                    }
                                    else if (mappedScoreType == "final_score")
                                    {
                                        data.final_score = reader.IsDBNull(8) ? 0 : reader.GetInt32(8);
                                    }
                                    else
                                    {
                                        data.accumulated_score = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                                        data.midterm_score = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                                        data.final_score = reader.IsDBNull(8) ? 0 : reader.GetInt32(8);
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
    }
}