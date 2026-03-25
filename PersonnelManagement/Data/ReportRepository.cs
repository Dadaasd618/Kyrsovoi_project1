using PersonnelManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PersonnelManagement.Data
{
    public class ReportRepository
    {
        public List<HoursReportRow> GetHoursReport(DateTime start, DateTime end)
        {
            var list = new List<HoursReportRow>();
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT 
    e.КодСотрудника,
    (e.Фамилия + N' ' + e.Имя + N' ' + ISNULL(e.Отчество,'')) AS ФИО,
    d.Название AS Отдел,
    SUM(((DATEDIFF(MINUTE,'00:00',te.EndTime) - DATEDIFF(MINUTE,'00:00',te.StartTime) + 1440) % 1440) - te.BreakMinutes) / 60.0 AS Часов,
    r.BaseHourlyRate AS Ставка
FROM dbo.РабочиеЧасы te
JOIN dbo.Сотрудники e ON e.КодСотрудника = te.КодСотрудника
JOIN dbo.Отделы d ON d.КодОтдела = e.КодОтдела
JOIN dbo.Должности r ON r.КодДолжности = e.КодДолжности
WHERE te.ДатаРаботы BETWEEN @start AND @end
GROUP BY e.КодСотрудника, e.Фамилия, e.Имя, e.Отчество, d.Название, r.BaseHourlyRate
ORDER BY ФИО;", con))
                {
                    cmd.Parameters.AddWithValue("@start", start.Date);
                    cmd.Parameters.AddWithValue("@end", end.Date);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            double hours = Convert.ToDouble(r.GetValue(3));
                            decimal rate = Convert.ToDecimal(r.GetValue(4));
                            list.Add(new HoursReportRow
                            {
                                EmployeeId = r.GetInt32(0),
                                ФИО = r.GetString(1).Replace("  ", " ").Trim(),
                                Отдел = r.GetString(2),
                                ОтработаноЧасов = hours,
                                Ставка = rate,
                                Сумма = (decimal)hours * rate
                            });
                        }
                    }
                }
            }
            return list;
        }

        public List<LeaveReportRow> GetLeaveReport(DateTime start, DateTime end)
        {
            var list = new List<LeaveReportRow>();
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT 
    (e.Фамилия + N' ' + e.Имя + N' ' + ISNULL(e.Отчество,'')) AS ФИО,
    s.ВидСтатуса,
    s.ДатаНачала,
    s.ДатаОкончания,
    DATEDIFF(DAY, s.ДатаНачала, s.ДатаОкончания) + 1 AS Дней,
    s.Комментарий
FROM dbo.СтатусыСотрудников s
JOIN dbo.Сотрудники e ON e.КодСотрудника = s.КодСотрудника
WHERE s.ДатаНачала <= @end AND s.ДатаОкончания >= @start
ORDER BY s.ДатаНачала;", con))
                {
                    cmd.Parameters.AddWithValue("@start", start.Date);
                    cmd.Parameters.AddWithValue("@end", end.Date);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new LeaveReportRow
                            {
                                ФИО = r.GetString(0).Replace("  ", " ").Trim(),
                                Тип = r.GetString(1),
                                С = r.GetDateTime(2),
                                По = r.GetDateTime(3),
                                Дней = r.GetInt32(4),
                                Комментарий = r.IsDBNull(5) ? null : r.GetString(5)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public EmployeeTimeSummary GetEmployeeSummary(int employeeId, DateTime weekStart, DateTime monthStart)
        {
            using (var con = Db.CreateConnection())
            {
                con.Open();
                decimal rate = 0;
                using (var cmd = new SqlCommand("SELECT BaseHourlyRate FROM dbo.Должности WHERE КодДолжности = (SELECT КодДолжности FROM dbo.Сотрудники WHERE КодСотрудника = @id);", con))
                {
                    cmd.Parameters.AddWithValue("@id", employeeId);
                    var obj = cmd.ExecuteScalar();
                    if (obj != null) rate = (decimal)obj;
                }

                double weekHours = 0;
                using (var cmd = new SqlCommand(@"
SELECT SUM(((DATEDIFF(MINUTE,'00:00',EndTime) - DATEDIFF(MINUTE,'00:00',StartTime) + 1440) % 1440) - BreakMinutes) / 60.0
FROM dbo.РабочиеЧасы
WHERE КодСотрудника = @id AND ДатаРаботы BETWEEN @start AND @end;", con))
                {
                    cmd.Parameters.AddWithValue("@id", employeeId);
                    cmd.Parameters.AddWithValue("@start", weekStart.Date);
                    cmd.Parameters.AddWithValue("@end", weekStart.AddDays(6).Date);
                    var obj = cmd.ExecuteScalar();
                    if (obj != DBNull.Value) weekHours = Convert.ToDouble(obj);
                }

                double monthHours = 0;
                using (var cmd = new SqlCommand(@"
SELECT SUM(((DATEDIFF(MINUTE,'00:00',EndTime) - DATEDIFF(MINUTE,'00:00',StartTime) + 1440) % 1440) - BreakMinutes) / 60.0
FROM dbo.РабочиеЧасы
WHERE КодСотрудника = @id AND ДатаРаботы BETWEEN @start AND @end;", con))
                {
                    cmd.Parameters.AddWithValue("@id", employeeId);
                    cmd.Parameters.AddWithValue("@start", monthStart.Date);
                    cmd.Parameters.AddWithValue("@end", monthStart.AddMonths(1).AddDays(-1).Date);
                    var obj = cmd.ExecuteScalar();
                    if (obj != DBNull.Value) monthHours = Convert.ToDouble(obj);
                }

                string fullName = "";
                using (var cmd = new SqlCommand("SELECT Фамилия + ' ' + Имя + ' ' + ISNULL(Отчество,'') FROM dbo.Сотрудники WHERE КодСотрудника = @id;", con))
                {
                    cmd.Parameters.AddWithValue("@id", employeeId);
                    var obj = cmd.ExecuteScalar();
                    if (obj != null) fullName = obj.ToString().Replace("  ", " ").Trim();
                }

                return new EmployeeTimeSummary
                {
                    ФИО = fullName,
                    ЧасовЗаНеделю = weekHours,
                    ЧасовЗаМесяц = monthHours,
                    Ставка = rate,
                    ЗарплатаЗаНеделю = (decimal)weekHours * rate,
                    ЗарплатаЗаМесяц = (decimal)monthHours * rate
                };
            }
        }
    }
}