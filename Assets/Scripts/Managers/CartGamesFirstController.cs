using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
namespace CardGames.Managers
{
    public class CartGamesFirstController : MonoBehaviour
{
    // ==================================================
    // TOP
    // ==================================================

    [Header("Top")]
    [SerializeField] private Button btnTitle;

    [SerializeField] private string titleSceneName;


    // ==================================================
    // PLAYER
    // ==================================================

    [Header("Player")]
    [SerializeField] 
    private Image avatarImage;

    [SerializeField] 
    private TMP_Text txtPlayerName;

    [SerializeField]
    private Sprite[] avatarSprites;

    // ==================================================
    // COINS
    // ==================================================

    [Header("Coins")]
    [SerializeField] private Button btnBuyCoins;

    // Text داخل btnBuyCoins
    [SerializeField] private TMP_Text txtCoins;


    // ==================================================
    // SETTINGS
    // ==================================================

    [Header("Settings")]
    [SerializeField] private Button btnSettings;

    [SerializeField] private string settingsSceneName;


    // ==================================================
    // BUY COINS SCENE
    // ==================================================

    [Header("Buy Coins Scene")]
    [SerializeField] private string buyCoinsSceneName;


    // ==================================================
    // GAMES
    // ==================================================

    [Header("Game Buttons")]

    [SerializeField]
    private Button[] gameButtons = new Button[16];


    [Header("Game Played Texts")]

    [SerializeField]
    private TMP_Text[] gamePlayedTexts = new TMP_Text[16];


    [Header("Game IDs")]

    [SerializeField]
    private string[] gameIds = new string[16];


    [Header("Game Scenes")]

    [SerializeField]
    private string[] gameScenes = new string[16];


    // ==================================================
    // UNITY
    // ==================================================

    private void Awake()
    {
        AutoFindObjects();

        InitializeGameIds();

        SetupButtons();
    }


    private void Start()
    {
        LoadPlayerData();

        LoadCoins();

        LoadGamesPlayed();
    }


    // ==================================================
    // AUTO FIND
    // ==================================================

    private void AutoFindObjects()
    {
        // ----------------------------------------------
        // Top
        // ----------------------------------------------

        if (btnTitle == null)
        {
            GameObject obj = GameObject.Find("btnTitle");

            if (obj != null)
                btnTitle = obj.GetComponent<Button>();
        }


        // ----------------------------------------------
        // Player
        // ----------------------------------------------

        if (avatarImage == null)
        {
            GameObject obj = GameObject.Find("AvatarImage");

            if (obj != null)
                avatarImage = obj.GetComponent<Image>();
        }


        if (txtPlayerName == null)
        {
            GameObject obj = GameObject.Find("txtPlayerName");

            if (obj != null)
                txtPlayerName = obj.GetComponent<TMP_Text>();
        }


        // ----------------------------------------------
        // Settings
        // ----------------------------------------------

        if (btnSettings == null)
        {
            GameObject obj = GameObject.Find("btnSettings");

            if (obj != null)
                btnSettings = obj.GetComponent<Button>();
        }


        // ----------------------------------------------
        // Buy Coins
        // ----------------------------------------------

        if (btnBuyCoins == null)
        {
            GameObject obj = GameObject.Find("btnBuyCoins");

            if (obj != null)
                btnBuyCoins = obj.GetComponent<Button>();
        }


        if (txtCoins == null && btnBuyCoins != null)
        {
            txtCoins =
                btnBuyCoins.GetComponentInChildren<TMP_Text>();
        }


        // ----------------------------------------------
        // Game Buttons
        // ----------------------------------------------

        if (gameButtons == null ||
            gameButtons.Length != 16)
        {
            gameButtons = new Button[16];
        }


        if (gamePlayedTexts == null ||
            gamePlayedTexts.Length != 16)
        {
            gamePlayedTexts = new TMP_Text[16];
        }


        for (int i = 0; i < 16; i++)
        {
            string buttonName =
                $"btnGame_{(i + 1):00}";

            GameObject gameObject =
                GameObject.Find(buttonName);

            if (gameObject == null)
                continue;


            // Button
            if (gameButtons[i] == null)
            {
                gameButtons[i] =
                    gameObject.GetComponent<Button>();
            }


            // Text داخل Button
            if (gamePlayedTexts[i] == null)
            {
                gamePlayedTexts[i] =
                    gameObject.GetComponentInChildren<TMP_Text>();
            }
        }
    }


    // ==================================================
    // GAME IDS
    // ==================================================

    private void InitializeGameIds()
    {
        if (gameIds == null ||
            gameIds.Length != 16)
        {
            gameIds = new string[16];
        }


        for (int i = 0; i < 16; i++)
        {
            if (string.IsNullOrEmpty(gameIds[i]))
            {
                gameIds[i] =
                    $"Game_{(i + 1):00}";
            }
        }
    }


    // ==================================================
    // LOAD PLAYER DATA
    // ==================================================

    private void LoadPlayerData()
    {
        // Player Name
        if (txtPlayerName != null)
        {
            txtPlayerName.text =PlayerData.PlayerName;
        }


        // Avatar
        LoadAvatar();
    }


    // ==================================================
    // LOAD AVATAR
    // ==================================================

