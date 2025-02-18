using Hr_System_Demo_3.Models;

namespace Hr_System_Demo_3.DTOs.SalaryStatment
{
    public class SalaryStatementDto
    {
        public int Id { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime StatementDate { get; set; }
        public decimal TotalSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal SalaryAfterDeductions { get; set; } // This is the new field
        public SalaryStatementState State { get; set; } = SalaryStatementState.Pending;
        public Guid HrId { get; set; }
        public Guid? ManagerId { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public List<DeductionDto> Deductions { get; set; } = new List<DeductionDto>();

    }
}
