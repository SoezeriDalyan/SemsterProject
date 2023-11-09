using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class User
    {
        [JsonPropertyName("Username")]
        public string Username { get; private set; }
        [JsonPropertyName("Password")]
        public string Password { get; private set; }
        public int VirtualCoins { get; private set; }
        public User(string username, string password) 
        { 
            Username = username;
            Password = password;
            VirtualCoins = 100;
        }
    }
}
