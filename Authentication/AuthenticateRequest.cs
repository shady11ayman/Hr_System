namespace Hr_System_Demo_3.Authentication
{
    public class AuthenticateRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Guid deptId { get; set; }
      //  public Guid Hr_Id { get; set; }
    }
}
