using UnityEngine;

public static class SettingsData
{
    private const string PlayerNameKey = "Settings_PlayerName";
    private const string AvatarKey = "Settings_Avatar";
    private const string MusicKey = "Settings_Music";
    private const string FXKey = "Settings_FX";


    // ==========================================
    // Player Name
    // ==========================================

    public static void SetPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            name = "Player";

        PlayerPrefs.SetString(PlayerNameKey, name);
        PlayerPrefs.Save();
    }

    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(
            PlayerNameKey,
            "Player"
        );
    }


    // ==========================================
    // Avatar
    // ==========================================

    public static void SetAvatar(int avatarIndex)
    {
        if (avatarIndex < 0)
            avatarIndex = 0;

        PlayerPrefs.SetInt(
            AvatarKey,
            avatarIndex
        );

        PlayerPrefs.Save();
    }

    public static int GetAvatar()
    {
        return PlayerPrefs.GetInt(
            AvatarKey,
            0
        );
    }


    // ==========================================
    // Music
    // ==========================================

    public static void SetMusic(bool enabled)
    {
        PlayerPrefs.SetInt(
            MusicKey,
            enabled ? 1 : 0
        );

        PlayerPrefs.Save();
    }

    public static bool GetMusic()
    {
        return PlayerPrefs.GetInt(
            MusicKey,
            1
        ) == 1;
    }


    // ==========================================
    // FX
    // ==========================================

    public static void SetFX(bool enabled)
    {
        PlayerPrefs.SetInt(
            FXKey,
            enabled ? 1 : 0
        );

        PlayerPrefs.Save();
    }

    public static bool GetFX()
    {
        return PlayerPrefs.GetInt(
            FXKey,
            1
        ) == 1;
    }


    // ==========================================
    // Save All
    // ==========================================

    public static void Save(
        string playerName,
        int avatarIndex,
        bool music,
        bool fx
    )
    {
        SetPlayerName(playerName);
        SetAvatar(avatarIndex);
        SetMusic(music);
        SetFX(fx);
    }
}