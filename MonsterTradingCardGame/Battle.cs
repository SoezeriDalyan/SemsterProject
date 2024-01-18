using System.Net.Sockets;
using System.Text;

namespace MonsterTradingCardGame
{
    public class Battle
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
        }

        /// <summary>
        /// Handles the Battle funktionalität and orchestrate all methods and parameters.
        /// </summary>
        /// <returns>yet to be transformed response Message</returns>
        public string BeginnTheBattle()
        {
            Player p1 = Players[0];
            Player p2 = Players[1];
            Card roundWinner = null;
            string roundWinnerName = String.Empty;
            //Seperator is there for a nice output on the console
            string seperator = "\n//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////";

            //Signal start to players
            SendToBoth("Let the battle beginn");

            int rounds = 1;
            while (rounds <= 100) //max 100 rounds otherwise draw
            {
                //get a random Card from Deck
                c1 = p1.Deck[rnd.Next(p1.Deck.Count)];
                c2 = p2.Deck[rnd.Next(p2.Deck.Count)];

                //the fight between the chosen cards
                string fight = $"{seperator}\nRound: {rounds}\n({p1.Name}): {c1.BattleString()} fights against ({p2.Name}): {c2.BattleString()}";

                Console.WriteLine(fight);
                SendToBoth(fight);

                //checks the outcome of the fight
                roundWinner = CheckRoundWinner(p1.Name, p2.Name);

                //null = no winner in round
                if (roundWinner != null)
                {
                    //check if roundwinner = c1 or c2
                    //Then add looser card to winner player
                    if (roundWinner == c1)
                    {
                        p2.Deck.Remove(c2);
                        p1.Deck.Add(c2);
                        roundWinnerName = p1.Name;
                    }
                    else
                    {
                        p1.Deck.Remove(c1);
                        p2.Deck.Add(c1);
                        roundWinnerName = p2.Name;
                    }

                    //Sends outcome to players
                    Console.WriteLine($"Winner in Round {rounds}: " + roundWinnerName);
                    SendToBoth($"\n\nWinner in Round {rounds}: " + roundWinnerName + seperator + "\n");

                    Console.WriteLine(p1.Deck.Count);
                    Console.WriteLine(p2.Deck.Count);

                    //Shows how many cards each player has left
                    SendToBoth($"\n\n{p1.Name} has {p1.Deck.Count} cards \n{p2.Name} has {p2.Deck.Count} cards\n\n");
                }
                else
                {
                    //Sends outcome to players
                    SendToBoth($"\n\nWinner in Round {rounds}: none{seperator}" + "\n");
                    Console.WriteLine($"Winner in Round {rounds}: none");
                }

                //Check if Decks are not 0 for each player => ergo checks who the looser is
                if (p1.Deck.Count == 0)
                {
                    Console.WriteLine("Winner = Player2: " + p2.Name);
                    UpdateElo(p2, p1); //update Elo
                    return $"Winner = {p2.Name}"; //returns outcome
                }

                if (p2.Deck.Count == 0)
                {
                    Console.WriteLine("Winner = Player1: " + p1.Name);
                    UpdateElo(p1, p2); //update Elo
                    return $"Winner = {p1.Name}"; //returns outcome
                }

                Thread.Sleep(500);

                rounds++;
            }

            if (rounds == 101)
            {
                //no need for Elo => draw nothing happens
                Console.WriteLine("Oh no, its a Draw");
                SendToBoth("\nOh no, its a Draw\n");
            }

            return "Its a draw";
        }

        /// <summary>
        /// Updates Elo for winner and looser
        /// +3 Winner
        /// -5 Looser
        /// </summary>
        /// <param name="winner"></param>
        /// <param name="looser"></param>
        public void UpdateElo(Player winner, Player looser)
        {
            if (Players[0].TcpClient != null && Players[1].TcpClient != null)
            {
                DatabaseConnection.UpdateElo(winner, looser);
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
            SendToBoth("\nAfter Effect" + fightAfterEffect);

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

        /// <summary>
        /// Goes through every possibility for effects that can happen between two cards.
        /// </summary>
        /// <returns>nothing, but effect the card depending on output</returns>
        public void Effectivnes()
        {
            if (CheckEffectivnes(c1, c2))
            {
                Console.WriteLine("Card 1 to Card 2" + CheckEffectivnes(c1, c2));
                SendToBoth($"\n{c1.Name} Effects {c2.Name}");
                c1.EffectDamage(2);
                c2.EffectDamage(0.5);

            }
            else if (CheckEffectivnes(c2, c1))
            {
                Console.WriteLine("Card 2 to Card 1" + CheckEffectivnes(c2, c1));
                SendToBoth($"\n{c2.Name} Effects {c1.Name}");
                c1.EffectDamage(0.5);
                c2.EffectDamage(2);
            }
            else
            {
                c1.EffectDamage(1);
                c2.EffectDamage(1);
            }
        }

        /// <summary>
        /// Checks the effects that can occure
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns>bool => true || false</returns>
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


        /// <summary>
        /// Goes through every possibility for special Cases that can happen between two cards.
        /// </summary>
        /// <returns>The winner Card</returns>
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
                SendToBoth("\nCard 1 cant win against Card 2");
                return c2;
            }

            return null;
        }

        /// <summary>
        /// Checks the special Cases
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns>bool => true || false</returns>
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

        /// <summary>
        /// Calls the sender Methode for each palyer to send to both and write less code
        /// </summary>
        /// <param name="message"></param>
        private void SendToBoth(string message)
        {
            if (Players[0].TcpClient != null && Players[1].TcpClient != null)
            {
                SendMessages(Players[0].TcpClient, message);
                SendMessages(Players[1].TcpClient, message);
            }
        }

        /// <summary>
        /// Send Data to client
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="message"></param>
        private void SendMessages(TcpClient tcpClient, string message)
        {
            var networkStream = tcpClient.GetStream();
            var responseBytes = Encoding.UTF8.GetBytes(message);
            networkStream.Write(responseBytes, 0, responseBytes.Length);
        }
    }
}
