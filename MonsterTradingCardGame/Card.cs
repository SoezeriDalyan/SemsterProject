﻿using System.Text.Json.Serialization;

namespace MonsterTradingCardGame
{
    public enum Attribut { Fire, Water, Normal };
    public enum Type { Monster, Spell };
    internal class Card
    {
        [JsonPropertyName("Id")]
        public string ID { get; private set; }
        [JsonPropertyName("Name")]
        public string Name { get; private set; }
        [JsonPropertyName("Damage")]
        public double Damage { get; private set; }

        public Attribut Attribute { get; private set; }

        public Type Type { get; private set; }

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

        public override string? ToString()
        {

            return $"{ID} {Name} {Damage} {Attribute} {Type}";
        }
    }
}
