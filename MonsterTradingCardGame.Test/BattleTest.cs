namespace MonsterTradingCardGame.Tests
{
    [TestFixture]
    public class BattleTests
    {
        [Test]
        public void BeginnTheBattle_Player1Wins()
        {
            // Arrange
            var player1 = new Player(null, "Player1", new List<Card>());
            var player2 = new Player(null, "Player2", new List<Card>());
            var players = new List<Player> { player1, player2 };
            var databaseConnection = new DB(); // Assuming DB class is available
            var battle = new Battle(databaseConnection, players);

            // Manually setting up the decks for testing (each deck with 4 cards)
            player1.Deck.AddRange(new[]
            {
                new Card("ID1", "Goblin", 6),
                new Card("ID2", "RegularSpell", 5),
                new Card("ID3", "FireSpell", 3),
                new Card("ID4", "Dragon", 4),
            });

            player2.Deck.AddRange(new[]
            {
                new Card("ID5", "Knight", 1),
                new Card("ID6", "RegularSpell", 2),
                new Card("ID7", "FireSpell", 1),
                new Card("ID8", "WaterGoblin", 2),
            });

            // Act
            string result = battle.BeginnTheBattle();

            // Assert
            Assert.AreEqual("Winner = Player1", result);
        }

        //inconsistent
        [Test]
        public void BeginnTheBattle_Player2Wins()
        {
            // Arrange
            var player1 = new Player(null, "Player1", new List<Card>());
            var player2 = new Player(null, "Player2", new List<Card>());
            var players = new List<Player> { player1, player2 };
            var databaseConnection = new DB(); // Assuming DB class is available
            var battle = new Battle(databaseConnection, players);

            // Manually setting up the decks for testing (each deck with 4 cards)
            player1.Deck.AddRange(new[]
            {
                new Card("ID1", "Knight", 1),
                new Card("ID2", "RegularSpell", 2),
                new Card("ID3", "FireSpell", 1),
                new Card("ID4", "WaterGoblin", 2),
            });

            player2.Deck.AddRange(new[]
            {
                new Card("ID5", "Goblin", 6),
                new Card("ID6", "RegularSpell", 5),
                new Card("ID7", "FireSpell", 3),
                new Card("ID8", "Dragon", 4),
            });

            // Act
            string result = battle.BeginnTheBattle();

            // Assert
            Assert.AreEqual("Winner = Player2", result);
        }

        //inconsistent
        [Test]
        public void BeginnTheBattle_Draw()
        {
            // Arrange
            var player1 = new Player(null, "Player1", new List<Card>());
            var player2 = new Player(null, "Player2", new List<Card>());
            var players = new List<Player> { player1, player2 };
            var databaseConnection = new DB(); // Assuming DB class is available
            var battle = new Battle(databaseConnection, players);

            // Manually setting up the decks for testing (each deck with 4 cards)
            player1.Deck.AddRange(new[]
            {
                new Card("ID1", "Dragon", 3),
                new Card("ID2", "Dragon", 3),
                new Card("ID3", "Dragon", 3),
                new Card("ID4", "Dragon", 3),
            });

            player2.Deck.AddRange(new[]
            {
                new Card("ID5", "Dragon", 3),
                new Card("ID6", "Dragon", 3),
                new Card("ID7", "Dragon", 3),
                new Card("ID8", "Dragon", 3),
            });

            // Act
            string result = battle.BeginnTheBattle();

            // Assert
            Assert.AreEqual("Its a draw", result);
        }
    }
}
