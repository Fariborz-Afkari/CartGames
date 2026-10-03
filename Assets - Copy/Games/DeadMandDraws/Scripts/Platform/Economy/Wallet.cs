namespace CardGames.DeadManDraws.Platform.Economy
{
    public sealed class Wallet
    {
        public int Coins { get; private set; }

        public void Initialize(int coins)
        {
            Coins = coins < 0
                ? 0
                : coins;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0)
                return false;

            if (Coins < amount)
                return false;

            Coins -= amount;
            return true;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
                return;

            Coins += amount;
        }
    }
}
