using Hr_System_Demo_3.lookups;

namespace Hr_System_Demo_3.Authentication
{
    public class HrRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Guid deptId { get; set; }
        public string Role { get; set; }
        //  public Guid Hr_Id { get; set; }
        public int ShiftTypereq { get; set; }
        
    }
}
