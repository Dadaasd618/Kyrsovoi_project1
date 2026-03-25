using PersonnelManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PersonnelManagement.Data
{
    public class StatusRepository
    {
        public List<EmployeeStatus> GetActiveStatuses(DateTime date)
        {
            var list = new List<EmployeeStatus>();
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT s.КодСтатуса, s.КодСотрудника, s.ВидСтатуса, s.ДатаНачала, s.ДатаОкончания, s.Комментарий,
       (e.Фамилия + N' ' + e.Имя + N' ' + ISNULL(e.Отчество,'')) AS ФИО
FROM dbo.СтатусыСотрудников s
JOIN dbo.Сотрудники e ON e.КодСотрудника = s.КодСотрудника
WHERE @date BETWEEN s.ДатаНачала AND s.ДатаОкончания
ORDER BY s.ДатаНачала;", con))
                {
                    cmd.Parameters.AddWithValue("@date", date.Date);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new EmployeeStatus
                            {
                                StatusId = r.GetInt32(0),
                                EmployeeId = r.GetInt32(1),
                                StatusType = r.GetString(2),
                                StartDate = r.GetDateTime(3),
                                EndDate = r.GetDateTime(4),
                                Comment = r.IsDBNull(5) ? null : r.GetString(5),
                                EmployeeName = r.GetString(6).Replace("  ", " ").Trim()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void AddStatus(EmployeeStatus status)
        {
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.СтатусыСотрудников(КодСотрудника, ВидСтатуса, ДатаНачала, ДатаОкончания, Комментарий)
VALUES(@emp, @type, @start, @end, @comment);", con))
                {
                    cmd.Parameters.AddWithValue("@emp", status.EmployeeId);
                    cmd.Parameters.AddWithValue("@type", status.StatusType);
                    cmd.Parameters.AddWithValue("@start", status.StartDate);
                    cmd.Parameters.AddWithValue("@end", status.EndDate);
                    cmd.Parameters.AddWithValue("@comment", status.Comment ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}