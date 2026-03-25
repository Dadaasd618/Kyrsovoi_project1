using PersonnelManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PersonnelManagement.Data
{
    public class TimeEntryRepository
    {
        public List<TimeEntry> GetLast(int days = 30)
        {
            return GetByPeriod(DateTime.Today.AddDays(-days), DateTime.Today);
        }

        public List<TimeEntry> GetByPeriod(DateTime start, DateTime end)
        {
            var list = new List<TimeEntry>();
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT te.КодЗаписи, te.КодСотрудника, te.КодТипаСмены,
       te.ДатаРаботы, te.StartTime, te.EndTime, te.BreakMinutes, ISNULL(te.Комментарий,''),
       (e.Фамилия + N' ' + e.Имя + N' ' + ISNULL(e.Отчество,'')) AS ФИО,
       st.Название AS ТипСмены
FROM dbo.РабочиеЧасы te
JOIN dbo.Сотрудники e ON e.КодСотрудника = te.КодСотрудника
JOIN dbo.ТипыСмен st ON st.КодТипаСмены = te.КодТипаСмены
WHERE te.ДатаРаботы BETWEEN @start AND @end
ORDER BY te.ДатаРаботы DESC;", con))
                {
                    cmd.Parameters.AddWithValue("@start", start.Date);
                    cmd.Parameters.AddWithValue("@end", end.Date);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new TimeEntry
                            {
                                TimeEntryId = r.GetInt32(0),
                                EmployeeId = r.GetInt32(1),
                                ShiftTypeId = r.GetInt32(2),
                                WorkDate = r.GetDateTime(3),
                                StartTime = r.GetTimeSpan(4),
                                EndTime = r.GetTimeSpan(5),
                                BreakMinutes = r.GetInt32(6),
                                Comment = r.GetString(7),
                                EmployeeName = r.GetString(8).Replace("  ", " ").Trim(),
                                ShiftName = r.GetString(9)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void Insert(TimeEntry t)
        {
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.РабочиеЧасы(КодСотрудника, КодТипаСмены, ДатаРаботы, StartTime, EndTime, BreakMinutes, Комментарий)
VALUES(@emp, @shift, @date, @start, @end, @break, NULLIF(@c,''));", con))
                {
                    cmd.Parameters.AddWithValue("@emp", t.EmployeeId);
                    cmd.Parameters.AddWithValue("@shift", t.ShiftTypeId);
                    cmd.Parameters.AddWithValue("@date", t.WorkDate.Date);
                    cmd.Parameters.AddWithValue("@start", t.StartTime);
                    cmd.Parameters.AddWithValue("@end", t.EndTime);
                    cmd.Parameters.AddWithValue("@break", t.BreakMinutes);
                    cmd.Parameters.AddWithValue("@c", t.Comment ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}