    private void LoadAvatar()
    {
        if (avatarImage == null)
            return;


        if (avatarSprites == null ||
            avatarSprites.Length == 0)
        {
            return;
        }


        int avatarIndex =
            PlayerData.AvatarIndex;


        if (avatarIndex < 0 ||
            avatarIndex >= avatarSprites.Length)
        {
            avatarIndex = 0;
        }


        avatarImage.sprite =
            avatarSprites[avatarIndex];
    }

    // ==================================================
    // LOAD COINS
    // ==================================================

    private void LoadCoins()
    {
        if (txtCoins == null)
            return;


        int coins =PlayerData.Coins;


        txtCoins.text =
            coins.ToString();
    }


    // ==================================================
    // LOAD GAMES PLAYED
    // ==================================================

    private void LoadGamesPlayed()
    {
        if (gameIds == null ||
            gamePlayedTexts == null)
        {
            return;
        }


        int count =
            Mathf.Min(
                gameIds.Length,
                gamePlayedTexts.Length
            );


        for (int i = 0; i < count; i++)
        {
            if (gamePlayedTexts[i] == null)
                continue;


            int playedCount =
                GameDataManager.GetGamePlayedCount(
                    gameIds[i]
                );


            gamePlayedTexts[i].text =
                playedCount.ToString();
        }
    }


    // ==================================================
    // SETUP BUTTONS
    // ==================================================

    private void SetupButtons()
    {
        // ----------------------------------------------
        // Title
        // ----------------------------------------------

        if (btnTitle != null)
        {
            btnTitle.onClick.RemoveListener(OpenTitleScene);

            btnTitle.onClick.AddListener(OpenTitleScene);
        }


        // ----------------------------------------------
        // Settings
        // ----------------------------------------------

        if (btnSettings != null)
        {
            btnSettings.onClick.RemoveListener(
                OpenSettingsScene
            );

            btnSettings.onClick.AddListener(
                OpenSettingsScene
            );
        }


        // ----------------------------------------------
        // Buy Coins
        // ----------------------------------------------

        if (btnBuyCoins != null)
        {
            btnBuyCoins.onClick.RemoveListener(
                OpenBuyCoinsScene
            );

            btnBuyCoins.onClick.AddListener(
                OpenBuyCoinsScene
            );
        }


        // ----------------------------------------------
        // Games
        // ----------------------------------------------

        if (gameButtons == null)
            return;


        for (int i = 0;
             i < gameButtons.Length;
             i++)
        {
            if (gameButtons[i] == null)
                continue;


            int index = i;


            gameButtons[i].onClick.RemoveAllListeners();


            gameButtons[i].onClick.AddListener(
                () => OpenGame(index)
            );
        }
    }


    // ==================================================
    // OPEN TITLE
    // ==================================================

    private void OpenTitleScene()
    {
        if (string.IsNullOrEmpty(titleSceneName))
        {
            Debug.LogError("Title Scene Name is empty.");

            return;
        }


        SceneManager.LoadScene(titleSceneName);
    }


    // ==================================================
    // OPEN SETTINGS
    // ==================================================

    private void OpenSettingsScene()
    {
        if (string.IsNullOrEmpty(settingsSceneName))
        {
            Debug.LogError(
                "Settings Scene Name is empty."
            );

            return;
        }


        SceneManager.LoadScene(
            settingsSceneName
        );
    }


    // ==================================================
    // OPEN BUY COINS
    // ==================================================

    private void OpenBuyCoinsScene()
    {
        if (string.IsNullOrEmpty(buyCoinsSceneName))
        {
            Debug.LogError(
                "Buy Coins Scene Name is empty."
            );

            return;
        }


        SceneManager.LoadScene(
            buyCoinsSceneName
        );
    }


    // ==================================================
    // OPEN GAME
    // ==================================================

    private void OpenGame(int index)
    {
        if (index < 0 ||
            index >= 16)
        {
            return;
        }


        if (gameButtons == null ||
            index >= gameButtons.Length)
        {
            return;
        }


        if (gameScenes == null ||
            index >= gameScenes.Length)
        {
            return;
        }


        string gameId =
            gameIds[index];


        string sceneName =
            gameScenes[index];


        if (string.IsNullOrEmpty(gameId))
        {
            Debug.LogError(
                $"Game ID for index {index} is empty."
            );

            return;
        }


        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError(
                $"Scene name for {gameId} is empty."
            );

            return;
        }


        // Increase play count
        GameDataManager.IncreaseGamePlayedCount(
            gameId
        );


        // Immediately update UI
        if (gamePlayedTexts != null &&
            index < gamePlayedTexts.Length &&
            gamePlayedTexts[index] != null)
        {
            int newCount =
                GameDataManager.GetGamePlayedCount(
                    gameId
                );


            gamePlayedTexts[index].text =
                newCount.ToString();
        }


        // Open game Scene
        SceneManager.LoadScene(
            sceneName
        );
    }


    // ==================================================
    // PUBLIC METHODS
    // ==================================================

    public void RefreshUI()
    {
        LoadPlayerData();

        LoadCoins();

        LoadGamesPlayed();
    }


    public void AddCoins(int amount)
    {
        GameDataManager.AddCoins(amount);

        LoadCoins();
    }


    public bool SpendCoins(int amount)
    {
        bool result =
            GameDataManager.SpendCoins(amount);

        if (result)
            LoadCoins();

        return result;
    }
}
}