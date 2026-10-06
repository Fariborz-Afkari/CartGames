using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
namespace CardGames.Managers
{
    public class SettingsController : MonoBehaviour
{
    // ==========================================
    // Player
    // ==========================================

    [Header("Player")]

    [SerializeField]
    private TMP_InputField txtName;

    [SerializeField]
    private Image avatarImage;


    // ==========================================
    // Avatar
    // ==========================================

    [Header("Avatar")]

    [SerializeField]
    private Button btnAvatar;

    [SerializeField]
    private GameObject avatarPanel;

    [SerializeField]
    private Button[] avatarButtons;

    [SerializeField]
    private Sprite[] avatarSprites;


    // ==========================================
    // Sound
    // ==========================================

    [Header("Sound")]

    [SerializeField]
    private Toggle toggleMusic;

    [SerializeField]
    private Toggle toggleFX;


    // ==========================================
    // Buttons
    // ==========================================

    [Header("Buttons")]

    [SerializeField]
    private Button btnSave;

    [SerializeField]
    private Button btnBack;


    // ==========================================
    // Scene
    // ==========================================

    [Header("Scene")]

    [SerializeField]
    private string mainSceneName = "CartGamesFirst";


    // ==========================================
    // Current Avatar
    // ==========================================

    private int currentAvatar;


    // ==========================================
    // Awake
    // ==========================================

    private void Awake()
    {
        FindSceneObjects();

        SetupButtons();
    }


    // ==========================================
    // Start
    // ==========================================

    private void Start()
    {
        LoadSettings();

        if (avatarPanel != null)
            avatarPanel.SetActive(false);
    }


    // ==========================================
    // Find Objects
    // ==========================================

    private void FindSceneObjects()
    {
        // --------------------------------------
        // Name
        // --------------------------------------

        if (txtName == null)
        {
            GameObject obj =
                GameObject.Find("txtName");

            if (obj != null)
                txtName =
                    obj.GetComponent<TMP_InputField>();
        }


        // --------------------------------------
        // Avatar Image
        // --------------------------------------

        if (avatarImage == null)
        {
            GameObject obj =
                GameObject.Find("AvatarImage");

            if (obj != null)
                avatarImage =
                    obj.GetComponent<Image>();
        }


        // --------------------------------------
        // Avatar Button
        // --------------------------------------

        if (btnAvatar == null)
        {
            GameObject obj =
                GameObject.Find("btnAvatar");

            if (obj != null)
                btnAvatar =
                    obj.GetComponent<Button>();
        }


        // --------------------------------------
        // Save
        // --------------------------------------

        if (btnSave == null)
        {
            GameObject obj =
                GameObject.Find("btnSave");

            if (obj != null)
                btnSave =
                    obj.GetComponent<Button>();
        }


        // --------------------------------------
        // Back
        // --------------------------------------

        if (btnBack == null)
        {
            GameObject obj =
                GameObject.Find("btnBack");

            if (obj != null)
                btnBack =
                    obj.GetComponent<Button>();
        }


        // --------------------------------------
        // Music
        // --------------------------------------

        if (toggleMusic == null)
        {
            GameObject obj =
                GameObject.Find("ToggleMusic");

            if (obj != null)
                toggleMusic =
                    obj.GetComponent<Toggle>();
        }


        // --------------------------------------
        // FX
        // --------------------------------------

        if (toggleFX == null)
        {
            GameObject obj =
                GameObject.Find("ToggleFX");

            if (obj != null)
                toggleFX =
                    obj.GetComponent<Toggle>();
        }
    }


    // ==========================================
    // Setup Buttons
    // ==========================================

    private void SetupButtons()
    {
        // Avatar
        if (btnAvatar != null)
        {
            btnAvatar.onClick.RemoveListener(
                OpenAvatarPanel
            );

            btnAvatar.onClick.AddListener(
                OpenAvatarPanel
            );
        }


        // Save
        if (btnSave != null)
        {
            btnSave.onClick.RemoveListener(
                SaveSettings
            );

            btnSave.onClick.AddListener(
                SaveSettings
            );
        }


        // Back
        if (btnBack != null)
        {
            btnBack.onClick.RemoveListener(
                BackToMainScene
            );

            btnBack.onClick.AddListener(
                BackToMainScene
            );
        }


        // Avatar buttons
        SetupAvatarButtons();
    }


