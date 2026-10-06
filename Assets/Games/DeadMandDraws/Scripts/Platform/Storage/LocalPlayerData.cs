using UnityEngine;
using CardGames.Managers;

    namespace CardGames.DeadManDraws.Platform.Storage
{
    public sealed class LocalPlayerData
    {
        // ============================================================
        // PLAYER NAME
        // ============================================================

        public string LoadPlayerName()
        {
            return PlayerData.PlayerName;
        }

        public void SavePlayerName(string playerName)
        {
            PlayerData.PlayerName = playerName;
        }

        // ============================================================
        // AVATAR
        // ============================================================

        public int LoadAvatar()
        {
            return PlayerData.AvatarIndex;
        }

        public void SaveAvatar(int avatarIndex)
        {
            PlayerData.AvatarIndex = avatarIndex;
        }

        // ============================================================
        // COINS
        // ============================================================

        public int LoadCoins()
        {
            return PlayerData.Coins;
        }

        public void SaveCoins(int coins)
        {
            PlayerData.Coins = Mathf.Max(0, coins);
        }

        // ============================================================
        // CLEAR
        // ============================================================

        public void Clear()
        {
            /*
             * اطلاعات اصلی بازیکن متعلق به سیستم مرکزی است.
             *
             * عمداً PlayerPrefs.DeleteKey انجام نمی‌دهیم
             * تا اطلاعات بازیکن در سایر بازی‌ها از بین نرود.
             */
        }
    }
}