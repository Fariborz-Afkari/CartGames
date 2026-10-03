using System;

namespace CardGames.DeadManDraws.Platform.Iap
{
    public interface IIapService
    {
        void PurchaseCoins(
            int amount,
            Action<bool> completed);
    }
}
