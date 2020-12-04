using System;
using System.Collections.Generic;

namespace iTeffa.Infodata
{
    public class AccountData
    {
        public string Login { get; protected set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string HWID { get; protected set; }
        public string IP { get; protected set; }
        public string SocialClub { get; protected set; }
        public long Coins { get; set; }
        public int VipLvl { get; set; }
        public DateTime VipDate { get; set; } = DateTime.Now;
        public List<string> PromoCodes { get; set; }
        public List<int> Characters { get; protected set; }
        public bool PresentGet { get; set; } = false;
    }
}