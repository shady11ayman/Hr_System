using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hr_System_Demo_3.Models
{
    public class Department
    {
        [Key]
        public Guid deptId { get; set; }

        public string deptName { get; set; }

        public Guid? ManagerId { get; set; }  // ✅ Make ManagerId nullable

        [ForeignKey("ManagerId")]
        public virtual Manager? Manager { get; set; }  // One-to-One Relationship

        public virtual ICollection<Employee>? Employees { get; set; }
    }
}
