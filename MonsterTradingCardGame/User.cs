using System.Text.Json.Serialization;

namespace MonsterTradingCardGame
{
    public class User
    {
        [JsonPropertyName("Username")]
        public string Username { get; private set; }
        [Newtonsoft.Json.JsonIgnore]
        public string Password { get; private set; }
        public int VirtualCoins { get; private set; }
        [JsonPropertyName("Ímage")]
        public string Image { get; private set; }
        [JsonPropertyName("Bio")]
        public string Bio { get; private set; }

        [Newtonsoft.Json.JsonConstructor] //Ensures that this constructor is selected when de/serializing.
        public User(string username, string password, string? bio, string? image)
        {
            Username = username;
            Password = password;
            VirtualCoins = 20;
            Bio = bio;
            Image = image;
        }

        //This structure is important when importing from the database again in order to change the data.
        //The difference here is that the coins are read in and must be checked before you can buy packs.
        public User(string username, int virutalcoins, string bio, string image)
        {
            Username = username;
            VirtualCoins = virutalcoins;
            Bio = bio;
            Image = image;
        }
    }
}
