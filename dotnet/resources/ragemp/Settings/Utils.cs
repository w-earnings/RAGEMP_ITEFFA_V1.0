using GTANetworkAPI;

namespace iTeffa.Settings
{
    public class Utils
    {
        public static AccountData GetAccount(Player client)
        {
            //client.GetExternalData<AccountData>(0);
            //return new AccountData();
            return client.GetData<AccountData>("AccData");
        }
        public static CharacterData GetCharacter(Player client)
        {
            //client.GetExternalData<CharacterData>(1);
            //return new CharacterData();
            return client.GetData<CharacterData>("CharData");
        }
    }
}
