using UnityEngine;

public static class PlayerData
{
    private const string PlayerNameKey = "Player_Name";
    private const string AvatarKey = "Player_Avatar";
    private const string CoinsKey = "Player_Coins";


    // =========================================
    // Player Name
    // =========================================

    public static string PlayerName
    {
        get
        {
            return PlayerPrefs.GetString(
                PlayerNameKey,
                "Player"
            );
        }

        set
        {
            string valueToSave = value;

            if (string.IsNullOrWhiteSpace(valueToSave))
                valueToSave = "Player";

            PlayerPrefs.SetString(
                PlayerNameKey,
                valueToSave
            );

            PlayerPrefs.Save();
        }
    }


    // =========================================
    // Avatar
    // =========================================

    public static int AvatarIndex
    {
        get
        {
            return PlayerPrefs.GetInt(
                AvatarKey,
                0
            );
        }

        set
        {
            int avatar = Mathf.Max(0, value);

            PlayerPrefs.SetInt(
                AvatarKey,
                avatar
            );

            PlayerPrefs.Save();
        }
    }


    // =========================================
    // Coins
    // =========================================

    public static int Coins
    {
        get
        {
            return PlayerPrefs.GetInt(
                CoinsKey,
                0
            );
        }

        set
        {
            int coins = Mathf.Max(0, value);

            PlayerPrefs.SetInt(
                CoinsKey,
                coins
            );

            PlayerPrefs.Save();
        }
    }


    // =========================================
    // Save Player
    // =========================================

    public static void SavePlayer(
        string playerName,
        int avatarIndex
    )
    {
        PlayerName = playerName;
        AvatarIndex = avatarIndex;
    }


    // =========================================
    // Coins
    // =========================================

    public static void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        Coins += amount;
    }


    public static bool SpendCoins(int amount)
    {
        if (amount < 0)
            return false;

        if (Coins < amount)
            return false;

        Coins -= amount;

        return true;
    }
}