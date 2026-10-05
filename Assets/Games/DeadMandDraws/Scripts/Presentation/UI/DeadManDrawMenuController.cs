using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using CardGames.DeadManDraws.Core.Players;
using CardGames.DeadManDraws.Presentation.UI;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class DeadManDrawMenuController : MonoBehaviour
    {
        [Header("Main UI")]
        [SerializeField] private GameUi _gameUi;

        [Header("Panels")]
        [SerializeField] private GameObject _startPanel;
        [SerializeField] private GameObject _helpPanel;
        [SerializeField] private GameObject _groupGamePanel;
        [SerializeField] private GameObject _newGamePanel;
        [SerializeField] private GameObject _selectTraitPanel;

        [Header("New Game")]
        [SerializeField] private Slider _playerNumSlider;
        [SerializeField] private Slider _hardnessSlider;
        [SerializeField] private TMP_Text _txtNumPlayer;

        [Header("Initial Scene")]
        [SerializeField] private string _initialSceneName = "CartGamesFirst";

        private Button _startPanelBackButton;
        private Button _helpButton;
        private Button _groupGameButton;
        private Button _continueButton;
        private Button _newGameButton;

        private Button _helpBackButton;
        private Button _groupGameBackButton;
        private Button _newGameBackButton;

        private Button _newGameStartButton;

        private Button _trait1Button;
        private Button _trait2Button;

        private const string SaveExistsKey =
            "DeadManDraws.Save.Exists";

        private const string SavePlayerCountKey =
            "DeadManDraws.Save.PlayerCount";

        private const string SaveDifficultyKey =
            "DeadManDraws.Save.Difficulty";


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            ResolveReferences();

            if (_gameUi == null)
            {
                _gameUi = GetComponent<GameUi>();
            }

            ConfigurePlayerSlider();
            ConfigureHardnessSlider();

            RegisterListeners();

            ShowStartPanel();

            UpdateContinueButton();
        }


        private void OnDestroy()
        {
            UnregisterListeners();
        }


        // =========================================================
        // REFERENCES
        // =========================================================

        private void ResolveReferences()
        {
            if (_startPanel == null)
                _startPanel = FindObjectByName("StartPanel");

            if (_helpPanel == null)
                _helpPanel = FindObjectByName("HelpPanel");

            if (_groupGamePanel == null)
                _groupGamePanel = FindObjectByName("GroupGamePanel");

            if (_newGamePanel == null)
                _newGamePanel = FindObjectByName("NewGamePanel");

            if (_selectTraitPanel == null)
                _selectTraitPanel = FindObjectByName("SelectTraitPanel");


            // -----------------------------------------------------
            // StartPanel buttons
            // -----------------------------------------------------

            if (_startPanel != null)
            {
                _helpButton =
                    FindButtonInside(_startPanel, "Help_Button");

                _groupGameButton =
                    FindButtonInside(_startPanel, "GroupGame_Button");

                _continueButton =
                    FindButtonInside(_startPanel, "Continue_Button");

                _newGameButton =
                    FindButtonInside(_startPanel, "NewGame_Button");

                _startPanelBackButton =
                    FindButtonInside(_startPanel, "Back_Button");
            }


            // -----------------------------------------------------
            // HelpPanel Back
            // -----------------------------------------------------

            if (_helpPanel != null)
            {
                _helpBackButton =
                    FindButtonInside(_helpPanel, "Back_Button");
            }


            // -----------------------------------------------------
            // GroupGamePanel Back
            // -----------------------------------------------------

            if (_groupGamePanel != null)
            {
                _groupGameBackButton =
                    FindButtonInside(_groupGamePanel, "Back_Button");
            }


            // -----------------------------------------------------
            // NewGamePanel
            // -----------------------------------------------------

            if (_newGamePanel != null)
            {
                _newGameBackButton =
                    FindButtonInside(_newGamePanel, "Back_Button");

                _newGameStartButton =
                    FindButtonInside(_newGamePanel, "Start_Button");

                if (_playerNumSlider == null)
                {
                    _playerNumSlider =
                        FindComponentInside<Slider>(
                            _newGamePanel,
                            "PlayerNumSlider");
                }

                if (_hardnessSlider == null)
                {
                    _hardnessSlider =
                        FindComponentInside<Slider>(
                            _newGamePanel,
                            "HardnessSlider");
                }

                if (_txtNumPlayer == null)
                {
                    _txtNumPlayer =
                        FindComponentInside<TMP_Text>(
                            _newGamePanel,
                            "txtNumPlayer");
                }
            }


            // -----------------------------------------------------
            // SelectTraitPanel
            // -----------------------------------------------------

            if (_selectTraitPanel != null)
            {
                _trait1Button =
                    FindButtonInside(
                        _selectTraitPanel,
                        "Trait1_Button");

                _trait2Button =
                    FindButtonInside(
                        _selectTraitPanel,
                        "Trait2_Button");
            }


            // -----------------------------------------------------
            // Diagnostic
            // -----------------------------------------------------

            Debug.Log(
                "[DeadManDrawMenuController] References resolved.\n" +
                $"GameUi: {_gameUi != null}\n" +
                $"StartPanel: {_startPanel != null}\n" +
                $"HelpPanel: {_helpPanel != null}\n" +
                $"GroupGamePanel: {_groupGamePanel != null}\n" +
                $"NewGamePanel: {_newGamePanel != null}\n" +
                $"SelectTraitPanel: {_selectTraitPanel != null}\n" +
                $"NewGame Start_Button: {_newGameStartButton != null}\n" +
                $"PlayerNumSlider: {_playerNumSlider != null}\n" +
                $"HardnessSlider: {_hardnessSlider != null}\n" +
                $"Trait1_Button: {_trait1Button != null}\n" +
                $"Trait2_Button: {_trait2Button != null}"
            );
        }


        private GameObject FindObjectByName(string objectName)
        {
            Transform[] allTransforms =
                GetComponentsInChildren<Transform>(true);

            foreach (Transform t in allTransforms)
            {
                if (t.name == objectName)
                    return t.gameObject;
            }

            Debug.LogWarning(
                $"[DeadManDrawMenuController] Could not find object: {objectName}");

            return null;
        }


        private Button FindButtonInside(
            GameObject parent,
            string buttonName)
        {
            if (parent == null)
                return null;

            Transform[] allTransforms =
                parent.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in allTransforms)
            {
                if (t.name != buttonName)
                    continue;

                Button button =
                    t.GetComponent<Button>();

                if (button != null)
                    return button;
            }

            Debug.LogWarning(
                $"[DeadManDrawMenuController] Button '{buttonName}' " +
                $"was not found inside '{parent.name}'.");

            return null;
        }


        private T FindComponentInside<T>(
            GameObject parent,
            string objectName)
            where T : Component
        {
            if (parent == null)
                return null;

            Transform[] allTransforms =
                parent.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in allTransforms)
            {
                if (t.name != objectName)
                    continue;

                T component =
                    t.GetComponent<T>();

                if (component != null)
                    return component;
            }

            Debug.LogWarning(
                $"[DeadManDrawMenuController] Component '{typeof(T).Name}' " +
                $"on object '{objectName}' was not found inside '{parent.name}'.");

            return null;
        }


        // =========================================================
        // LISTENERS
        // =========================================================

        private void RegisterListeners()
        {
            // Remove first so this method is safe even if
            // Unity calls initialization more than once.
            UnregisterListeners();


            if (_helpButton != null)
            {
                _helpButton.onClick.AddListener(
                    OpenHelpPanel);
            }

            if (_groupGameButton != null)
            {
                _groupGameButton.onClick.AddListener(
                    OpenGroupGamePanel);
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(
                    ContinueGame);
            }

            if (_newGameButton != null)
            {
                _newGameButton.onClick.AddListener(
                    OpenNewGamePanel);
            }


            if (_startPanelBackButton != null)
            {
                _startPanelBackButton.onClick.AddListener(
                    BackToInitialScene);
            }


            if (_helpBackButton != null)
            {
                _helpBackButton.onClick.AddListener(
                    ShowStartPanel);
            }

            if (_groupGameBackButton != null)
            {
                _groupGameBackButton.onClick.AddListener(
                    ShowStartPanel);
            }

            if (_newGameBackButton != null)
            {
                _newGameBackButton.onClick.AddListener(
                    ShowStartPanel);
            }


            // =====================================================
            // THIS IS THE IMPORTANT PART
            //
            // NewGamePanel/Start_Button is explicitly registered
            // here. GameUi must NOT register this button.
            // =====================================================

            if (_newGameStartButton != null)
            {
                _newGameStartButton.onClick.AddListener(
                    StartNewGame);

                Debug.Log(
                    "[DeadManDrawMenuController] " +
                    "Start_Button listener registered.");
            }
            else
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "NewGamePanel/Start_Button was NOT found!");
            }


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


            RegisterTraitButtons();
        }


        private void UnregisterListeners()
        {
            if (_helpButton != null)
                _helpButton.onClick.RemoveListener(
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

            if (_startPanelBackButton != null)
                _startPanelBackButton.onClick.RemoveListener(
                    BackToInitialScene);

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
                _playerNumSlider.onValueChanged.RemoveListener(
                    OnPlayerCountChanged);

            if (_hardnessSlider != null)
                _hardnessSlider.onValueChanged.RemoveListener(
                    OnDifficultyChanged);

            if (_trait1Button != null)
                _trait1Button.onClick.RemoveListener(
                    SelectTrait1);

            if (_trait2Button != null)
                _trait2Button.onClick.RemoveListener(
                    SelectTrait2);
        }


        // =========================================================
        // SLIDERS
        // =========================================================

        private void ConfigurePlayerSlider()
        {
            if (_playerNumSlider == null)
                return;

            _playerNumSlider.minValue = 2;
            _playerNumSlider.maxValue = 8;
            _playerNumSlider.wholeNumbers = true;

            int savedPlayerCount =
                PlayerPrefs.GetInt(SavePlayerCountKey,2);

            savedPlayerCount =
                Mathf.Clamp(savedPlayerCount, 2, 5);

            _playerNumSlider.SetValueWithoutNotify(
                savedPlayerCount);

            UpdatePlayerCountText(savedPlayerCount);
        }


        private void ConfigureHardnessSlider()
        {
            if (_hardnessSlider == null)
                return;

            _hardnessSlider.minValue = 0;
            _hardnessSlider.maxValue = 2;
            _hardnessSlider.wholeNumbers = true;

            int savedDifficulty =
                PlayerPrefs.GetInt(
                    SaveDifficultyKey,
                    1);

            savedDifficulty =
                Mathf.Clamp(savedDifficulty, 0, 2);

            _hardnessSlider.SetValueWithoutNotify(
                savedDifficulty);
        }


        private void OnPlayerCountChanged(float value)
        {
            int playerCount =
                Mathf.RoundToInt(value);

            playerCount =
                Mathf.Clamp(playerCount, 2, 5);

            UpdatePlayerCountText(playerCount);
        }


        private void OnDifficultyChanged(float value)
        {
            int difficulty =
                Mathf.RoundToInt(value);

            difficulty =
                Mathf.Clamp(difficulty, 0, 2);

            Debug.Log(
                $"[DeadManDrawMenuController] AI difficulty: {difficulty}");
        }


        private void UpdatePlayerCountText(int playerCount)
        {
            if (_txtNumPlayer == null)
                return;

            _txtNumPlayer.text =
                playerCount.ToString();
        }


        // =========================================================
        // START PANEL
        // =========================================================

        private void ShowStartPanel()
        {
            SetPanel(_startPanel, true);
            SetPanel(_helpPanel, false);
            SetPanel(_groupGamePanel, false);
            SetPanel(_newGamePanel, false);
            SetPanel(_selectTraitPanel, false);

            UpdateContinueButton();

            Debug.Log(
                "[DeadManDrawMenuController] StartPanel shown.");
        }


        private void OpenHelpPanel()
        {
            SetPanel(_startPanel, false);
            SetPanel(_helpPanel, true);
            SetPanel(_groupGamePanel, false);
            SetPanel(_newGamePanel, false);
            SetPanel(_selectTraitPanel, false);

            Debug.Log(
                "[DeadManDrawMenuController] HelpPanel opened.");
        }


        private void OpenGroupGamePanel()
        {
            SetPanel(_startPanel, false);
            SetPanel(_helpPanel, false);
            SetPanel(_groupGamePanel, true);
            SetPanel(_newGamePanel, false);
            SetPanel(_selectTraitPanel, false);

            Debug.Log(
                "[DeadManDrawMenuController] GroupGamePanel opened.");
        }


        private void OpenNewGamePanel()
        {
            SetPanel(_startPanel, false);
            SetPanel(_helpPanel, false);
            SetPanel(_groupGamePanel, false);
            SetPanel(_newGamePanel, true);
            SetPanel(_selectTraitPanel, false);

            Debug.Log(
                "[DeadManDrawMenuController] NewGamePanel opened.");
        }


        // =========================================================
        // NEW GAME
        // =========================================================

        private void StartNewGame()
        {
            Debug.Log(
                "[DeadManDrawMenuController] " +
                "StartNewGame() CLICKED.");


            if (_gameUi == null)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "GameUi reference is NULL.");

                return;
            }


            int playerCount = 2;

            if (_playerNumSlider != null)
            {
                playerCount =
                    Mathf.RoundToInt(
                        _playerNumSlider.value);
            }

            playerCount =
                Mathf.Clamp(playerCount, 2, 5);


            int difficulty = 1;

            if (_hardnessSlider != null)
            {
                difficulty =
                    Mathf.RoundToInt(
                        _hardnessSlider.value);
            }

            difficulty =
                Mathf.Clamp(difficulty, 0, 2);


            Debug.Log(
                "[DeadManDrawMenuController] " +
                $"Starting match. Players={playerCount}, " +
                $"Difficulty={difficulty}");


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


            SaveMatchSetup(
                playerCount,
                difficulty);


            Debug.Log(
                "[DeadManDrawMenuController] " +
                "Match started successfully.");


            ShowTraitSelection();
        }


        // =========================================================
        // CONTINUE
        // =========================================================

        private void ContinueGame()
        {
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
                    "GameUi reference is NULL.");

                return;
            }


            int playerCount =
                PlayerPrefs.GetInt(
                    SavePlayerCountKey,
                    2);

            int difficulty =
                PlayerPrefs.GetInt(
                    SaveDifficultyKey,
                    1);


            playerCount =
                Mathf.Clamp(playerCount, 2, 5);

            difficulty =
                Mathf.Clamp(difficulty, 0, 2);


            bool started =
                _gameUi.BeginNewMatch(
                    playerCount,
                    difficulty);


            if (!started)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "Continue failed.");

                return;
            }


            ShowTraitSelection();
        }


        private void SaveMatchSetup(
            int playerCount,
            int difficulty)
        {
            PlayerPrefs.SetInt(
                SaveExistsKey,
                1);

            PlayerPrefs.SetInt(
                SavePlayerCountKey,
                playerCount);

            PlayerPrefs.SetInt(
                SaveDifficultyKey,
                difficulty);

            PlayerPrefs.Save();
        }


        private bool HasSavedGame()
        {
            return PlayerPrefs.GetInt(
                SaveExistsKey,
                0) == 1;
        }


        private void UpdateContinueButton()
        {
            if (_continueButton == null)
                return;

            _continueButton.interactable =
                HasSavedGame();
        }


        // =========================================================
        // TRAIT SELECTION
        // =========================================================

        private void ShowTraitSelection()
        {
            SetPanel(_startPanel, false);
            SetPanel(_helpPanel, false);
            SetPanel(_groupGamePanel, false);
            SetPanel(_newGamePanel, false);
            SetPanel(_selectTraitPanel, true);

            RegisterTraitButtons();

            Debug.Log(
                "[DeadManDrawMenuController] " +
                "SelectTraitPanel shown.");
        }


        private void RegisterTraitButtons()
        {
            if (_trait1Button != null)
            {
                _trait1Button.onClick.RemoveListener(
                    SelectTrait1);

                _trait1Button.onClick.AddListener(
                    SelectTrait1);
            }

            if (_trait2Button != null)
            {
                _trait2Button.onClick.RemoveListener(
                    SelectTrait2);

                _trait2Button.onClick.AddListener(
                    SelectTrait2);
            }
        }


        private void SelectTrait1()
        {
            SelectTraitByIndex(0);
        }


        private void SelectTrait2()
        {
            SelectTraitByIndex(1);
        }


        private void SelectTraitByIndex(int index)
        {
            if (_gameUi == null)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "GameUi is NULL.");

                return;
            }


            IReadOnlyList<PlayerTrait> traits =
                _gameUi.TraitOptions;


            if (traits == null ||
                traits.Count == 0)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "No TraitOptions are available.");

                return;
            }


            if (index < 0 ||
                index >= traits.Count)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    $"Trait index {index} is invalid. " +
                    $"Available traits: {traits.Count}");

                return;
            }


            PlayerTrait selectedTrait =
                traits[index];


            Debug.Log(
                "[DeadManDrawMenuController] " +
                $"Selecting trait: {selectedTrait}");


            bool selected =
                _gameUi.SelectTrait(
                    selectedTrait);


            if (!selected)
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "SelectTrait() returned FALSE.");

                return;
            }


            ShowGameBoard();
        }


        // =========================================================
        // GAME BOARD
        // =========================================================

        private void ShowGameBoard()
        {
            // IMPORTANT:
            //
            // We do NOT disable the actual game board.
            // Only menu panels are hidden.

            SetPanel(_startPanel, false);
            SetPanel(_helpPanel, false);
            SetPanel(_groupGamePanel, false);
            SetPanel(_newGamePanel, false);
            SetPanel(_selectTraitPanel, false);

            Debug.Log(
                "[DeadManDrawMenuController] " +
                "Menu hidden. Game board active.");
        }


        // =========================================================
        // BACK TO INITIAL SCENE
        // =========================================================

        private void BackToInitialScene()
        {
            if (string.IsNullOrWhiteSpace(
                _initialSceneName))
            {
                Debug.LogError(
                    "[DeadManDrawMenuController] " +
                    "Initial scene name is empty.");

                return;
            }


            Debug.Log(
                "[DeadManDrawMenuController] " +
                $"Loading initial scene: {_initialSceneName}");


            SceneManager.LoadScene(
                _initialSceneName);
        }


        // =========================================================
        // HELPERS
        // =========================================================

        private void SetPanel(
            GameObject panel,
            bool visible)
        {
            if (panel == null)
                return;

            panel.SetActive(visible);
        }
    }
}