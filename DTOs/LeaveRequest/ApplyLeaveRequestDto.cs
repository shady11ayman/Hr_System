namespace Hr_System_Demo_3.DTOs.LeaveRequest
{
    public class ApplyLeaveRequestDto
    {
        public string LeaveType { get; set; }
        public DateTime LeaveFrom { get; set; }
        public DateTime LeaveTo { get; set; }
        public string comment { get; set; }
    }
}