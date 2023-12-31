using System.Text.Json.Serialization;

namespace MonsterTradingCardGame
{
    internal class User
    {
        [JsonPropertyName("Username")]
        public string Username { get; private set; }
        [JsonPropertyName("Password")]
        public string Password { get; private set; }
        public int VirtualCoins { get; private set; }
        [JsonPropertyName("Ímage")]
        public string Image { get; private set; }
        [JsonPropertyName("Bio")]
        public string Bio { get; private set; }
        [JsonPropertyName("Name")]
        public string Name { get; private set; }

        //Name soll username erstezten ????? Todo, Fragen
        public User(string username, string password, string bio, string image, string name)
        {
            Username = username;
            Password = password;
            VirtualCoins = 20;
            Bio = bio;
            Image = image;
            Name = name;
        }
    }
}
