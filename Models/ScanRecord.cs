namespace Hr_System_Demo_3.Models
{
    public class ScanRecord
    {
        public int Id { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime Date { get; set; } 
        public DateTime? EntryTime { get; set; } 
        public DateTime? ExitTime { get; set; } 

        
        public Employee Employee { get; set; }
    }
}
