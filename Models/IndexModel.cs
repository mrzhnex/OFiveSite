namespace OFiveSite.Models
{
    public class IndexModel
    {
        public string UserIp { get; set; }
        public UserState UserState { get; set; }
        public JoinState JoinState { get; set; }
        public string Message { get; set; }
        public string Login { get; set; }
        public string DiscordId { get; set; }
        public string Password { get; set; }
    }
}
