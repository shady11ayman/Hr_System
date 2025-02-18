using Hr_System_Demo_3.Models;

namespace Hr_System_Demo_3.DTOs.Deduction
{
    public class DeductionActionRequest
    {
        public int DeductionId { get; set; }
        public DeductionState Action { get; set; }
    }
}