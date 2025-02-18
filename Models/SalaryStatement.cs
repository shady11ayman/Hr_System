namespace Hr_System_Demo_3.Models
{
    public class SalaryStatement
    {
        public int Id { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime StatementDate { get; set; }
        public decimal TotalSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public SalaryStatementState State { get; set; } = SalaryStatementState.Pending;
        public Guid HrId { get; set; }
        public Guid? ManagerId { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        // Add a collection of deductions
        public List<Deduction> Deductions { get; set; } = new List<Deduction>();

        public Employee Employee { get; set; }
    }

    public enum SalaryStatementState
    {
        Pending,
        Submitted,
        Approved,
        Rejected,
        Paid
    }
}