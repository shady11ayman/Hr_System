using Hr_System_Demo_3.lookups;

namespace Hr_System_Demo_3.DTOs.Auth
{
    public class HrRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Guid deptId { get; set; }
        public int PositionId { get; set; }
        public Guid Hr_Id { get; set; }
        public int ShiftTypereq { get; set; }
        public string PhoneNumber { get; set; }

        // New fields:
        public decimal Salary { get; set; }  // Gross salary
        public decimal InsuranceRate { get; set; }  // Percentage for insurance deduction
        public decimal TaxRate { get; set; }  // Percentage for tax deduction
        public decimal MedicalInsuranceRate { get; set; }  // Percentage for medical insurance
        public Guid ManagerId { get; set; }  // General Manager for approval
    }
}
