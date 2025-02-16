using Hr_System_Demo_3.lookups;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hr_System_Demo_3.Models
{
    public class Manager
    {
        public Guid ManagerId { get; set; }
        public string Name { get; set; }
        public string? Address { get; set; }
        public Guid Hr_Id { get; set; } //hr_id no 
        public int? PhoneNumber { get; set; }
        public int PositionId { get; set; }

        [ForeignKey("PositionId")]
        public Position? Position { get; set; }

        public Guid? DepartmentId { get; set; } // Nullable to allow soft deletion

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        public double? WorkHours { get; set; }
        public List<string> WorkingDays { get; set; } = new List<string>();

        public int ShiftTypeId { get; set; }
        [ForeignKey("ShiftTypeId")]
        public virtual ShiftType? ShiftType { get; set; }

        public int ContractTypeId { get; set; }
        [ForeignKey("ContractTypeId")]
        public virtual ContractType? ContractType { get; set; }

        public Guid? DirectManagerId { get; set; }

        [ForeignKey("DirectManagerId")]
        public virtual Manager? DirectManager { get; set; }

        public int LeaveTypeId { get; set; }

        [ForeignKey("LeaveTypeId")]
        public virtual LeaveType LeaveType { get; set; }

        public DateTime? ContractStart { get; set; }
        public DateTime? ContractEnd { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }

    /*
     As an admin I need to add a new manager by enter below fields:
1.	Code (Auto)
2.	Name (M)
3.	Address (String)
4.	Phone number (Number)
5.	Position (Drop down)
6.	Department
7.	Direct Manager (Drop Down filter by department)
8.	Working Days (Selection)
9.	Working hours(number)
10.	Shift Type (Lookup)
11.	Contract Type (Drop Down)
12.	Contract duration )number)
13.	Contract start from (M)
14.	Contract end on (M)
15.	Leaves Type and its number
16.	Upload his contract 

     */
}
