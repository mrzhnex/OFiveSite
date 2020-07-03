namespace OFiveSite
{
    public class User
    {
        public string Login { get; set; }
        public ulong DiscordId { get; set; }
        public int BanCount { get; private set; }
        public int WarnCount { get; private set; }
        public int ReceiveRequestCount { get; private set; }
        public string Name { get; set; }
        public int AccessLevel { get; set; }
        public string DiscordMention { get; set; }

        public static User Guest = new User()
        {
            Login = "Guest",
            BanCount = 0,
            WarnCount = 0,
            ReceiveRequestCount = 0,
            Name = "Гость",
            DiscordId = 2,
            AccessLevel = 0
        };

        public static User Owner = new User()
        {
            Login = "Owner",
            BanCount = 0,
            WarnCount = 0,
            ReceiveRequestCount = 0,
            Name = "Владелец",
            DiscordId = 1,
            AccessLevel = 5
        };

        public User() { }

        public void SetAccessLevel(int AccessLevel)
        {
            if (AccessLevel > 5) { this.AccessLevel = 5; }
            else if (AccessLevel < 0) { this.AccessLevel = 0; }
            else { this.AccessLevel = AccessLevel; }
        }
        public void AddBanCount()
        {
            BanCount += 1;
        }
        public void RemoveBanCount()
        {
            BanCount -= 1;
        }
        public void AddWarnCount()
        {
            WarnCount += 1;
        }
        public void AddRecieveRequestCount()
        {
            ReceiveRequestCount += 1;
        }     
        public void SetName(string Name)
        {
            this.Name = Name;
        }
    }
}