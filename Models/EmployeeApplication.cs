using Hr_System_Demo_3.lookups;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hr_System_Demo_3.Models
{
    public class EmployeeApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public Guid deptId { get; set; }

        [ForeignKey("deptId")]
        public  Department Department { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public Guid HrId { get; set; }

        [ForeignKey("HrId")]
        public virtual Manager HrManager { get; set; }

        public string Status { get; set; } = "Pending";
        public string? RejectReason { get; set; }

        [Required]
        public int ShiftTypeId { get; set; }

        [ForeignKey("ShiftTypeId")]
        public virtual ShiftType? ShiftType { get; set; }

        [Required]
        public decimal Salary { get; set; }

        public decimal NetSalary => Salary - (Salary * (InsuranceRate + TaxRate + MedicalInsuranceRate));

        [Required]
        public decimal InsuranceRate { get; set; } = 0.1m;

        [Required]
        public decimal TaxRate { get; set; } = 0.15m;

        [Required]
        public decimal MedicalInsuranceRate { get; set; } = 0.05m;

        public string ApprovalStatus { get; set; } = "PendingApproval";

        public Guid? ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        public virtual Manager? Manager { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


}
