using OFivePlugin;
using System.Linq;

namespace OFiveSite
{
    public class Session
    {
        public ulong Time { get; set; }
        public User User { get; set; }
        public Reason Reason { get; set; }

        public static Session New(User user)
        {
            return new Session()
            {
                Time = 0,
                User = user,
                Reason = Global.reasons.First()
            };
        }

        public void TickTime()
        {
            Time += 1;
        }

    }
}