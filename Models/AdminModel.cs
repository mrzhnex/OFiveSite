using OFivePlugin;

namespace OFiveSite.Models
{
    public class AdminModel
    {
        public string UserIp { get; set; }
        public string Message { get; set; }
        public ServerInfo Server { get; set; }
        public Player Player { get; set; }
        public Reason Reason { get; set; }
        public string CustomReason { get; set; }
        public int Time { get; set; }
        public string Comment { get; set; }

        public AdminModel() { }
    }
}