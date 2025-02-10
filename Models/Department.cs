namespace Hr_System_Demo_3.Models
{
    public class Department
    {
        public Guid deptId { get; set; }
        public string deptName { get; set; }

        public ICollection<Employee> Employees { get; set; }   
    }
}
