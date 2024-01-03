namespace MonsterTradingCardGame.Tests
{
    [TestFixture]
    public class TradingTests
    {
        [Test]
        public void Constructor_WithTradeRequest_ParsesPropertiesCorrectly()
        {
            // Arrange
            var trading = new Trading("ID123", "CardA", "Monster", 10.5);

            // Assert
            Assert.AreEqual("ID123", trading.ID);
            Assert.AreEqual("CardA", trading.CardToTrade);
            Assert.AreEqual(Type.Monster, trading.Type);
            Assert.AreEqual(10.5, trading.MinimumDamage);
            Assert.IsNull(trading.Trader);
        }

        [Test]
        public void Constructor_WithTradeOffer_ParsesPropertiesCorrectly()
        {
            // Arrange
            var trading = new Trading("ID456", "CardB", "Spell", 5.0, "Player1");

            // Assert
            Assert.AreEqual("ID456", trading.ID);
            Assert.AreEqual("CardB", trading.CardToTrade);
            Assert.AreEqual(Type.Spell, trading.Type);
            Assert.AreEqual(5.0, trading.MinimumDamage);
            Assert.AreEqual("Player1", trading.Trader);
        }

        [Test]
        public void Constructor_WithInvalidType_ParsesPropertiesCorrectly()
        {
            // Arrange
            var trading = new Trading("ID789", "CardC", "InvalidType", 8.0);

            // Assert
            Assert.AreEqual("ID789", trading.ID);
            Assert.AreEqual("CardC", trading.CardToTrade);
            Assert.AreEqual(Type.Spell, trading.Type); // Default to Spell for invalid type
            Assert.AreEqual(8.0, trading.MinimumDamage);
            Assert.IsNull(trading.Trader);
        }

        // Add more test cases for other scenarios as needed.
    }
}
