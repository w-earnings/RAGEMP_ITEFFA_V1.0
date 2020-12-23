using GTANetworkAPI;
using iTeffa.Infodata;

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