    // ==========================================
    // Load Settings
    // ==========================================

    private void LoadSettings()
    {
        // Player Name
        if (txtName != null)
        {
            txtName.text = PlayerData.PlayerName;
        }


        // Avatar
        currentAvatar = PlayerData.AvatarIndex;

        UpdateAvatarImage();


        // Music
        if (toggleMusic != null)
        {
            toggleMusic.isOn = SettingsData.GetMusic();
        }


        // FX
        if (toggleFX != null)
        {
            toggleFX.isOn = SettingsData.GetFX();
        }
    }

    // ==========================================
    // Avatar Panel
    // ==========================================

    private void OpenAvatarPanel()
    {
        if (avatarPanel == null)
        {
            Debug.LogError(
                "Avatar Panel is not assigned."
            );

            return;
        }

        avatarPanel.SetActive(true);
    }


    // ==========================================
    // Close Avatar Panel
    // ==========================================

    public void CloseAvatarPanel()
    {
        if (avatarPanel != null)
            avatarPanel.SetActive(false);
    }


    // ==========================================
    // Setup Avatar Buttons
    // ==========================================

    private void SetupAvatarButtons()
    {
        if (avatarButtons == null)
            return;


        for (int i = 0;
             i < avatarButtons.Length;
             i++)
        {
            if (avatarButtons[i] == null)
                continue;


            int index = i;


            avatarButtons[i].onClick.RemoveAllListeners();


            avatarButtons[i].onClick.AddListener(
                () => SelectAvatar(index)
            );
        }
    }


    // ==========================================
    // Select Avatar
    // ==========================================

    private void SelectAvatar(int avatarIndex)
    {
        if (avatarSprites == null ||
            avatarSprites.Length == 0)
        {
            return;
        }


        if (avatarIndex < 0 ||
            avatarIndex >= avatarSprites.Length)
        {
            return;
        }


        currentAvatar = avatarIndex;


        // تغییر تصویر روی Settings
        UpdateAvatarImage();


        // ذخیره مرکزی
        PlayerData.AvatarIndex = currentAvatar;


        // بستن پنل
        if (avatarPanel != null)
        {
            avatarPanel.SetActive(false);
        }
    }

    // ==========================================
    // Update Avatar
    // ==========================================

    private void UpdateAvatarImage()
    {
        if (avatarImage == null)
            return;


        if (avatarSprites == null ||
            avatarSprites.Length == 0)
        {
            return;
        }


        if (currentAvatar < 0 ||
            currentAvatar >= avatarSprites.Length)
        {
            currentAvatar = 0;
        }


        avatarImage.sprite =
            avatarSprites[currentAvatar];
    }


    // ==========================================
    // Save Settings
    // ==========================================

    private void SaveSettings()
    {
        string playerName = "Player";

        if (txtName != null)
        {
            playerName = txtName.text;
        }


        bool music = true;

        if (toggleMusic != null)
        {
            music = toggleMusic.isOn;
        }


        bool fx = true;

        if (toggleFX != null)
        {
            fx = toggleFX.isOn;
        }


        // ذخیره اطلاعات اصلی بازیکن
        PlayerData.SavePlayer(
            playerName,
            currentAvatar
        );


        // ذخیره تنظیمات صدا
        SettingsData.SetMusic(music);
        SettingsData.SetFX(fx);


        Debug.Log(
            "Player settings saved."
        );
    }

    // ==========================================
    // Back
    // ==========================================

    private void BackToMainScene()
    {
        // قبل از خروج ذخیره شود
        SaveSettings();


        if (string.IsNullOrEmpty(mainSceneName))
        {
            Debug.LogError(
                "Main Scene Name is empty."
            );

            return;
        }


        SceneManager.LoadScene(
            mainSceneName
        );
    }
}
}