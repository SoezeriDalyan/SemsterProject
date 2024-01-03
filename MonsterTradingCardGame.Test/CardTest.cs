using MonsterTradingCardGame;

namespace MonsterTradingCardGameTests
{
    [TestFixture]
    public class CardTests
    {
        [Test]
        public void CardConstructor_SetsPropertiesCorrectlyForMonster()
        {
            // Arrange
            string id = "1";
            string name = "MonsterCard";
            double damage = 10.5;

            // Act
            Card card = new Card(id, name, damage);

            // Assert
            Assert.AreEqual(id, card.ID);
            Assert.AreEqual(name, card.Name);
            Assert.AreEqual(damage, card.Damage);
            Assert.AreEqual(MonsterTradingCardGame.Type.Monster, card.Type);
            Assert.AreEqual(Attribut.Normal, card.Attribute);
        }

        [Test]
        public void CardConstructor_SetsPropertiesCorrectlyForSpell()
        {
            // Arrange
            string id = "2";
            string name = "SpellCard";
            double damage = 5.0;

            // Act
            Card card = new Card(id, name, damage);

            // Assert
            Assert.AreEqual(id, card.ID);
            Assert.AreEqual(name, card.Name);
            Assert.AreEqual(damage, card.Damage);
            Assert.AreEqual(MonsterTradingCardGame.Type.Spell, card.Type);
            Assert.AreEqual(Attribut.Normal, card.Attribute);
        }

        [Test]
        public void EffectDamage_UpdatesTempDamageCorrectly()
        {
            // Arrange
            Card card = new Card("3", "MonsterCard", 15.0);
            double factor = 0.8;

            // Act
            card.EffectDamage(factor);

            // Assert
            Assert.AreEqual(12.0, card.TempDamage, 0.001);
        }

        [Test]
        public void ToString_ReturnsExpectedString()
        {
            // Arrange
            Card card = new Card("4", "FireMonster", 20.0);

            // Act
            string result = card.ToString();

            // Assert
            string expected = "ID(4) Name(FireMonster) Damage(20) Attribut(Fire) Type(Monster)";
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void BattleString_ReturnsExpectedString()
        {
            // Arrange
            Card card = new Card("5", "WaterSpell", 8.0);
            card.EffectDamage(0.5);

            // Act
            string result = card.BattleString();

            // Assert
            string expected = "Name(WaterSpell) Damage(8) TempDamage(4)";
            Assert.AreEqual(expected, result);
        }
    }
}


