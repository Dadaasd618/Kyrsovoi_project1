using PersonnelManagement.Models;
using System.Data.SqlClient;

namespace PersonnelManagement.Data
{
    public class AuthService
    {
        public User Login(string login, string password)
        {
            using (var con = Db.CreateConnection())
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 КодПользователя, Логин, ФИО, Администратор
FROM dbo.Пользователи
WHERE Логин = @login AND Пароль = @pass AND Активен = 1;", con))
                {
                    cmd.Parameters.AddWithValue("@login", login.Trim());
                    cmd.Parameters.AddWithValue("@pass", password);

                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;
                        return new User
                        {
                            UserId = r.GetInt32(0),
                            Login = r.GetString(1),
                            FullName = r.GetString(2),
                            IsAdmin = r.GetBoolean(3)
                        };
                    }
                }
            }
        }
    }
}