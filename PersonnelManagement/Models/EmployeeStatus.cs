using System;

namespace PersonnelManagement.Models
{
    public class EmployeeStatus
    {
        public int StatusId { get; set; }
        public int EmployeeId { get; set; }
        public string StatusType { get; set; } // "Отпуск", "Больничный", "Командировка"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Comment { get; set; }

        public string EmployeeName { get; set; }
    }
}