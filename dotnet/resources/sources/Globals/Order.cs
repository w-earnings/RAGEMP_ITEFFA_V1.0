using Newtonsoft.Json;

namespace iTeffa.Globals
{
    public class Order
    {
        public Order(string name, int amount, bool taked = false)
        {
            Name = name;
            Amount = amount;
            Taked = taked;
        }

        public string Name { get; set; }
        public int Amount { get; set; }
        [JsonIgnore]
        public bool Taked { get; set; }
        [JsonIgnore]
        public int UID { get; set; }
    }
}
