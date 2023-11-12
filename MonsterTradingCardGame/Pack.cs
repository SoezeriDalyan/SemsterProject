namespace MonsterTradingCardGame
{
    internal class Pack
    {
        public string PackID { get; private set; }

        public Pack()
        {
            PackID = Guid.NewGuid().ToString();
        }
    }
}
