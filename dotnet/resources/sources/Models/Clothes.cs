using System.Collections.Generic;

namespace iTeffa.Globals
{
    class Clothes
    {
        public Clothes(int variation, List<int> colors, int price, int type = -1, int bodyArmor = -1)
        {
            Variation = variation;
            Colors = colors;
            Price = price;
            Type = type;
            BodyArmor = bodyArmor;
        }

        public int Variation { get; }
        public List<int> Colors { get; }
        public int Price { get; }
        public int Type { get; }
        public int BodyArmor { get; }
    }
}
