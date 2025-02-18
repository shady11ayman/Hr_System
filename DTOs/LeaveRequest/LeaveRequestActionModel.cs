namespace Hr_System_Demo_3.DTOs.LeaveRequest
{
    public class LeaveRequestActionModel
    {
        public int LeaveRequestId { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectReason { get; set; }
    }
}