using MonsterTradingCardGame;

[TestFixture]
public class DbClassTests
{
    string Username = "testUser";
    string PackId = "";

    [Order(1)]
    [Test]
    public void TestAddUser()
    {
        // Arrange
        var db = new DB();
        var user = new User(Username, "testPassword", "", "");

        // Act
        var result1 = db.AddUser(user);

        // Assert
        Assert.IsTrue(result1.Contains("Success"));

        // Act
        var result1False = db.AddUser(user);

        // Assert
        Assert.IsTrue(result1False.Contains("Error"));
    }

    [Order(2)]
    [Test]
    public void TestCreateSession()
    {
        // Arrange
        var db = new DB();
        var user = new User(Username, "testPassword", "", "");

        // Act
        var result = db.CreateSession(user);

        // Assert
        Assert.IsTrue(result.Contains("Success"));

        // Act
        var result2 = db.CreateSession(user);

        // Assert
        Assert.IsTrue(result2.Contains("Error: Session already exists"));
    }

    [Order(3)]
    [Test]
    public void TestCreatePackandCards()
    {
        // Arrange
        var db = new DB();
        var cards = new List<Card>
        {
            new Card("1", "Knight", 999),
            new Card("2", "Knight", 999),
            new Card("3", "Knight", 999),
            new Card("4", "Knight", 999),
            new Card("5", "Knight", 999)
        };

        // Act
        var result = db.CreatePackandCards(cards, "testUser-mtcgToken");
        PackId = result.Split(".\"")[1].Substring(0, result.Split(".\"")[1].Length - 2);

        // Assert
        Assert.IsTrue(result.Contains("Success"));
    }

    [Order(4)]
    [Test]
    public void TestBuyPack()
    {
        // Arrange
        var db = new DB();

        // Act
        var result = db.BuyPack("testUser-mtcgToken");

        // Assert
        Assert.IsTrue(result.Contains("Success"));
    }

    [Order(5)]
    [Test]
    public void GetCards()
    {
        // Arrange
        var db = new DB();

        // Act
        var result = db.GetAllCards("testUser-mtcgToken");

        // Assert
        Assert.IsTrue(result.Contains("Success"));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        var db = new DB();
        db.DeleteTestingData(Username, PackId);
    }
}
