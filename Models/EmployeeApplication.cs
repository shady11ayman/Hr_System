using Hr_System_Demo_3.lookups;

namespace Hr_System_Demo_3.Models
{
    public class EmployeeApplication
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } 
        public Guid deptId { get; set; }
        public string Role { get; set; }
        public Guid HrId { get; set; } 
        public string Status { get; set; } = "Pending"; 
        public string? RejectReason { get; set; } 
        public int ShiftTypeId { get; set; }
        public ShiftType? ShiftType { get; set; }
    }
}
