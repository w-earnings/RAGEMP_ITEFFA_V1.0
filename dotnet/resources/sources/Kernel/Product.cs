namespace iTeffa.Kernel
{
    public class Product
    {
        public Product(int price, int left, int autosell, string name, bool ordered)
        {
            Price = price;
            Lefts = left;
            Autosell = autosell;
            Name = name;
            Ordered = ordered;
        }

        public int Price { get; set; }
        public int Lefts { get; set; }
        public int Autosell { get; set; }
        public string Name { get; set; }
        public bool Ordered { get; set; }
    }
}
