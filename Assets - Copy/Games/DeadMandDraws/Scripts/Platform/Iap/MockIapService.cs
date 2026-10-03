using System;

namespace CardGames.DeadManDraws.Platform.Iap
{
    public sealed class MockIapService : IIapService
    {
        public void PurchaseCoins(
            int amount,
            Action<bool> completed)
        {
            if (amount <= 0)
            {
                completed?.Invoke(false);
                return;
            }

            // Mock purchase:
            // Always succeeds for now.
            completed?.Invoke(true);
        }
    }
}
