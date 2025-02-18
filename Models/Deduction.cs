namespace Hr_System_Demo_3.Models
{
    public class Deduction
    {
        public int Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public Guid DeptId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public string Reason { get; set; } 
        public decimal PenaltyAmount { get; set; }
        public Manager? Manager { get; set; }
        public Guid? ManagerId { get; set; }
        public Guid HrId { get; set; }
        public DeductionState state { get; set; }
        public bool isFinalized { get; set; }
        // Add a reference to the SalaryStatement
        public int? SalaryStatementId { get; set; }
        public SalaryStatement? SalaryStatement { get; set; }
    }
}
