using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class Session
    {
        public User User { get; private set; }
        public bool SessionActive { get; private set; }
        public string Token { get; private set; }

        public Session(User user, bool sessionActive)
        {
            User = user;
            SessionActive = sessionActive;
        }
    }
}
