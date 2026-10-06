using CardGames.Managers;
using UnityEngine;

namespace CardGames.DeadManDraws.Platform.Storage
{
    public sealed class LocalPlayerData
    {
        public string LoadPlayerName()
        {
            return PlayerData.PlayerName;
        }

        public void SavePlayerName(string playerName)
        {
            PlayerData.PlayerName = playerName;
        }

        public int LoadAvatar()
        {
            return PlayerData.AvatarIndex;
        }

        public void SaveAvatar(int avatarIndex)
        {
            PlayerData.AvatarIndex = avatarIndex;
        }

        public int LoadCoins()
        {
            return PlayerData.Coins;
        }

        public void SaveCoins(int coins)
        {
            PlayerData.Coins = Mathf.Max(0, coins);
        }

        public void Clear()
        {
            // اطلاعات مرکزی بازیکن را پاک نمی‌کنیم.
            // Coins / Name / Avatar متعلق به سیستم مرکزی هستند.
        }
    }
}