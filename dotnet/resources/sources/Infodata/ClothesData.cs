using iTeffa.Globals;

namespace iTeffa.Infodata
{
    public class ClothesData
    {
        public ComponentItem Mask { get; set; }
        public ComponentItem Gloves { get; set; }
        public ComponentItem Torso { get; set; }
        public ComponentItem Leg { get; set; }
        public ComponentItem Bag { get; set; }
        public ComponentItem Feet { get; set; }
        public ComponentItem Accessory { get; set; }
        public ComponentItem Undershit { get; set; }
        public ComponentItem Bodyarmor { get; set; }
        public ComponentItem Decals { get; set; }
        public ComponentItem Top { get; set; }
        public ClothesData()
        {
            Mask = new ComponentItem(0, 0);
            Gloves = new ComponentItem(0, 0);
            Torso = new ComponentItem(15, 0);
            Leg = new ComponentItem(21, 0);
            Bag = new ComponentItem(0, 0);
            Feet = new ComponentItem(34, 0);
            Accessory = new ComponentItem(0, 0);
            Undershit = new ComponentItem(15, 0);
            Bodyarmor = new ComponentItem(0, 0);
            Decals = new ComponentItem(0, 0);
            Top = new ComponentItem(15, 0);
        }
    }
}
