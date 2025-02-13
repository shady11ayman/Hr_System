using Hr_System_Demo_3.Models;
using System.ComponentModel.DataAnnotations;

namespace Hr_System_Demo_3.lookups
{
    public class ShiftType
    {
        [Key]
        public int ShiftTypeId { get; set; }  // ✅ Ensure this matches Employee model

        [Required]
        public string Name { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }


}
