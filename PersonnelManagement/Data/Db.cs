using System.Configuration;
using System.Data.SqlClient;

namespace PersonnelManagement.Data
{
    public static class Db
    {
        public static string ConnectionString
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["Db"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(cs))
                    throw new System.InvalidOperationException("Строка подключения не найдена.");
                return cs;
            }
        }

        public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);
    }
}