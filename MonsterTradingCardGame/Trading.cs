using System.Text.Json.Serialization;

namespace MonsterTradingCardGame
{
    internal class Trading
    {
        [JsonPropertyName("ID")]
        public string ID { get; private set; }
        [JsonPropertyName("CardToTrade")]
        public string CardToTrade { get; private set; }
        [JsonPropertyName("Type")]
        public Type Type { get; private set; }
        [JsonPropertyName("MinimumDamage")]
        public double MinimumDamage { get; private set; }

        [JsonPropertyName("Trader")]
        public string Trader { get; set; }

        [Newtonsoft.Json.JsonConstructor]
        public Trading(string iD, string cardToTrade, string type, double minimumDamage)
        {
            ID = iD;
            CardToTrade = cardToTrade;
            if (type.ToLower() == Type.Monster.ToString().ToLower())
            {
                Type = Type.Monster;
            }
            else
            {
                Type = Type.Spell;
            }
            MinimumDamage = minimumDamage;
        }

        public Trading(string iD, string cardToTrade, string wantedType, double wantedMinDamage, string trader)
        {
            ID = iD;
            CardToTrade = cardToTrade;
            if (wantedType.ToLower() == Type.Monster.ToString().ToLower())
            {
                Type = Type.Monster;
            }
            else
            {
                Type = Type.Spell;
            }
            MinimumDamage = wantedMinDamage;
            Trader = trader;
        }
    }
}
