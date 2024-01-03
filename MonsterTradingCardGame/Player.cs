using System.Net.Sockets;

namespace MonsterTradingCardGame
{
    public class Player
    {
        //This class handles all necessary data of a player during a battle.
        //The TypeClient is intended as support for sending the data
        public string Name { get; set; }
        public TcpClient TcpClient { get; set; }
        public List<Card> Deck { get; private set; }

        public Player(TcpClient tcpClient, string username, List<Card> deck)
        {
            TcpClient = tcpClient;
            Name = username;
            Deck = deck;
        }

        public override bool Equals(object? obj)
        {
            return obj is Player player &&
                   Name == player.Name;
        }
    }
}
