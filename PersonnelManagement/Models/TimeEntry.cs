using System;

namespace PersonnelManagement.Models
{
    public class TimeEntry
    {
        public int TimeEntryId { get; set; }
        public int EmployeeId { get; set; }
        public int ShiftTypeId { get; set; }

        public DateTime WorkDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int BreakMinutes { get; set; }
        public string Comment { get; set; }

        public string EmployeeName { get; set; }
        public string ShiftName { get; set; }

        public double HoursWorked => ((EndTime - StartTime).TotalMinutes - BreakMinutes) / 60.0;
    }
}