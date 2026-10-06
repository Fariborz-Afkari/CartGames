using UnityEngine;
namespace CardGames.Managers
{
    public static class GameDataManager
    {
        private const string PlayerNameKey = "Player_Name";
        private const string AvatarKey = "Player_Avatar";
        private const string CoinsKey = "Player_Coins";

        private const string GamePlayedPrefix = "Game_Played_";

        // --------------------------------------------------
        // Player Name
        // --------------------------------------------------

        public static void SetPlayerName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = "Player";

            PlayerPrefs.SetString(PlayerNameKey, playerName);
            PlayerPrefs.Save();
        }

        public static string GetPlayerName()
        {
            return PlayerPrefs.GetString(PlayerNameKey, "Player");
        }


        // --------------------------------------------------
        // Avatar
        // --------------------------------------------------

        public static void SetAvatar(int avatarIndex)
        {
            if (avatarIndex < 0)
                avatarIndex = 0;

            PlayerPrefs.SetInt(AvatarKey, avatarIndex);
            PlayerPrefs.Save();
        }

        public static int GetAvatar()
        {
            return PlayerPrefs.GetInt(AvatarKey, 0);
        }


        // --------------------------------------------------
        // Coins
        // --------------------------------------------------

        public static void SetCoins(int amount)
        {
            amount = Mathf.Max(0, amount);

            PlayerPrefs.SetInt(CoinsKey, amount);
            PlayerPrefs.Save();
        }

        public static int GetCoins()
        {
            return PlayerPrefs.GetInt(CoinsKey, 0);
        }

        public static void AddCoins(int amount)
        {
            if (amount <= 0)
                return;

            int currentCoins = GetCoins();

            SetCoins(currentCoins + amount);
        }

        public static bool SpendCoins(int amount)
        {
            if (amount < 0)
                return false;

            int currentCoins = GetCoins();

            if (currentCoins < amount)
                return false;

            SetCoins(currentCoins - amount);

            return true;
        }


        // --------------------------------------------------
        // Games Played
        // --------------------------------------------------

        public static int GetGamePlayedCount(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
                return 0;

            return PlayerPrefs.GetInt(
                GamePlayedPrefix + gameId,
                0
            );
        }

        public static void SetGamePlayedCount(
            string gameId,
            int count
        )
        {
            if (string.IsNullOrEmpty(gameId))
                return;

            count = Mathf.Max(0, count);

            PlayerPrefs.SetInt(
                GamePlayedPrefix + gameId,
                count
            );

            PlayerPrefs.Save();
        }

        public static void IncreaseGamePlayedCount(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
                return;

            int currentCount = GetGamePlayedCount(gameId);

            SetGamePlayedCount(
                gameId,
                currentCount + 1
            );
        }


        // --------------------------------------------------
        // Reset
        // --------------------------------------------------

        public static void DeleteAllData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}