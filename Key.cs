namespace OFiveSite
{
    public class Key
    {
        public string Password { get; set; }
        public int AliveTime { get; set; }
        public Key(string Password, int AliveTime)
        {
            this.Password = Password;
            this.AliveTime = AliveTime;
        }
    }
}