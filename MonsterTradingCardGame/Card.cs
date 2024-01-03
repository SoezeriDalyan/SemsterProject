using System.Text.Json.Serialization;

namespace MonsterTradingCardGame
{
    public enum Attribut { Fire, Water, Normal };
    public enum Type { Monster, Spell };
    public class Card
    {
        [JsonPropertyName("Id")]
        public string ID { get; private set; }
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("Damage")]
        public double Damage { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Attribut Attribute { get; private set; }
        [Newtonsoft.Json.JsonIgnore]
        public Type Type { get; private set; }
        [Newtonsoft.Json.JsonIgnore]
        public double TempDamage { get; private set; }

        public Card(string iD, string name, double damage)
        {
            ID = iD;
            Name = name;
            Damage = damage;

            if (name.Contains("Spell"))
                this.Type = Type.Spell;
            else
                this.Type = Type.Monster;

            if (name.Contains("Water"))
                this.Attribute = Attribut.Water;
            else if (name.Contains("Fire"))
                this.Attribute = Attribut.Fire;
            else
                this.Attribute = Attribut.Normal;
        }

        public void EffectDamage(double factor)
        {
            TempDamage = Damage * factor;
        }

        public override string? ToString()
        {

            return $"ID({ID}) Name({Name}) Damage({Damage}) Attribut({Attribute.ToString()}) Type({Type.ToString()})";
        }

        public string BattleString()
        {
            return $"Name({Name}) Damage({Damage}) TempDamage({TempDamage})";
        }
    }
}
