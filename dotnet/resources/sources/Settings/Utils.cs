using GTANetworkAPI;
using iTeffa.Infodata;
using iTeffa.Plugins;
using iTeffa.Models;
using iTeffa.Globals;

namespace iTeffa.Settings
{
    public class Utils
    {
        public static AccountData GetAccount(Player client)
        {
            return client.GetData<AccountData>("AccData");
        }
        public static CharacterData GetCharacter(Player client)
        {
            return client.GetData<CharacterData>("CharData");
        }
    }
}
