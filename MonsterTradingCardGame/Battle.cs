using System.Net.Sockets;
using System.Text;

namespace MonsterTradingCardGame
{
    internal class Battle
    {
        public DB DatabaseConnection { get; private set; }
        public List<Player> Players { get; private set; }
        private Random rnd = new Random();
        private Card c1;
        private Card c2;

        public Battle(DB databaseConnection, List<Player> players)
        {
            DatabaseConnection = databaseConnection;
            Players = players;

            BeginnTheBattle();
        }


        public void BeginnTheBattle()
        {
            Player p1 = Players[0];
            Player p2 = Players[1];
            Card roundWinner = null;

            SendToBoth("Let the battle beginn");

            int rounds = 1;
            while (rounds < 100)
            {
                c1 = p1.Deck[rnd.Next(p1.Deck.Count)];
                c2 = p2.Deck[rnd.Next(p2.Deck.Count)];

                string fight = $"\n\n({p1.Name}): {c1.BattleString()} fights against ({p2.Name}): {c2.BattleString()}";

                Console.WriteLine(fight);
                SendToBoth(fight);

                roundWinner = CheckRoundWinner(p1.Name, p2.Name);

                if (roundWinner != null)
                {
                    //check if roundwinner = c1 oder c2 dann add looser to other player and change the name in the card
                    if (roundWinner == c1)
                    {
                        p2.Deck.Remove(c2);
                        p1.Deck.Add(c2);

                        Console.WriteLine($"Winner in Round {rounds}: " + p1.Name);
                        SendToBoth($"\nWinner in Round {rounds}: " + p1.Name);
                    }
                    else
                    {
                        p1.Deck.Remove(c1);
                        p2.Deck.Add(c1);

                        Console.WriteLine($"Winner in Round {rounds}: " + p2.Name);
                        SendToBoth($"\nWinner in Round {rounds}: " + p2.Name);
                    }

                    Console.WriteLine(p1.Deck.Count);
                    Console.WriteLine(p2.Deck.Count);
                }
                else
                {
                    SendToBoth($"\nWinner in Round {rounds}: none");
                    Console.WriteLine($"Winner in Round {rounds}: none");
                }

                //Check if Decks are not 0 for each player

                if (p1.Deck.Count == 0)
                {
                    //SendtoBoth and Calculate ELO
                    Console.WriteLine("Winner = Player2: " + p2.Name);
                    SendToBoth("\nWinner = Player2: " + p2.Name);
                    break;
                }

                if (p2.Deck.Count == 0)
                {
                    //SendtoBoth and Calculate ELO
                    Console.WriteLine("Winner = Player1: " + p1.Name);
                    SendToBoth("\nWinner = Player1: " + p1.Name);
                    break;
                }



                rounds++;
            }

            if (rounds == 100)
            {
                //SendtoBoth and Calculate ELO
                Console.WriteLine("Oh no, its a Draw");
                SendToBoth("\nOh no, its a Draw");
            }

        }

        public Card CheckRoundWinner(string n1, string n2)
        {
            Card winner = null;

            //check effectivness

            Effectivnes();

            Console.WriteLine("After Effect");
            string fightAfterEffect = $"\n({n1}): {c1.BattleString()} fights against ({n2}): {c2.BattleString()}";

            Console.WriteLine(fightAfterEffect);
            SendToBoth("\nAfter Effect\n" + fightAfterEffect);

            //Check special Cases
            Card c = SpecialCase();
            if (c != null)
            {
                return c;
            }
            else
            {
                //am ende wer mehr damage
                if (c1.TempDamage > c2.TempDamage)
                    return c1;
                else if (c2.TempDamage > c1.TempDamage)
                    return c2;
            }

            return null;
        }

        public void Effectivnes()
        {
            if (CheckEffectivnes(c1, c2))
            {
                Console.WriteLine("Card 1 to Card 2" + CheckEffectivnes(c1, c2));
                SendToBoth("\nCard 1 Effects Card 2 ");
                c1.EffectDamage(2);
                c2.EffectDamage(0.5);

            }
            else if (CheckEffectivnes(c2, c1))
            {
                Console.WriteLine("Card 2 to Card 1" + CheckEffectivnes(c2, c1));
                SendToBoth("\nCard 2 to Card 1");
                c1.EffectDamage(0.5);
                c2.EffectDamage(2);
            }
            else
            {
                c1.EffectDamage(1);
                c2.EffectDamage(1);
            }

        }

        public bool CheckEffectivnes(Card c1, Card c2)
        {
            if (c1.Attribute == Attribut.Water && c2.Attribute == Attribut.Fire)
            {
                return true;
            }

            if (c1.Attribute == Attribut.Fire && c2.Attribute == Attribut.Normal)
            {
                return true;
            }

            if (c1.Attribute == Attribut.Normal && c2.Attribute == Attribut.Water)
            {
                return true;
            }

            return false;
        }

        public Card SpecialCase()
        {

            if (CheckSpecialCase(c1, c2))
            {
                Console.WriteLine("Card 1 to Card 2" + CheckSpecialCase(c1, c2));
                SendToBoth("\nCard 2 cant win against Card 1");
                return c1;

            }

            if (CheckSpecialCase(c2, c1))
            {
                Console.WriteLine("Card 2 to Card 1" + CheckSpecialCase(c2, c1));
                SendToBoth("\nCard 1 cant win against Card 2\n");
                return c2;
            }

            return null;
        }

        public bool CheckSpecialCase(Card c1, Card c2)
        {
            if (c1.Name == "Dragon" && c2.Name.Contains("Goblin"))
            {
                return true;
            }

            //Es gibt kein Wizzard und Kraken auch nicht??
            if (c1.Type == Type.Spell && c1.Attribute == Attribut.Water && c2.Name == "Knight")
            {
                return true;
            }

            return false;
        }

        private void SendToBoth(string message)
        {
            SendMessages(Players[0].TcpClient, message);
            SendMessages(Players[1].TcpClient, message);
        }

        private void SendMessages(TcpClient tcpClient, string message)
        {
            var networkStream = tcpClient.GetStream();
            var responseBytes = Encoding.UTF8.GetBytes(message);
            networkStream.Write(responseBytes, 0, responseBytes.Length);
        }
    }
}
