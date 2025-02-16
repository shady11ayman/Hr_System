using Hr_System_Demo_3.lookups;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hr_System_Demo_3.Models
{
    public class Employee
    {
        public Guid empId { get; set; }
        public string empName { get; set; }
        public string empEmail { get; set; }
        public string empPassword { get; set; }

        public string? Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public int PositionId { get; set; }

        [ForeignKey("PositionId")]
        public virtual Position Position { get; set; }

        public Guid deptId { get; set; }
        public Guid Hr_Id { get; set; }
        public string Role { get; set; }
        public Manager? Manager { get; set; }
        public Guid? ManagerId {  get; set; }
        public double salary { get; set; }
        public double WorkHours { get; set; }

        [ForeignKey("ShiftTypeId")]
        public int ShiftTypeId { get; set; }  
        public ShiftType ShiftType { get; set; }

        public int ContractTypeId { get; set; }

        [ForeignKey("ContractTypeId")]
        public virtual ContractType ContractType { get; set; }

        public int LeaveTypeId { get; set; }

        [ForeignKey("LeaveTypeId")]
        public virtual LeaveType LeaveType { get; set; }

        public DateTime ContractStart { get; set; }

        public DateTime ContractEnd { get; set; }

        public List<string> WorkingDays { get; set; } = new List<string>();

        public int ContractDuration { get; set; }

        public Department Department { get; set; }
    }


}
