using PersonnelManagement.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PersonnelManagement.Data
{
    public class EmployeeRepository
    {
        public List<Employee> GetAll()
        {
            var list = new List<Employee>();
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT e.КодСотрудника, e.КодОтдела, e.КодДолжности,
       e.Фамилия, e.Имя, ISNULL(e.Отчество,''),
       ISNULL(e.Телефон,''), ISNULL(e.Почта,''), e.Активен,
       d.Название AS Отдел, r.Название AS Должность
FROM dbo.Сотрудники e
JOIN dbo.Отделы d ON d.КодОтдела = e.КодОтдела
JOIN dbo.Должности r ON r.КодДолжности = e.КодДолжности
ORDER BY e.КодСотрудника;", con))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new Employee
                        {
                            EmployeeId = r.GetInt32(0),
                            DepartmentId = r.GetInt32(1),
                            RoleId = r.GetInt32(2),
                            LastName = r.GetString(3),
                            FirstName = r.GetString(4),
                            MiddleName = r.GetString(5),
                            Phone = r.GetString(6),
                            Email = r.GetString(7),
                            IsActive = r.GetBoolean(8),
                            DepartmentName = r.GetString(9),
                            RoleName = r.GetString(10)
                        });
                    }
                }
            }
            return list;
        }

        public void Insert(Employee e)
        {
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Сотрудники
(КодОтдела, КодДолжности, Фамилия, Имя, Отчество, Телефон, Почта, ДатаПриема, Активен)
VALUES
(@dep, @role, @ln, @fn, NULLIF(@mn,''), NULLIF(@ph,''), NULLIF(@em,''), CAST(GETDATE() AS DATE), @active);", con))
                {
                    cmd.Parameters.AddWithValue("@dep", e.DepartmentId);
                    cmd.Parameters.AddWithValue("@role", e.RoleId);
                    cmd.Parameters.AddWithValue("@ln", e.LastName);
                    cmd.Parameters.AddWithValue("@fn", e.FirstName);
                    cmd.Parameters.AddWithValue("@mn", e.MiddleName ?? "");
                    cmd.Parameters.AddWithValue("@ph", e.Phone ?? "");
                    cmd.Parameters.AddWithValue("@em", e.Email ?? "");
                    cmd.Parameters.AddWithValue("@active", e.IsActive);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Update(Employee e)
        {
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
UPDATE dbo.Сотрудники
SET КодОтдела=@dep, КодДолжности=@role,
    Фамилия=@ln, Имя=@fn, Отчество=NULLIF(@mn,''),
    Телефон=NULLIF(@ph,''), Почта=NULLIF(@em,''),
    Активен=@active
WHERE КодСотрудника=@id;", con))
                {
                    cmd.Parameters.AddWithValue("@dep", e.DepartmentId);
                    cmd.Parameters.AddWithValue("@role", e.RoleId);
                    cmd.Parameters.AddWithValue("@ln", e.LastName);
                    cmd.Parameters.AddWithValue("@fn", e.FirstName);
                    cmd.Parameters.AddWithValue("@mn", e.MiddleName ?? "");
                    cmd.Parameters.AddWithValue("@ph", e.Phone ?? "");
                    cmd.Parameters.AddWithValue("@em", e.Email ?? "");
                    cmd.Parameters.AddWithValue("@active", e.IsActive);
                    cmd.Parameters.AddWithValue("@id", e.EmployeeId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SoftDeactivate(int employeeId)
        {
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand("UPDATE dbo.Сотрудники SET Активен=0 WHERE КодСотрудника=@id;", con))
                {
                    cmd.Parameters.AddWithValue("@id", employeeId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}