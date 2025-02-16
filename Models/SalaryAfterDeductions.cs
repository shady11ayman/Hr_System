namespace Hr_System_Demo_3.Models
{
    public class SalaryAfterDeductions
    {
        public int Id { get; set; }
        public Employee Employee { get; set; }
        public Guid empId { get; set; }
        public int DeductionId { get; set; }
        public Deduction Deduction { get; set; }
        public double Salary { get; set; }
        public double? Salaryafterchange { get; set; }
    }
}
