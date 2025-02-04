using OfficeOpenXml;
using ScoreManagement.Model.ExcelScore;
using System;
using System.Collections.Generic;
using System.IO;

namespace ScoreManagement.Helpers
{
    public static class ExcelHelper
    {
        public static string GenerateExcelBase64(List<ExcelScoreModel> data)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Score Report");

                // เพิ่ม Header
                worksheet.Cells[1, 1].Value = "รหัสวิชา";
                worksheet.Cells[1, 2].Value = "ชื่อวิชา";
                worksheet.Cells[1, 3].Value = "ปีการศึกษา";
                worksheet.Cells[1, 4].Value = "ภาคเรียน";
                worksheet.Cells[1, 5].Value = "หมู่เรียน";
                worksheet.Cells[1, 6].Value = "ประเภทคะแนน";
                worksheet.Cells[1, 7].Value = "จำนวนนิสิต";
                worksheet.Cells[1, 8].Value = "คะแนนเฉลี่ย";
                worksheet.Cells[1, 9].Value = "คะแนนต่ำสุด";
                worksheet.Cells[1, 10].Value = "คะแนนสูงสุด";
                worksheet.Cells[1, 11].Value = "ค่าเบี่ยงเบนมาตรฐาน";
                worksheet.Cells[1, 12].Value = "สัดส่วนคะแนนช่วง 0-39";
                worksheet.Cells[1, 13].Value = "สัดส่วนคะแนนช่วง 40-49";
                worksheet.Cells[1, 14].Value = "สัดส่วนคะแนนช่วง 50-59";
                worksheet.Cells[1, 15].Value = "สัดส่วนคะแนนช่วง 60-69";
                worksheet.Cells[1, 16].Value = "สัดส่วนคะแนนช่วง 70-79";
                worksheet.Cells[1, 17].Value = "สัดส่วนคะแนนช่วง 80+";

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.SubjectId;
                    worksheet.Cells[row, 2].Value = item.SubjectName;
                    worksheet.Cells[row, 3].Value = item.AcademicYear;
                    worksheet.Cells[row, 4].Value = item.Semester;
                    worksheet.Cells[row, 5].Value = item.Section;
                    worksheet.Cells[row, 6].Value = item.ScoreType;
                    worksheet.Cells[row, 7].Value = item.StudentCount;
                    worksheet.Cells[row, 8].Value = item.AverageScore;
                    worksheet.Cells[row, 9].Value = item.MinScore;
                    worksheet.Cells[row, 10].Value = item.MaxScore;
                    worksheet.Cells[row, 11].Value = item.StandardDeviation;
                    worksheet.Cells[row, 12].Value = item.Sum0_39;
                    worksheet.Cells[row, 13].Value = item.Sum40_49;
                    worksheet.Cells[row, 14].Value = item.Sum50_59;
                    worksheet.Cells[row, 15].Value = item.Sum60_69;
                    worksheet.Cells[row, 16].Value = item.Sum70_79;
                    worksheet.Cells[row, 17].Value = item.Count80Plus;

                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                using (var stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
        }
        public static string GenerateExcelBase64_Other(List<ExcelScoreModel_Other> data)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Score Report");

                // เพิ่ม Header
                worksheet.Cells[1, 1].Value = "รหัสวิชา";
                worksheet.Cells[1, 2].Value = "ชื่อวิชา";
                worksheet.Cells[1, 3].Value = "ปีการศึกษา";
                worksheet.Cells[1, 4].Value = "ภาคเรียน";
                worksheet.Cells[1, 5].Value = "หมู่เรียน";
                worksheet.Cells[1, 6].Value = "ประเภทคะแนน";
                worksheet.Cells[1, 7].Value = "จำนวนนิสิต";
                worksheet.Cells[1, 8].Value = "คะแนนเฉลี่ย";
                worksheet.Cells[1, 9].Value = "คะแนนต่ำสุด";
                worksheet.Cells[1, 10].Value = "คะแนนสูงสุด";
                worksheet.Cells[1, 11].Value = "ค่าเบี่ยงเบนมาตรฐาน";
                worksheet.Cells[1, 12].Value = "สัดส่วนคะแนนช่วง 0-9";
                worksheet.Cells[1, 13].Value = "สัดส่วนคะแนนช่วง 10-19";
                worksheet.Cells[1, 14].Value = "สัดส่วนคะแนนช่วง 20-29";
                worksheet.Cells[1, 15].Value = "สัดส่วนคะแนนช่วง 30-39";
                worksheet.Cells[1, 16].Value = "สัดส่วนคะแนนช่วง 40+";

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[row, 1].Value = item.SubjectId;
                    worksheet.Cells[row, 2].Value = item.SubjectName;
                    worksheet.Cells[row, 3].Value = item.AcademicYear;
                    worksheet.Cells[row, 4].Value = item.Semester;
                    worksheet.Cells[row, 5].Value = item.Section;
                    worksheet.Cells[row, 6].Value = item.ScoreType;
                    worksheet.Cells[row, 7].Value = item.StudentCount;
                    worksheet.Cells[row, 8].Value = item.AverageScore;
                    worksheet.Cells[row, 9].Value = item.MinScore;
                    worksheet.Cells[row, 10].Value = item.MaxScore;
                    worksheet.Cells[row, 11].Value = item.StandardDeviation;
                    worksheet.Cells[row, 12].Value = item.Sum0_9;
                    worksheet.Cells[row, 13].Value = item.Sum10_19;
                    worksheet.Cells[row, 14].Value = item.Sum20_29;
                    worksheet.Cells[row, 15].Value = item.Sum30_39;
                    worksheet.Cells[row, 16].Value = item.Count40Plus;

                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                using (var stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
        }
    }
}