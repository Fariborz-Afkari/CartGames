using CardGames.DeadManDraws.Platform.Storage;

namespace CardGames.DeadManDraws.Platform.Economy
{
    public sealed class EconomyService : IEconomyService
    {
        private const int MatchCost = 1;

        private readonly Wallet _wallet;
        private readonly LocalPlayerData _storage;

        public int Coins
        {
            get { return _wallet.Coins; }
        }

        public EconomyService(
            LocalPlayerData storage)
        {
            _storage = storage;
            _wallet = new Wallet();

            _wallet.Initialize(
                _storage.LoadCoins());
        }

        public bool TryStartMatch(
            out string error)
        {
            if (!_wallet.TrySpend(MatchCost))
            {
                error ="Not enough coins. Use the Mock IAP button to buy 10 coins.";

                return false;
            }

            Save();

            error = string.Empty;

            return true;
        }

        public void GrantCoins(
            int amount)
        {
            _wallet.AddCoins(amount);
            Save();
        }

        public void Save()
        {
            _storage.SaveCoins(
                _wallet.Coins);
        }
    }
}
