namespace Hr_System_Demo_3.Authentication
{
    public class JwtOptions

    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Lifetime { get; set; }
        public string SigningKey { get; set; }
    }
}
