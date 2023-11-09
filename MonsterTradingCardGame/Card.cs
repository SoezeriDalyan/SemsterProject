using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class Card
    {
        //Vielleicht Dann Monster und Spell trennen mit verärbung => um zu zeigen man kann es?
        //Attribut sowieso eigene Klasse => Handle who is affected by whom
        [JsonPropertyName("Id")]
        public string ID { get; private set; }
        [JsonPropertyName("Name")]
        public string Name { get; private set; }
        //[JsonPropertyName("Attributes")]
        //public string Atributes { get; private set; }//für jetzt string, List Attributes => speichern, Dass Attribute schwächen haben, gegen andere
        //[JsonPropertyName("Username")]
        //public string Typ { get; private set; }//für jetzt string, ändern auf enum
        [JsonPropertyName("Damage")]
        public double Damage { get; private set; }//auf const, bzw readonly machen

        public Card(string iD, string name, double damage)
        {
            ID = iD;
            Name = name;
            Damage = damage;
            //Atributes = atributes;
            //Typ = typ;
        }

        public override string? ToString()
        {

            return $"{ID} {Name} {Damage}";
        }
    }
}
