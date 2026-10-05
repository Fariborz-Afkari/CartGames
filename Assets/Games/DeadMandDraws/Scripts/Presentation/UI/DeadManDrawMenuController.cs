using CardGames.DeadManDraws.Core.Players;
using CardGames.DeadManDraws.Presentation.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGames.DeadManDraws.Presentation
{
    public class DeadManDrawMenuController : MonoBehaviour
    {
        // ============================================================
        // MENU PANELS
        // ============================================================

        [Header("Main Panels")]

        [SerializeField]
        private GameObject _startPanel;

        [SerializeField]
        private GameObject _helpPanel;

        [SerializeField]
        private GameObject _groupGamePanel;

        [SerializeField]
        private GameObject _newGamePanel;

        [SerializeField]
        private GameObject _selectTraitPanel;

        [SerializeField]
        private GameObject _traitHelpPanel;


        // ============================================================
        // GAME UI
        // ============================================================

        [Header("Game UI")]

        [SerializeField]
        private GameUi _gameUi;


        // ============================================================
        // START PANEL BUTTONS
        // ============================================================

        [Header("Start Panel Buttons")]

        [SerializeField]
        private Button _startHelpButton;

        [SerializeField]
        private Button _groupGameButton;

        [SerializeField]
        private Button _continueButton;

        [SerializeField]
        private Button _newGameButton;

        [SerializeField]
        private Button _startBackButton;


        // ============================================================
        // HELP PANEL
        // ============================================================

        [Header("Help Panel")]

        [SerializeField]
        private Button _helpBackButton;


        // ============================================================
        // GROUP GAME PANEL
        // ============================================================

        [Header("Group Game Panel")]

        [SerializeField]
        private Button _groupGameBackButton;


        // ============================================================
        // NEW GAME PANEL
        // ============================================================

        [Header("New Game Panel")]

        [SerializeField]
        private Button _newGameBackButton;

        [SerializeField]
        private Slider _playerNumSlider;

        [SerializeField]
        private Slider _hardnessSlider;

        [SerializeField]
        private TMP_Text _txtNumPlayer;

        [SerializeField]
        private Button _newGameStartButton;


        // ============================================================
        // TRAIT SELECTION PANEL
        // ============================================================

        [Header("Trait Selection")]

        [SerializeField]
        private Button _trait1Button;

        [SerializeField]
        private Button _trait2Button;

        [SerializeField]
        private Button _trait1HelpButton;

        [SerializeField]
        private Button _trait2HelpButton;

        [SerializeField]
        private Image _trait1Image;

        [SerializeField]
        private Image _trait2Image;


        // ============================================================
        // TRAIT HELP PANEL
        // ============================================================

        [Header("Trait Help")]

        [SerializeField]
        private TMP_Text _traitHelpText;

        [SerializeField]
        private Button _traitHelpBackButton;


        // ============================================================
        // TRAIT VISUAL DATA
        // ============================================================

        [Serializable]
        private class TraitVisualData
        {
            public PlayerTrait trait;

            public Sprite image;

            [TextArea(3, 8)]
            public string description;
        }

        [Header("Trait Data")]

        [SerializeField]
        private List<TraitVisualData> _traitData =
            new List<TraitVisualData>();


        // ============================================================
        // PLAYER SETTINGS
        // ============================================================

        private const int MinPlayers = 2;
        private const int MaxPlayers = 5;

        private const int MinDifficulty = 0;
        private const int MaxDifficulty = 2;


        // ============================================================
        // RUNTIME STATE
        // ============================================================

        private PlayerTrait _trait1 = PlayerTrait.None;
        private PlayerTrait _trait2 = PlayerTrait.None;

        private bool _listenersRegistered;


        // ============================================================
        // PLAYER PREFS
        // ============================================================

        private const string SavedGameKey =
            "DeadManDraw.HasSavedGame";

        private const string SavedPlayerCountKey =
            "DeadManDraw.PlayerCount";

        private const string SavedDifficultyKey =
            "DeadManDraw.Difficulty";


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            ResolveReferences();

            ConfigureSliders();

            RegisterListeners();

            ShowStartPanel();
        }


        private void OnDestroy()
        {
            UnregisterListeners();
        }


        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            if (_gameUi == null)
            {
                _gameUi = FindFirstObjectByType<GameUi>();
            }

            if (_startPanel == null)
            {
                _startPanel = FindPanel("StartPanel");
            }

            if (_helpPanel == null)
            {
                _helpPanel = FindPanel("HelpPanel");
            }

            if (_groupGamePanel == null)
            {
                _groupGamePanel = FindPanel("GroupGamePanel");
            }

            if (_newGamePanel == null)
            {
                _newGamePanel = FindPanel("NewGamePanel");
            }

            if (_selectTraitPanel == null)
            {
                _selectTraitPanel = FindPanel("SelectTraitPanel");
            }

            if (_traitHelpPanel == null)
            {
                _traitHelpPanel = FindPanel("TraitHelpPanel");
            }


            // --------------------------------------------------------
            // StartPanel
            // --------------------------------------------------------

            if (_startPanel != null)
            {
                _startHelpButton =
                    FindButton(_startPanel, "Help_Button");

                _groupGameButton =
                    FindButton(_startPanel, "GroupGame_Button");

                _continueButton =
                    FindButton(_startPanel, "Continue_Button");

                _newGameButton =
                    FindButton(_startPanel, "NewGame_Button");

                _startBackButton =
                    FindButton(_startPanel, "Back_Button");
            }


            // --------------------------------------------------------
            // HelpPanel
            // --------------------------------------------------------

            if (_helpPanel != null)
            {
                _helpBackButton =
                    FindButton(_helpPanel, "Back_Button");
            }


            // --------------------------------------------------------
            // GroupGamePanel
            // --------------------------------------------------------

            if (_groupGamePanel != null)
            {
                _groupGameBackButton =
                    FindButton(
                        _groupGamePanel,
                        "Back_Button");
            }


            // --------------------------------------------------------
            // NewGamePanel
            // --------------------------------------------------------

            if (_newGamePanel != null)
            {
                _newGameBackButton =
                    FindButton(
                        _newGamePanel,
                        "Back_Button");

                _playerNumSlider =
                    FindComponent<Slider>(
                        _newGamePanel,
                        "PlayerNumSlider");

                _hardnessSlider =
                    FindComponent<Slider>(
                        _newGamePanel,
                        "HardnessSlider");

                _txtNumPlayer =
                    FindComponent<TMP_Text>(
                        _newGamePanel,
                        "txtNumPlayer");

                _newGameStartButton =
                    FindButton(
                        _newGamePanel,
                        "Start_Button");
            }


            // --------------------------------------------------------
            // SelectTraitPanel
            // --------------------------------------------------------

            if (_selectTraitPanel != null)
            {
                _trait1Button =
                    FindButton(
                        _selectTraitPanel,
                        "Trait1_Button");

                _trait2Button =
                    FindButton(
                        _selectTraitPanel,
                        "Trait2_Button");

                _trait1HelpButton =
                    FindButton(
                        _selectTraitPanel,
                        "Trait1Help_Button");

                _trait2HelpButton =
                    FindButton(
                        _selectTraitPanel,
                        "Trait2Help_Button");

                _trait1Image =
                    FindComponent<Image>(
                        _selectTraitPanel,
                        "Trait1_Image");

                _trait2Image =
                    FindComponent<Image>(
                        _selectTraitPanel,
                        "Trait2_Image");
            }


            // --------------------------------------------------------
            // TraitHelpPanel
            // --------------------------------------------------------

            if (_traitHelpPanel != null)
            {
                _traitHelpText =
                    FindComponent<TMP_Text>(
                        _traitHelpPanel,
                        "TraitHelpText");

                _traitHelpBackButton =
                    FindButton(
                        _traitHelpPanel,
                        "Back_Button");
            }
        }


        // ============================================================
        // FIND HELPERS
        // ============================================================

        private GameObject FindPanel(string panelName)
        {
            Transform child =
                transform.Find(panelName);

            if (child != null)
                return child.gameObject;

            GameObject found =
                GameObject.Find(panelName);

            return found;
        }


        private Button FindButton(
            GameObject parent,
            string buttonName)
        {
            if (parent == null)
                return null;

            Transform target =
                FindChildRecursive(
                    parent.transform,
                    buttonName);

            if (target == null)
            {
                Debug.LogWarning(
                    $"[DeadManDrawMenuController] " +
                    $"Could not find Button '{buttonName}' " +
                    $"inside '{parent.name}'.");

                return null;
            }

            Button button =
                target.GetComponent<Button>();

            if (button == null)
            {
                Debug.LogWarning(
                    $"[DeadManDrawMenuController] " +
                    $"Object '{buttonName}' does not contain Button.");
            }

            return button;
        }


        private T FindComponent<T>(
            GameObject parent,
            string objectName)
            where T : Component
        {
            if (parent == null)
                return null;

            Transform target =
                FindChildRecursive(
                    parent.transform,
                    objectName);

            if (target == null)
            {
                Debug.LogWarning(
                    $"[DeadManDrawMenuController] " +
                    $"Could not find '{objectName}' " +
                    $"inside '{parent.name}'.");

                return null;
            }

            T component =
                target.GetComponent<T>();

            if (component == null)
            {
                Debug.LogWarning(
                    $"[DeadManDrawMenuController] " +
                    $"Object '{objectName}' does not contain " +
                    $"{typeof(T).Name}.");
            }

            return component;
        }


        private Transform FindChildRecursive(
            Transform parent,
            string childName)
        {
            if (parent.name == childName)
                return parent;

            for (int i = 0;
                 i < parent.childCount;
                 i++)
            {
                Transform child =
                    parent.GetChild(i);

                Transform result =
                    FindChildRecursive(
                        child,
                        childName);

                if (result != null)
                    return result;
            }

            return null;
        }


        // ============================================================
        // LISTENERS
        // ============================================================

        private void RegisterListeners()
        {
            if (_listenersRegistered)
                return;

            _listenersRegistered = true;


            // Start Panel
            if (_startHelpButton != null)
                _startHelpButton.onClick.AddListener(
                    OpenHelpPanel);

            if (_groupGameButton != null)
                _groupGameButton.onClick.AddListener(
                    OpenGroupGamePanel);

            if (_continueButton != null)
                _continueButton.onClick.AddListener(
                    ContinueGame);

            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(
                    OpenNewGamePanel);

            if (_startBackButton != null)
                _startBackButton.onClick.AddListener(
                    BackToPreviousPage);


            // Help
            if (_helpBackButton != null)
                _helpBackButton.onClick.AddListener(
                    ShowStartPanel);


            // Group Game
            if (_groupGameBackButton != null)
                _groupGameBackButton.onClick.AddListener(
                    ShowStartPanel);


            // New Game
            if (_newGameBackButton != null)
                _newGameBackButton.onClick.AddListener(
                    ShowStartPanel);

            if (_newGameStartButton != null)
                _newGameStartButton.onClick.AddListener(
                    StartNewGame);


            // Sliders
            if (_playerNumSlider != null)
            {
                _playerNumSlider.onValueChanged.AddListener(
                    OnPlayerCountChanged);
            }

            if (_hardnessSlider != null)
            {
                _hardnessSlider.onValueChanged.AddListener(
                    OnDifficultyChanged);
            }


            // Trait Selection
            if (_trait1Button != null)
                _trait1Button.onClick.AddListener(
                    SelectTrait1);

            if (_trait2Button != null)
                _trait2Button.onClick.AddListener(
                    SelectTrait2);

            if (_trait1HelpButton != null)
                _trait1HelpButton.onClick.AddListener(
                    ShowTrait1Help);

            if (_trait2HelpButton != null)
                _trait2HelpButton.onClick.AddListener(
                    ShowTrait2Help);


            // Trait Help
            if (_traitHelpBackButton != null)
                _traitHelpBackButton.onClick.AddListener(
                    CloseTraitHelp);
        }


        private void UnregisterListeners()
        {
            if (!_listenersRegistered)
                return;

            _listenersRegistered = false;


            if (_startHelpButton != null)
                _startHelpButton.onClick.RemoveListener(
                    OpenHelpPanel);

            if (_groupGameButton != null)
                _groupGameButton.onClick.RemoveListener(
                    OpenGroupGamePanel);

            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(
                    ContinueGame);

            if (_newGameButton != null)
                _newGameButton.onClick.RemoveListener(
                    OpenNewGamePanel);

            if (_startBackButton != null)
                _startBackButton.onClick.RemoveListener(
                    BackToPreviousPage);


            if (_helpBackButton != null)
                _helpBackButton.onClick.RemoveListener(
                    ShowStartPanel);

            if (_groupGameBackButton != null)
                _groupGameBackButton.onClick.RemoveListener(
                    ShowStartPanel);

            if (_newGameBackButton != null)
                _newGameBackButton.onClick.RemoveListener(
                    ShowStartPanel);

            if (_newGameStartButton != null)
                _newGameStartButton.onClick.RemoveListener(
                    StartNewGame);


            if (_playerNumSlider != null)
            {
                _playerNumSlider.onValueChanged.RemoveListener(
                    OnPlayerCountChanged);
            }

            if (_hardnessSlider != null)
            {
                _hardnessSlider.onValueChanged.RemoveListener(
                    OnDifficultyChanged);
            }


            if (_trait1Button != null)
                _trait1Button.onClick.RemoveListener(
                    SelectTrait1);

            if (_trait2Button != null)
                _trait2Button.onClick.RemoveListener(
                    SelectTrait2);

            if (_trait1HelpButton != null)
                _trait1HelpButton.onClick.RemoveListener(
                    ShowTrait1Help);

            if (_trait2HelpButton != null)
                _trait2HelpButton.onClick.RemoveListener(
                    ShowTrait2Help);

            if (_traitHelpBackButton != null)
                _traitHelpBackButton.onClick.RemoveListener(
                    CloseTraitHelp);
        }


        // ============================================================
        // SLIDERS
        // ============================================================

        private void ConfigureSliders()
        {
            if (_playerNumSlider != null)
            {
                _playerNumSlider.wholeNumbers = true;
                _playerNumSlider.minValue = MinPlayers;
                _playerNumSlider.maxValue = MaxPlayers;

                int current =
                    Mathf.Clamp(
                        Mathf.RoundToInt(
                            _playerNumSlider.value),
                        MinPlayers,
                        MaxPlayers);

                _playerNumSlider.value = current;

                UpdatePlayerCountText(current);
            }


            if (_hardnessSlider != null)
            {
                _hardnessSlider.wholeNumbers = true;
                _hardnessSlider.minValue = MinDifficulty;
                _hardnessSlider.maxValue = MaxDifficulty;

                _hardnessSlider.value =
                    Mathf.Clamp(
                        Mathf.RoundToInt(
                            _hardnessSlider.value),
                        MinDifficulty,
                        MaxDifficulty);
            }
        }


        private void OnPlayerCountChanged(float value)
        {
            int playerCount =
                Mathf.Clamp(
                    Mathf.RoundToInt(value),
                    MinPlayers,
                    MaxPlayers);

            UpdatePlayerCountText(playerCount);
        }


        private void UpdatePlayerCountText(
            int playerCount)
        {
            if (_txtNumPlayer == null)
                return;

            _txtNumPlayer.text =
                playerCount.ToString();
        }


        private void OnDifficultyChanged(float value)
        {
            int difficulty =
                Mathf.Clamp(
                    Mathf.RoundToInt(value),
                    MinDifficulty,
                    MaxDifficulty);

            Debug.Log(
                $"[DeadManDrawMenuController] " +
                $"AI difficulty = {difficulty}");
        }


        // ============================================================
        // START PANEL
        // ============================================================

        private void ShowStartPanel()
        {
            HideAllMenuPanels();

            if (_startPanel != null)
                _startPanel.SetActive(true);

            UpdateContinueButton();

            Debug.Log(
                "[DeadManDrawMenuController] " +
                "StartPanel shown.");
        }


        private void OpenHelpPanel()
        {
            HideAllMenuPanels();

            if (_helpPanel != null)
                _helpPanel.SetActive(true);

            Debug.Log(
                "[DeadManDrawMenuController] " +
                "HelpPanel opened.");
        }


        private void OpenGroupGamePanel()
        {
            HideAllMenuPanels();

            if (_groupGamePanel != null)
                _groupGamePanel.SetActive(true);

            Debug.Log(
                "[DeadManDrawMenuController] " +
                "GroupGamePanel opened.");
        }


        private void OpenNewGamePanel()
        {
            HideAllMenuPanels();

            if (_newGamePanel != null)
                _newGamePanel.SetActive(true);

            ConfigureSliders();

            Debug.Log(
                "[DeadManDrawMenuController] " +
                "NewGamePanel opened.");
        }


        private void BackToPreviousPage()
        {
            ShowStartPanel();
        }


        // ============================================================
        // NEW GAME
        // ============================================================

        private void StartNewGame()
        {




            Debug.Log(
                "[DeadManDrawMenuController] " +
                "StartNewGame() CLICKED.");


            if (_gameUi == null)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "GameUi is NULL.");

                return;
            }


            int playerCount =
                _playerNumSlider != null
                    ? Mathf.Clamp(
                        Mathf.RoundToInt(
                            _playerNumSlider.value),
                        MinPlayers,
                        MaxPlayers)
                    : 2;


            int difficulty =
                _hardnessSlider != null
                    ? Mathf.Clamp(
                        Mathf.RoundToInt(
                            _hardnessSlider.value),
                        MinDifficulty,
                        MaxDifficulty)
                    : 0;


            bool started =
                _gameUi.BeginNewMatch(
                    playerCount,
                    difficulty);


            if (!started)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "GameUi.BeginNewMatch() returned FALSE.");

                return;
            }


            SaveGameSetup(
                playerCount,
                difficulty);


            /*
             * مهم:
             *
             * خود GameEngine در زمان StartMatch
             * دو Trait متفاوت را به صورت تصادفی
             * برای بازیکن تولید کرده است.
             *
             * بنابراین اینجا Trait تولید نمی‌کنیم.
             * فقط همان دو گزینه را از GameUi می‌گیریم.
             */

            ShowTraitSelection();
        }


        // ============================================================
        // TRAIT SELECTION
        // ============================================================

        private void ShowTraitSelection()
        {
            if (_selectTraitPanel == null)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "SelectTraitPanel is NULL.");

                return;
            }

            if (_gameUi == null)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "GameUi is NULL.");

                return;
            }


            IReadOnlyList<PlayerTrait> options =
                _gameUi.TraitOptions;


            if (options == null)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "GameUi.TraitOptions is NULL.");

                return;
            }


            Debug.Log(
                "[DeadManDrawMenuController] " +
                $"TraitOptions count = {options.Count}");


            if (options.Count < 2)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "GameUi did not provide two Trait options.");

                return;
            }


            _trait1 = options[0];
            _trait2 = options[1];


            Debug.Log(
                "[DeadManDrawMenuController] " +
                $"Trait 1 = {_trait1}");

            Debug.Log(
                "[DeadManDrawMenuController] " +
                $"Trait 2 = {_trait2}");


            UpdateTraitUI();


            HideAllMenuPanels();


            _selectTraitPanel.SetActive(true);


            if (_traitHelpPanel != null)
                _traitHelpPanel.SetActive(false);


            Debug.Log(
                "[DeadManDrawMenuController] " +
                "SelectTraitPanel shown.");
        }


        private void UpdateTraitUI()
        {
            UpdateTraitImage(
                _trait1Image,
                _trait1);

            UpdateTraitImage(
                _trait2Image,
                _trait2);
        }


        private void UpdateTraitImage(
            Image image,
            PlayerTrait trait)
        {
            if (image == null)
                return;


            TraitVisualData data =
                GetTraitData(trait);


            if (data == null)
            {
                image.sprite = null;

                Debug.LogWarning(
                    "[DeadManDrawMenuController] " +
                    $"No visual data found for Trait '{trait}'.");

                return;
            }


            image.sprite = data.image;

            image.enabled =
                data.image != null;
        }


        // ============================================================
        // TRAIT BUTTONS
        // ============================================================

        private void SelectTrait1()
        {
            SelectTrait(_trait1);
        }


        private void SelectTrait2()
        {
            SelectTrait(_trait2);
        }


        private void SelectTrait(
            PlayerTrait trait)
        {
            if (trait == PlayerTrait.None)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "Trying to select None Trait.");

                return;
            }

            if (_gameUi == null)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "GameUi is NULL.");

                return;
            }

            /*
             * Trait را به GameEngine می‌فرستیم.
             */
            bool selected =
                _gameUi.SelectTrait(trait);

            if (!selected)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    $"Could not select Trait '{trait}'.");

                return;
            }

            Debug.Log(
                "[DeadManDrawMenuController] " +
                $"Trait selected: {trait}");


            /*
             * ابتدا Help را ببند.
             */
            if (_traitHelpPanel != null)
                _traitHelpPanel.SetActive(false);


            /*
             * سپس SelectTraitPanel را ببند.
             */
            if (_selectTraitPanel != null)
                _selectTraitPanel.SetActive(false);


            /*
             * حالا که بازیکن Trait خودش را انتخاب کرده،
             * اجازه می‌دهیم GameFlowController ادامه بازی
             * و Turnهای AI را اجرا کند.
             */
            _gameUi.ContinueAfterTraitSelection();


            Debug.Log(
                "[DeadManDrawMenuController] " +
                "Trait selection completed. " +
                "Gameplay continued.");
        }

        // ============================================================
        // TRAIT HELP
        // ============================================================

        private void ShowTrait1Help()
        {
            ShowTraitHelp(_trait1);
        }


        private void ShowTrait2Help()
        {
            ShowTraitHelp(_trait2);
        }


        private void ShowTraitHelp(
            PlayerTrait trait)
        {
            if (_traitHelpPanel == null)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "TraitHelpPanel is NULL.");

                return;
            }


            /*
             * نکته مهم:
             *
             * SelectTraitPanel را Hide نمی‌کنیم.
             *
             * TraitHelpPanel روی آن نمایش داده می‌شود.
             */

            UpdateTraitHelpText(trait);


            _traitHelpPanel.SetActive(true);


            Debug.Log(
                "[DeadManDrawMenuController] " +
                $"TraitHelpPanel opened for '{trait}'.");
        }


        private void UpdateTraitHelpText(
            PlayerTrait trait)
        {
            if (_traitHelpText == null)
                return;


            TraitVisualData data =
                GetTraitData(trait);


            if (data == null)
            {
                _traitHelpText.text =
                    GetDefaultTraitDescription(trait);

                return;
            }


            if (string.IsNullOrWhiteSpace(
                    data.description))
            {
                _traitHelpText.text =
                    GetDefaultTraitDescription(trait);

                return;
            }


            _traitHelpText.text =
                data.description;
        }


        private void CloseTraitHelp()
        {
            if (_traitHelpPanel != null)
                _traitHelpPanel.SetActive(false);


            /*
             * SelectTraitPanel عمداً دوباره فعال نمی‌شود،
             * چون از ابتدا فعال باقی مانده است.
             */

            Debug.Log(
                "[DeadManDrawMenuController] " +
                "TraitHelpPanel closed. " +
                "SelectTraitPanel remains visible.");
        }


        // ============================================================
        // TRAIT DATA
        // ============================================================

        private TraitVisualData GetTraitData(
            PlayerTrait trait)
        {
            if (_traitData == null)
                return null;


            for (int i = 0;
                 i < _traitData.Count;
                 i++)
            {
                TraitVisualData data =
                    _traitData[i];

                if (data != null &&
                    data.trait == trait)
                {
                    return data;
                }
            }


            return null;
        }


        // ============================================================
        // DEFAULT DESCRIPTIONS
        // ============================================================

        private string GetDefaultTraitDescription(
            PlayerTrait trait)
        {
            switch (trait)
            {
                case PlayerTrait.Beastmaster:
                    return
                        "Beastmaster\n\n" +
                        "توانایی ویژه مرتبط با موجودات و " +
                        "کارت‌های حیوانی.";


                case PlayerTrait.CaptainsHook:
                    return
                        "Captain's Hook\n\n" +
                        "توانایی ویژه کاپیتان برای استفاده " +
                        "بهتر از موقعیت‌های بازی.";


                case PlayerTrait.Casanova:
                    return
                        "Casanova\n\n" +
                        "توانایی ویژه برای تعامل و اثرگذاری " +
                        "بیشتر روی بازیکنان دیگر.";


                case PlayerTrait.DavyJonesLocker:
                    return
                        "Davy Jones' Locker\n\n" +
                        "توانایی ویژه مرتبط با خطر و " +
                        "گنجینه‌های دریایی.";


                case PlayerTrait.Fisherman:
                    return
                        "Fisherman\n\n" +
                        "توانایی ویژه در ارتباط با ماهیگیری " +
                        "و کارت‌های مربوط به آن.";


                case PlayerTrait.GoldenScales:
                    return
                        "Golden Scales\n\n" +
                        "توانایی ویژه برای مدیریت بهتر " +
                        "ریسک و پاداش.";


                case PlayerTrait.Miser:
                    return
                        "Miser\n\n" +
                        "توانایی ویژه برای حفظ و مدیریت " +
                        "بهتر سکه‌ها.";


                case PlayerTrait.Navigator:
                    return
                        "Navigator\n\n" +
                        "توانایی ویژه برای کنترل بهتر " +
                        "مسیر و انتخاب‌های بازی.";


                case PlayerTrait.MasterGunner:
                    return
                        "Master Gunner\n\n" +
                        "توانایی ویژه مرتبط با توپ و " +
                        "کارت‌های خطرناک.";


                case PlayerTrait.Misfire:
                    return
                        "Misfire\n\n" +
                        "توانایی ویژه مرتبط با شلیک ناموفق " +
                        "و تغییر نتیجه برخی موقعیت‌ها.";


                case PlayerTrait.Mystic:
                    return
                        "Mystic\n\n" +
                        "توانایی ویژه برای پیش‌بینی یا " +
                        "تأثیرگذاری بر اتفاقات بازی.";


                case PlayerTrait.Parry:
                    return
                        "Parry\n\n" +
                        "توانایی دفاعی برای مقابله با " +
                        "برخی حملات.";


                case PlayerTrait.Plunderer:
                    return
                        "Plunderer\n\n" +
                        "توانایی ویژه برای غارت و گرفتن " +
                        "منابع از بازیکنان دیگر.";


                case PlayerTrait.SafeHarbor:
                    return
                        "Safe Harbor\n\n" +
                        "توانایی دفاعی برای ایجاد یک " +
                        "موقعیت امن در بازی.";


                case PlayerTrait.Scavenger:
                    return
                        "Scavenger\n\n" +
                        "توانایی ویژه برای استفاده بهتر " +
                        "از منابع باقی‌مانده.";


                case PlayerTrait.Swordsman:
                    return
                        "Swordsman\n\n" +
                        "توانایی ویژه در مبارزه و " +
                        "مقابله با بازیکنان دیگر.";


                case PlayerTrait.TreasureHunter:
                    return
                        "Treasure Hunter\n\n" +
                        "توانایی ویژه برای پیدا کردن و " +
                        "به دست آوردن گنجینه‌ها.";


                default:
                    return
                        "No description available.";
            }
        }


        // ============================================================
        // PANEL MANAGEMENT
        // ============================================================

        private void HideAllMenuPanels()
        {
            if (_startPanel != null)
                _startPanel.SetActive(false);

            if (_helpPanel != null)
                _helpPanel.SetActive(false);

            if (_groupGamePanel != null)
                _groupGamePanel.SetActive(false);

            if (_newGamePanel != null)
                _newGamePanel.SetActive(false);

            if (_selectTraitPanel != null)
                _selectTraitPanel.SetActive(false);

            if (_traitHelpPanel != null)
                _traitHelpPanel.SetActive(false);
        }


        // ============================================================
        // SAVE / CONTINUE
        // ============================================================

        private void SaveGameSetup(
            int playerCount,
            int difficulty)
        {
            PlayerPrefs.SetInt(
                SavedGameKey,
                1);

            PlayerPrefs.SetInt(
                SavedPlayerCountKey,
                playerCount);

            PlayerPrefs.SetInt(
                SavedDifficultyKey,
                difficulty);

            PlayerPrefs.Save();
        }


        private bool HasSavedGame()
        {
            return PlayerPrefs.GetInt(
                       SavedGameKey,
                       0) == 1;
        }


        private void UpdateContinueButton()
        {
            if (_continueButton == null)
                return;

            _continueButton.interactable =
                HasSavedGame();
        }


        private void ContinueGame()
        {
            Debug.Log(
                "[DeadManDrawMenuController] " +
                "ContinueGame() CLICKED.");


            if (!HasSavedGame())
            {
                Debug.LogWarning(
                    "[DeadManDrawMenuController] " +
                    "No saved game exists.");

                return;
            }


            if (_gameUi == null)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "GameUi is NULL.");

                return;
            }


            int playerCount =
                PlayerPrefs.GetInt(
                    SavedPlayerCountKey,
                    2);


            int difficulty =
                PlayerPrefs.GetInt(
                    SavedDifficultyKey,
                    0);


            playerCount =
                Mathf.Clamp(
                    playerCount,
                    MinPlayers,
                    MaxPlayers);


            difficulty =
                Mathf.Clamp(
                    difficulty,
                    MinDifficulty,
                    MaxDifficulty);


            bool started =
                _gameUi.BeginNewMatch(
                    playerCount,
                    difficulty);


            if (!started)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "Could not continue game.");

                return;
            }


            ShowTraitSelection();
        }
    }
}