namespace PersonnelManagement.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
    }
}