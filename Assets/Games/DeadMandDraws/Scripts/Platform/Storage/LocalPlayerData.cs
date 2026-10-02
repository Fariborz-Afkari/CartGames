using UnityEngine;

namespace CardGames.DeadManDraws.Platform.Storage
{
    public sealed class LocalPlayerData
    {
        private const string CoinsKey = "CardGames.DeadManDraws.Coins";

        public int LoadCoins()
        {
            return PlayerPrefs.GetInt(
                CoinsKey,
                10);
        }

        public void SaveCoins(int coins)
        {
            PlayerPrefs.SetInt(
                CoinsKey,
                coins);

            PlayerPrefs.Save();
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(
                CoinsKey);

            PlayerPrefs.Save();
        }
    }
}
