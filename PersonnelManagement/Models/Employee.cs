using System;

namespace PersonnelManagement.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public int RoleId { get; set; }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }

        public string DepartmentName { get; set; }
        public string RoleName { get; set; }

        public string FullName => (LastName + " " + FirstName + " " + MiddleName).Replace("  ", " ").Trim();
    }
}