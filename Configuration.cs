namespace BlogApi
{
    public static class Configuration
    {
        public static string JwtKey = "4d2d7148d3f1ef98864ba8b6ebff203c170e44b25187524b9d23467d94b93124";
        public static string ApiKeyName = "Api_key";
        public static string ApiKey = "testeApi";
        public static SmtpConfiguration Smtp = new();

        public class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; } = 25;
            public string UserName { get; set; }
            public string Password { get; set; }


        }
    }
}