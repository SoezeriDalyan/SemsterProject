namespace MonsterTradingCardGame
{
    internal class Battle
    {
        public DB DatabaseConnection { get; private set; }
        public List<Player> Players { get; private set; }

        public Battle(DB databaseConnection, List<Player> players)
        {
            DatabaseConnection = databaseConnection;
            Players = players;
        }


        public void BeginnTheBattle()
        {

        }


    }
}
