namespace CardGames.DeadManDraws.Platform.Economy
{
    public interface IEconomyService
    {
        int Coins { get; }
        bool TryStartMatch(out string error);
        void GrantCoins(int amount);
        void Save();
    }
}
