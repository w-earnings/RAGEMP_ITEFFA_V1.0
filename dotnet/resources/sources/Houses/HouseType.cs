using GTANetworkAPI;

namespace iTeffa.Houses
{
    public class HouseType
    {
        public string Name { get; }
        public Vector3 Position { get; }
        public string IPL { get; set; }
        public Vector3 PetPosition { get; }
        public float PetRotation { get; }

        public HouseType(string name, Vector3 position, Vector3 petpos, float rotation, string ipl = "")
        {
            Name = name;
            Position = position;
            IPL = ipl;
            PetPosition = petpos;
            PetRotation = rotation;
        }

        public void Create()
        {
            if (IPL != "") NAPI.World.RequestIpl(IPL);
        }
    }
}
