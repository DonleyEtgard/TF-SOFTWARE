namespace ZONAUTO.config
{
    public class EmailSettings
    {
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
