using PersonnelManagement.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PersonnelManagement.Data
{
    public class LookupRepository
    {
        public List<LookupItem> GetDepartments()
        {
            var list = new List<LookupItem>();
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT КодОтдела, Название FROM dbo.Отделы ORDER BY Название;", con))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(new LookupItem { Id = r.GetInt32(0), Name = r.GetString(1) });
                }
            }
            return list;
        }

        public List<LookupItem> GetRoles()
        {
            var list = new List<LookupItem>();
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT КодДолжности, Название FROM dbo.Должности ORDER BY Название;", con))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(new LookupItem { Id = r.GetInt32(0), Name = r.GetString(1) });
                }
            }
            return list;
        }

        public List<LookupItem> GetEmployees()
        {
            var list = new List<LookupItem>();
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT КодСотрудника, (Фамилия + N' ' + Имя + N' ' + ISNULL(Отчество,'')) AS ФИО
FROM dbo.Сотрудники
WHERE Активен = 1
ORDER BY Фамилия, Имя;", con))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(new LookupItem { Id = r.GetInt32(0), Name = r.GetString(1).Replace("  ", " ").Trim() });
                }
            }
            return list;
        }

        public List<LookupItem> GetShiftTypes()
        {
            var list = new List<LookupItem>();
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT КодТипаСмены, Название FROM dbo.ТипыСмен ORDER BY Название;", con))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(new LookupItem { Id = r.GetInt32(0), Name = r.GetString(1) });
                }
            }
            return list;
        }
    }
}