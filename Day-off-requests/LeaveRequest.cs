using Hr_System_Demo_3.Models;

namespace Hr_System_Demo_3.Day_off_requests
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public DateTime LeaveFrom { get; set; }
        public DateTime LeaveTo { get; set; }
        public string? workingDaysOff { get; set; }
        public string? TotalDaysOff { get; set; }
        public int? DayOffRequestId { get; set; }
        public string? BackupName { get; set; }
        public string? comment { get; set; }
        public string? Action { get; set; }
        public string? RejectReason { get; set; }
        public LeaveStatus Status { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}
/*
1 - Employee Name(D)
2 - Leave Type(Drop Down)(D)
3 - Leave from(D)
4 - Leave To(D)
5 - Working days off(D)
6 - Total Days off(D)
7 - Backup name(D)
8 - Comment(D)
9 - Action(M)
10 - Rejected Reason(M)*/