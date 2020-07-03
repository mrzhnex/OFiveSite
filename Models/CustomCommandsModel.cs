using System.Collections.Generic;
using OFivePlugin;

namespace OFiveSite.Models
{
    public class CustomCommandsModel
    {
        public string UserIp { get; set; }
        public string Message { get; set; }
        public ServerInfo Server { get; set; }
        public Player Player { get; set; }
        public Dictionary<Command, CommandInfo> CommandInfo = Global.CommandInfo;
    }

    public class Command
    {
        private Command Type { get; set; }
        private string Name { get; set; }
        public string GetName() { return Name; }
        public void SetType(Command Type)
        {
            this.Type = Type;
            Name = Global.CommandInfo[this.Type].Name;
        }   
    }

    public class TranqillMaster : Command
    {
        public int BulletCount { get; set; }
        public TranqillType TranqillType { get; set; }
        public TranqillMaster()
        {
            SetType(this);
        }
    }

    public enum TranqillType
    {
        FRP,
        MTF
    }

    public class CommandInfo
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int AccessLevel { get; private set; }
        public CommandInfo(string Name, string Description, int AccessLevel)
        {
            this.Name = Name;
            this.Description = Description;
            this.AccessLevel = AccessLevel;
        }
        public CommandInfo() { }
    }


}