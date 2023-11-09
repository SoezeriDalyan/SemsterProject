using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class Pack
    {
        //Schau ma mal
        public List<List<Card>> Packs { get; private set; }

        public Pack() {
            Packs = new List<List<Card>>();
        }
        
        //Authorization einfügen
        public void Add(List<Card> cards) => Packs.Add(cards);

        public override string? ToString()
        {
            string retString = "";
            foreach (var item in Packs)
            {
                retString += "\n\nPack:\n";
                retString += String.Join("\n", item.Select(x=>x.ToString()));
            }
            return retString;
        }
    }
}
