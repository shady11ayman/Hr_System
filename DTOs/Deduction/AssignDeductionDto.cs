namespace Hr_System_Demo_3.DTOs.Deduction
{
    public class AssignDeductionDto
    {
        public Guid EmployeeId { get; set; }
        public string Reason { get; set; }
        public decimal PenaltyAmount { get; set; }
    }
}