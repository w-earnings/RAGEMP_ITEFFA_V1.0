namespace iTeffa.Models
{
    public class OldConfig
    {
        public string ServerName { get; set; } = "RP1";
        public string ServerNumber { get; set; } = "1";
        public bool VoIPEnabled { get; set; } = false;
        public bool RemoteControl { get; set; } = false;
        public bool DonateChecker { get; set; } = false;
        public bool DonateSaleEnable { get; set; } = false;
        public int PaydayMultiplier { get; set; } = 1;
        public int LastBonusMin { get; set; } = 120;
        public int ExpMultiplier { get; set; } = 1;
        public bool SCLog { get; set; } = false;
    }
}
