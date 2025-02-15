﻿namespace Hr_System_Demo_3.Models
{
    public class Deduction
    {
        public int Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid DeptId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public string Reason { get; set; } // "Late Arrival", "Early Leave", or both
        public decimal PenaltyAmount { get; set; } // Deduction amount

        public Employee Employee { get; set; }
    }
}
