using Hr_System_Demo_3.Models;

public class DeductionDto
{
    public int Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public string Reason { get; set; }
    public decimal PenaltyAmount { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid HrId { get; set; }
    public DeductionState State { get; set; }
}
