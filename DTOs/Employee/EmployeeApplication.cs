namespace Hr_System_Demo_3.DTOs.Employee
{
    public class EmployeeApplication
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public Guid deptId { get; set; }
        public Guid HrId { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public int ShiftTypeId { get; set; }
        public decimal Salary { get; set; }
        public decimal InsuranceRate { get; set; }
        public decimal TaxRate { get; set; }
        public decimal MedicalInsuranceRate { get; set; }
        public string ApprovalStatus { get; set; }
        public Guid? ManagerId { get; set; }
        public string PhoneNumber { get; set; }
    }
}