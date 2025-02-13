using Hr_System_Demo_3.lookups;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hr_System_Demo_3.Models
{
    public class Employee
    {
        public Guid empId { get; set; }
        public string empName { get; set; }
        public string empEmail { get; set; }
        public string empPassword { get; set; }
        public Guid deptId { get; set; }
        public Guid Hr_Id { get; set; }
        public string Role { get; set; }

        public int ShiftTypeId { get; set; }  
        public ShiftType ShiftType { get; set; }  

        public Department Department { get; set; }
    }


}
