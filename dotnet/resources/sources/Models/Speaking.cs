using GTANetworkAPI;

namespace iTeffa.Models
{
    struct VoiceMetaData
    {
        public bool IsEnabledMicrophone;
        public string RadioRoom;
        public string StateConnection;
        public int MicrophoneKey;
    }
    struct VoicePhoneMetaData
    {
        public Player Target;
        public string CallingState;
    }
}
