using System.Text.Json.Serialization;

namespace MonsterTradingCardGame
{
    internal class User
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

        //Name soll username erstezten ????? Todo, Fragen
        [Newtonsoft.Json.JsonConstructor]
        public User(string username, string password, string? bio, string? image)
        {
            Username = username;
            Password = password;
            VirtualCoins = 20;
            Bio = bio;
            Image = image;
        }
        public User(string username, int virutalcoins, string bio, string image)
        {
            Username = username;
            VirtualCoins = virutalcoins;
            Bio = bio;
            Image = image;
        }
    }
}
