using System.Collections.Generic;

namespace iTeffa.Globals
{
    class Underwear
    {
        public Underwear(int top, int price, List<int> colors)
        {
            Top = top;
            Price = price;
            Colors = colors;
        }
        public Underwear(int top, int price, Dictionary<int, int> undershirtIDs, List<int> colors)
        {
            Top = top;
            UndershirtIDs = undershirtIDs;
            Price = price;
            Colors = colors;
        }

        public int Top { get; }
        public int Price { get; }
        public Dictionary<int, int> UndershirtIDs { get; } = new Dictionary<int, int>(); // key - тип undershirt'а, value - id-шник
        public List<int> Colors { get; }
    }
}
