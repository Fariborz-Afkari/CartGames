using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CardGames.DeadManDraws.Core.Players;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class DeadManDrawMenuController : MonoBehaviour
    {
        private const string SaveExistsKey =
            "CardGames.DeadManDraws.SaveExists";

        private const string SavedPlayerCountKey =
            "CardGames.DeadManDraws.SavedPlayerCount";

        private const string SavedDifficultyKey =
            "CardGames.DeadManDraws.SavedDifficulty";

        private const string InitialScene =
            "CartGamesFirst";

        private GameUi gameUi;

        private GameObject startPanel;
        private GameObject helpPanel;
        private GameObject groupGamePanel;
        private GameObject newGamePanel;
        private GameObject selectTraitPanel;

        private Button continueButton;

        private Slider playerNumSlider;
        private Slider hardnessSlider;

        private TMP_Text txtNumPlayer;

        private readonly List<Button> traitButtons =
            new List<Button>();

        private void Awake()
        {
            gameUi = GetComponent<GameUi>();

            startPanel = FindObject("StartPanel");
            helpPanel = FindObject("HelpPanel");
            groupGamePanel = FindObject("GroupGamePanel");
            newGamePanel = FindObject("NewGamePanel");
            selectTraitPanel = FindObject("SelectTraitPanel");

            continueButton =
                FindButton(startPanel, "Continue_Button");

            playerNumSlider =
                FindComponent<Slider>("PlayerNumSlider");

            hardnessSlider =
                FindComponent<Slider>("HardnessSlider");

            txtNumPlayer =
                FindComponent<TMP_Text>("txtNumPlayer");

            SetupStartPanel();
            SetupNewGamePanel();
            SetupTraitPanel();
            SetupSliders();

            ShowStartPanel();
        }

        private void SetupStartPanel()
        {
            AddClick(
                startPanel,
                "NewGame_Button",
                ShowNewGamePanel);

            AddClick(
                startPanel,
                "Help_Button",
                ShowHelpPanel);

            AddClick(
                startPanel,
                "GroupGame_Button",
                ShowGroupGamePanel);

            AddClick(
                startPanel,
                "Continue_Button",
                ContinueGame);

            AddClick(
                startPanel,
                "Back_Button",
                BackToInitialScene);
        }

        private void SetupNewGamePanel()
        {
            AddClick(
                newGamePanel,
                "Back_Button",
                ShowStartPanel);

            AddClick(
                newGamePanel,
                "Start_Button",
                StartNewGame);
        }

        private void SetupSliders()
        {
            if (playerNumSlider != null)
            {
                playerNumSlider.minValue = 2;
                playerNumSlider.maxValue = 5;
                playerNumSlider.wholeNumbers = true;

                playerNumSlider.onValueChanged.AddListener(
                    OnPlayerNumberChanged);

                OnPlayerNumberChanged(
                    playerNumSlider.value);
            }

            if (hardnessSlider != null)
            {
                // 0 = Easy
                // 1 = Normal
                // 2 = Hard
                hardnessSlider.minValue = 0;
                hardnessSlider.maxValue = 2;
                hardnessSlider.wholeNumbers = true;
            }
        }

        private void OnPlayerNumberChanged(float value)
        {
            if (txtNumPlayer != null)
            {
                txtNumPlayer.text =
                    Mathf.RoundToInt(value).ToString();
            }
        }

        private void SetupTraitPanel()
        {
            if (selectTraitPanel == null)
                return;

            Button[] buttons =
                selectTraitPanel
                    .GetComponentsInChildren<Button>(true);

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = false;
                traitButtons.Add(buttons[i]);
            }
        }

        private void StartNewGame()
        {
            int playerCount =
                Mathf.RoundToInt(
                    playerNumSlider != null
                        ? playerNumSlider.value
                        : 4);

            int difficulty =
                Mathf.RoundToInt(
                    hardnessSlider != null
                        ? hardnessSlider.value
                        : 1);

            playerCount =
                Mathf.Clamp(playerCount, 2, 5);

            difficulty =
                Mathf.Clamp(difficulty, 0, 2);

            if (gameUi == null)
                return;

            bool started =
                gameUi.BeginNewMatch(
                    playerCount,
                    difficulty);

            if (!started)
                return;

            PlayerPrefs.SetInt(
                SaveExistsKey,
                1);

            PlayerPrefs.SetInt(
                SavedPlayerCountKey,
                playerCount);

            PlayerPrefs.SetInt(
                SavedDifficultyKey,
                difficulty);

            PlayerPrefs.Save();

            SetupTraitButtons();

            ShowOnly(selectTraitPanel);
        }

        private void SetupTraitButtons()
        {
            IReadOnlyList<PlayerTrait> traits =
                gameUi.TraitOptions;

            for (int i = 0; i < traitButtons.Count; i++)
            {
                Button button = traitButtons[i];

                button.onClick.RemoveAllListeners();

                if (i >= traits.Count)
                {
                    button.interactable = false;
                    continue;
                }

                PlayerTrait trait = traits[i];

                button.interactable = true;

                TMP_Text text =
                    button.GetComponentInChildren<TMP_Text>(
                        true);

                if (text != null)
                    text.text = trait.ToString();

                button.onClick.AddListener(
                    () => SelectTrait(trait));
            }
        }

        private void SelectTrait(PlayerTrait trait)
        {
            if (!gameUi.SelectTrait(trait))
                return;

            // Trait انتخاب شد؛ منوها بسته می‌شوند
            // و صفحه اصلی بازی نمایش داده می‌شود.
            HideMenuPanels();
            ShowGameBoard();

            // دیگر Save مربوط به "شروع نشده" نیست.
            // در سیستم Save واقعی باید اینجا State ذخیره شود.
        }

        private void ContinueGame()
        {
            if (!HasSavedGame())
                return;

            int playerCount =
                PlayerPrefs.GetInt(
                    SavedPlayerCountKey,
                    4);

            int difficulty =
                PlayerPrefs.GetInt(
                    SavedDifficultyKey,
                    1);

            if (!gameUi.BeginNewMatch(
                    playerCount,
                    difficulty))
            {
                return;
            }

            SetupTraitButtons();

            ShowOnly(selectTraitPanel);
        }

        private bool HasSavedGame()
        {
            return PlayerPrefs.GetInt(
                SaveExistsKey,
                0) == 1;
        }

        private void ShowStartPanel()
        {
            ShowOnly(startPanel);

            if (continueButton != null)
                continueButton.interactable =
                    HasSavedGame();
        }

        private void ShowNewGamePanel()
        {
            ShowOnly(newGamePanel);
        }

        private void ShowHelpPanel()
        {
            ShowOnly(helpPanel);
        }

        private void ShowGroupGamePanel()
        {
            ShowOnly(groupGamePanel);
        }

        private void BackToInitialScene()
        {
            SceneManager.LoadScene(
                InitialScene);
        }

        private void ShowOnly(GameObject panel)
        {
            HideMenuPanels();

            if (panel != null)
                panel.SetActive(true);

            HideGameBoard();
        }

        private void HideMenuPanels()
        {
            SetActive(startPanel, false);
            SetActive(helpPanel, false);
            SetActive(groupGamePanel, false);
            SetActive(newGamePanel, false);
            SetActive(selectTraitPanel, false);
        }

        private void HideGameBoard()
        {
            string[] names =
            {
                "Background",
                "TopArea",
                "PlayerArea",
                "TurnIndicator",
                "BoardPanel",
                "DrawDeck",
                "BurnDeck",
                "Trait",
                "CollectButton",
                "PlayerBanks",
                "Score",
                "PlayerDetailsPopup",
                "CardsPopup",
                "ConfirmationPopup",
                "PausePopup",
                "VictoryPopup",
                "DefeatPopup",
                "Notification"
            };

            for (int i = 0; i < names.Length; i++)
                SetActive(
                    FindObject(names[i]),
                    false);
        }

        private void ShowGameBoard()
        {
            string[] names =
            {
                "Background",
                "TopArea",
                "PlayerArea",
                "TurnIndicator",
                "BoardPanel",
                "DrawDeck",
                "BurnDeck",
                "Trait",
                "CollectButton",
                "PlayerBanks",
                "Score",
                "Notification"
            };

            for (int i = 0; i < names.Length; i++)
                SetActive(
                    FindObject(names[i]),
                    true);
        }

        private void AddClick(
            GameObject parent,
            string buttonName,
            UnityEngine.Events.UnityAction action)
        {
            Button button =
                FindButton(
                    parent,
                    buttonName);

            if (button == null)
                return;

            button.onClick.AddListener(action);
        }

        private Button FindButton(
            GameObject parent,
            string name)
        {
            if (parent == null)
                return null;

            Transform child =
                FindDeepChild(
                    parent.transform,
                    name);

            return child != null
                ? child.GetComponent<Button>()
                : null;
        }

        private T FindComponent<T>(
            string name)
            where T : Component
        {
            Transform child =
                FindDeepChild(
                    transform,
                    name);

            return child != null
                ? child.GetComponent<T>()
                : null;
        }

        private GameObject FindObject(
            string name)
        {
            Transform child =
                FindDeepChild(
                    transform,
                    name);

            return child != null
                ? child.gameObject
                : null;
        }

        private static Transform FindDeepChild(
            Transform root,
            string name)
        {
            if (root == null)
                return null;

            if (root.name == name)
                return root;

            for (int i = 0;
                 i < root.childCount;
                 i++)
            {
                Transform result =
                    FindDeepChild(
                        root.GetChild(i),
                        name);

                if (result != null)
                    return result;
            }

            return null;
        }

        private static void SetActive(
            GameObject target,
            bool value)
        {
            if (target != null)
                target.SetActive(value);
        }
    }
}