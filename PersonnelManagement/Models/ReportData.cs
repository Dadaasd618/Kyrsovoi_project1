using System;

namespace PersonnelManagement.Models
{
    public class HoursReportRow
    {
        public int EmployeeId { get; set; }
        public string ФИО { get; set; }
        public string Отдел { get; set; }
        public double ОтработаноЧасов { get; set; }
        public decimal Ставка { get; set; }
        public decimal Сумма { get; set; }
    }

    public class LeaveReportRow
    {
        public string ФИО { get; set; }
        public string Тип { get; set; }
        public DateTime С { get; set; }
        public DateTime По { get; set; }
        public int Дней { get; set; }
        public string Комментарий { get; set; }
    }

    public class EmployeeTimeSummary
    {
        public string ФИО { get; set; }
        public double ЧасовЗаНеделю { get; set; }
        public double ЧасовЗаМесяц { get; set; }
        public decimal Ставка { get; set; }
        public decimal ЗарплатаЗаНеделю { get; set; }
        public decimal ЗарплатаЗаМесяц { get; set; }
    }
}