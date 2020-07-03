using OFivePlugin;

namespace OFiveSite.Models
{
    public class OfflineModel
    {
        public string UserIp { get; set; }
        public string Message { get; set; }
        public string SteamId { get; set; }
        public Reason Reason { get; set; }
        public string CustomReason { get; set; }
        public int Time { get; set; }
        public string Comment { get; set; }

        public OfflineModel() { }
    }
}
