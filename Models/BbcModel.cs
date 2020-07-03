using OFivePlugin;

namespace OFiveSite.Models
{
    public class BbcModel
    {
        public string UserIp { get; set; }
        public string Message { get; set; }
        public Broadcast Broadcast { get; set; }
        public ServerInfo Server { get; set; }
        public Player Player { get; set; }
        public Team Team { get; set; }
        public int Time { get; set; }
        public string Text { get; set; }
        public string colors { get; set; }
    }

    public enum BroadcastType
    {
        Bc,
        Pbc,
        Tbc,
        Dbc,
        Sbc,
        Gbc
    }

    public class Style
    {
        public int StartIndex { get; set; }
        public int Lenght { get; set; }
        public string Color { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public int Size { get; set; }
    }

    public class Team
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public Smod2.API.Team SmodTeam { get; set; }
        public Team(string Name, int Id, Smod2.API.Team SmodTeam)
        {
            this.Name = Name;
            this.Id = Id;
            this.SmodTeam = SmodTeam;
        }
    }

    public class Broadcast
    {
        public string Name { get; set; }
        public BroadcastType BroadcastType { get; set; }
        public Broadcast(string Name, BroadcastType BroadcastType)
        {
            this.Name = Name;
            this.BroadcastType = BroadcastType;
        }
    }

}