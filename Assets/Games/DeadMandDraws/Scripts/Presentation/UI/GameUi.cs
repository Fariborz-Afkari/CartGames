using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGames.DeadManDraws.Core.Players;
using CardGames.DeadManDraws.Presentation.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class GameUi : MonoBehaviour
    {
        // ============================================================
        // STATUS
        // ============================================================

        [Header("Status")]
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _turnText;

        // ============================================================
        // PLAYERS
        // ============================================================

        [Header("Players")]
        [SerializeField] private Transform _playersContainer;
        [SerializeField] private PlayerPanelUi _playerPanelPrefab;

        // ============================================================
        // BOARD
        // ============================================================

        [Header("Board")]
        [SerializeField] private BoardCardSlotUi[] _boardSlots;

        // ============================================================
        // BUTTONS
        // ============================================================

        [Header("Buttons")]
        [SerializeField] private Button _drawDeckButton;
        [SerializeField] private Button _collectCardsButton;
        [SerializeField]
        private Button _backButton;

        // ============================================================
        // TEXT
        // ============================================================

        [Header("Texts")]
        [SerializeField] private TMP_Text _coinsText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _deckText;
        [SerializeField] private TMP_Text _burnText;
        [SerializeField] private TMP_Text _turnLogText;

        // ============================================================
        // TRAIT
        // ============================================================

        [Header("Current Trait")]
        [SerializeField] private Image _traitImage;

        // ============================================================
        // NOTIFICATION
        // ============================================================

        [Header("Notification")]
        [SerializeField] private CanvasGroup _notificationGroup;
        [SerializeField] private TMP_Text _notificationText;

        [SerializeField]
        private float _notificationDuration = 1.8f;

        // ============================================================
        // BANKS
        // ============================================================

        [Header("Player Banks")]
        [SerializeField]
        private RectTransform[] _playerBankAnchors;

        // ============================================================
        // TARGET
        // ============================================================

        [Header("Target")]
        [SerializeField] private GameObject _targetPanel;
        [SerializeField] private Transform _targetContainer;
        [SerializeField] private Button _targetButtonPrefab;

        // ============================================================
        // CARDS
        // ============================================================

        [Header("Cards")]
        [SerializeField] private Transform _handContainer;
        [SerializeField] private CardWidget _cardWidgetPrefab;

        // ============================================================
        // RUNTIME
        // ============================================================

        private GameViewModel _viewModel;

        private readonly Dictionary<int, PlayerPanelUi>
            _playerPanels =
                new Dictionary<int, PlayerPanelUi>();

        private Coroutine _notificationRoutine;

        private int? _selectedCardId;

        private string _lastNotification;

        // ============================================================
        // TRAIT
        // ============================================================

        public IReadOnlyList<PlayerTrait>
            TraitOptions
        {
            get
            {
                if (_viewModel == null)
                    return Array.Empty<PlayerTrait>();

                return _viewModel.TraitOptions
                       ?? Array.Empty<PlayerTrait>();
            }
        }
        public bool SelectTrait(PlayerTrait trait)
        {
            if (_viewModel == null)
            {
                Debug.LogError(
                    "[GameUi] GameViewModel is NULL.");

                return false;
            }

            if (trait == PlayerTrait.None)
            {
                Debug.LogError(
                    "[GameUi] Cannot select None Trait.");

                return false;
            }

            bool result =
                _viewModel.SelectTrait(trait);

            Debug.Log(
                $"[GameUi] SelectTrait: " +
                $"{trait} -> {result}");

            return result;
        }
        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            _viewModel =
                new GameViewModel();

            ResolveSceneReferences();

            RegisterListeners();

            HideTargetPanel();

            if (_notificationGroup != null)
            {
                _notificationGroup.alpha = 0f;
                _notificationGroup.gameObject.SetActive(false);
            }

            ClearInitialBanks();
        }

        private void OnEnable()
        {
            if (_viewModel != null)
                _viewModel.Changed += Refresh;
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            if (_viewModel != null)
                _viewModel.Changed -= Refresh;
        }

        private void OnDestroy()
        {
            UnregisterListeners();

            if (_notificationRoutine != null)
                StopCoroutine(
                    _notificationRoutine);

            if (_viewModel != null)
            {
                _viewModel.Dispose();
                _viewModel = null;
            }
        }

        // ============================================================
        // SCENE REFERENCES
        // ============================================================
        private TMP_Text FindTextByAnyName(
    params string[] names)
        {
            for (int i = 0;
                 i < names.Length;
                 i++)
            {
                Transform target =
                    FindChildRecursive(
                        transform,
                        names[i]);

                if (target == null)
                    continue;

                TMP_Text text =
                    target.GetComponent<TMP_Text>();

                if (text != null)
                    return text;
            }

            return null;
        }
        private void ResolveSceneReferences()
        {
            if (_statusText == null)
            {
                _statusText =
                    FindText(
                        "TopArea",
                        "StatusText");
            }

            if (_playersContainer == null)
            {
                _playersContainer =
                    FindTransform(
                        "PlayerArea");
            }

            if (_drawDeckButton == null)
            {
                _drawDeckButton =
                    FindButton(
                        "DrawDeck");
            }

            if (_collectCardsButton == null)
            {
                _collectCardsButton =
                    FindButton(
                        "CollectButton");
            }

            if(_scoreText == null)
{
                _scoreText =
                    FindTextByAnyName(
                        "txtScore",
                        "Score",
                        "Text");
            }

            if (_deckText == null)
            {
                _deckText =
                    FindText(
                        "DrawDeck",
                        "Text");
            }

            if (_burnText == null)
            {
                _burnText =
                    FindText(
                        "BurnDeck",
                        "Text");
            }

            if (_traitImage == null)
            {
                Transform trait =
                    FindTransform("Trait");

                if (trait != null)
                    _traitImage =
                        trait.GetComponent<Image>();
            }

            if (_notificationGroup == null)
            {
                Transform notification =
                    FindTransform(
                        "Notification");

                if (notification != null)
                {
                    _notificationGroup =
                        notification.GetComponent<
                            CanvasGroup>();
                }
            }

            if (_notificationText == null)
            {
                _notificationText =
                    FindText(
                        "Notification",
                        "Text");
            }

            if (_handContainer == null)
            {
                Transform hand =
                    FindTransform(
                        "Hand");

                if (hand != null)
                    _handContainer = hand;
            }

            if (_targetPanel == null)
            {
                Transform target =
                    FindTransform(
                        "TargetPanel");

                if (target != null)
                    _targetPanel =
                        target.gameObject;
            }

            if (_backButton == null)
            {
                _backButton =
                    FindButton("BackButton");

                if (_backButton == null)
                {
                    _backButton =
                        FindButton("Back_Button");
                }
            }
            ResolveBoardSlots();

            ResolveBankAnchors();

            ResolvePlayerPanels();
        }

        private void ResolvePlayerPanels()
        {
            if (_playersContainer == null)
                return;

            PlayerPanelUi[] existing =
                _playersContainer
                    .GetComponentsInChildren<
                        PlayerPanelUi>(
                        true);

            for (int i = 0;
                 i < existing.Length;
                 i++)
            {
                PlayerPanelUi panel =
                    existing[i];

                if (panel == null)
                    continue;

                int id = ExtractPlayerId(
                    panel.gameObject.name);

                if (id >= 0)
                    _playerPanels[id] =
                        panel;
            }

            /*
             * PlayerPanelهای Scene فعلی
             * PlayerPanelUi ندارند.
             *
             * بنابراین آن‌ها را پیدا کرده و
             * component را runtime اضافه می‌کنیم.
             */
            for (int i = 0;
                 i < _playersContainer.childCount;
                 i++)
            {
                Transform child =
                    _playersContainer.GetChild(i);

                if (!child.name.StartsWith(
                        "PlayerPanel_"))
                {
                    continue;
                }

                int id =
                    ExtractPlayerId(
                        child.name);

                if (id < 0)
                    continue;

                PlayerPanelUi panel =
                    child.GetComponent<
                        PlayerPanelUi>();

                if (panel == null)
                {
                    panel =
                        child.gameObject.AddComponent<
                            PlayerPanelUi>();
                }

                _playerPanels[id] =
                    panel;
            }
        }

        private void ResolveBoardSlots()
        {
            Transform board =
                FindTransform("BoardArea");

            if (board == null)
                return;

            BoardCardSlotUi[] slots =
                board.GetComponentsInChildren<
                    BoardCardSlotUi>(
                    true);

            if (slots.Length > 0)
                _boardSlots = slots;

            if (_boardSlots == null)
                return;

            for (int i = 0;
                 i < _boardSlots.Length;
                 i++)
            {
                if (_boardSlots[i] != null)
                    _boardSlots[i].Initialize(i);
            }
        }

        private void ResolveBankAnchors()
        {
            Transform banks =
                FindTransform("PlayerBanks");

            if (banks == null)
                return;

            RectTransform[] children =
                banks.GetComponentsInChildren<
                    RectTransform>(
                    true);

            List<RectTransform> result =
                new List<RectTransform>();

            for (int i = 0;
                 i < children.Length;
                 i++)
            {
                if (children[i] == banks)
                    continue;

                if (children[i].name.StartsWith(
                        "Bank_"))
                {
                    result.Add(children[i]);
                }
            }

            result.Sort(
                (a, b) =>
                    ExtractPlayerId(
                        a.name).CompareTo(
                        ExtractPlayerId(
                            b.name)));

            _playerBankAnchors =
                result.ToArray();
        }

        // ============================================================
        // LISTENERS
        // ============================================================

        private void RegisterListeners()
        {
            if (_drawDeckButton != null)
            {
                _drawDeckButton.onClick.RemoveListener(
                    OnDrawDeckClicked);

                _drawDeckButton.onClick.AddListener(
                    OnDrawDeckClicked);
            }

            if (_collectCardsButton != null)
            {
                _collectCardsButton.onClick.RemoveListener(
                    OnCollectCardsClicked);

                _collectCardsButton.onClick.AddListener(
                    OnCollectCardsClicked);
            }

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(
                    OnBackButtonClicked);

                _backButton.onClick.AddListener(
                    OnBackButtonClicked);
            }
        }

        private void UnregisterListeners()
        {
            if (_drawDeckButton != null)
                _drawDeckButton.onClick.RemoveListener(
                    OnDrawDeckClicked);

            if (_collectCardsButton != null)
                _collectCardsButton.onClick.RemoveListener(
                    OnCollectCardsClicked);

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(
                    OnBackButtonClicked);
            }
        }

        // ============================================================
        // REFRESH
        // ============================================================

        private void Refresh()
        {
            if (_viewModel == null)
                return;

            RefreshStatus();
            RefreshEconomy();
            RefreshDeck();
            RefreshPlayers();
            RefreshTrait();
            RefreshLog();
            RefreshActions();

            ShowNotificationIfNeeded(
                _viewModel.Status);
        }

        private void RefreshStatus()
        {
            if (_statusText != null)
                _statusText.text =
                    _viewModel.Status;

            if (_turnText != null)
            {
                _turnText.text =
                    _viewModel.IsGameOver
                        ? "GAME OVER"
                        : _viewModel.IsPlayerTurn
                            ? "YOUR TURN"
                            : "OPPONENT TURN";
            }
        }

        private void RefreshActions()
        {
            bool playerTurn =
                _viewModel.IsPlayerTurn &&
                !_viewModel.IsGameOver;

            if (_drawDeckButton != null)
            {
                _drawDeckButton.interactable =
                    playerTurn;
            }

            if (_collectCardsButton != null)
            {
                _collectCardsButton.interactable =
                    playerTurn;
            }
        }

        // ============================================================
        // ECONOMY
        // ============================================================

        private void RefreshEconomy()
        {
            if (_coinsText != null)
            {
                _coinsText.text =
                    _viewModel.Coins.ToString();
            }

            if (_scoreText != null)
            {
                _scoreText.text =
                    _viewModel.Score.ToString();
            }
        }

        // ============================================================
        // DECK
        // ============================================================

        private void RefreshDeck()
        {
            if (_deckText != null)
            {
                _deckText.text =
                    _viewModel.DeckCount.ToString();
            }

            if (_burnText != null)
            {
                _burnText.text =
                    _viewModel.DiscardCount.ToString();
            }
        }

        // ============================================================
        // TRAIT
        // ============================================================

        private void RefreshTrait()
        {
            if (_traitImage == null)
                return;

            /*
             * تصویر Trait در مرحله انتخاب توسط
             * DeadManDrawMenuController تنظیم می‌شود.
             *
             * اینجا فقط وقتی Trait واقعی داریم
             * آن را نگه می‌داریم.
             */
        }

        public void SetSelectedTraitVisual(
            Sprite sprite)
        {
            if (_traitImage == null)
                return;

            _traitImage.sprite =
                sprite;

            _traitImage.enabled =
                sprite != null;
        }

        // ============================================================
        // PLAYERS
        // ============================================================

        private void RefreshPlayers()
        {
            IReadOnlyList<PlayerViewData> players =
                _viewModel.Players;

            if (players == null)
                return;

            for (int i = 0;
                 i < players.Count;
                 i++)
            {
                PlayerViewData player =
                    players[i];

                if (!_playerPanels.TryGetValue(
                        player.PlayerId,
                        out PlayerPanelUi panel))
                {
                    panel =
                        FindOrCreatePlayerPanel(
                            player.PlayerId);

                    if (panel == null)
                        continue;

                    _playerPanels[
                        player.PlayerId] =
                        panel;

                    int capturedId =
                        player.PlayerId;

                    panel.Clicked += () =>
                        OnPlayerClicked(
                            capturedId);
                }

                Sprite avatar =
                    FindAvatarSprite(
                        player.AvatarIndex);

                panel.Bind(
                    player,
                    player.PlayerId ==
                        _viewModel.CurrentPlayerId,
                    avatar);

                panel.transform.SetSiblingIndex(i);
            }

            /*
             * بازیکن‌هایی که در Match نیستند
             * پنهان شوند.
             */
            HashSet<int> alive =
                new HashSet<int>(
                    players.Select(
                        p => p.PlayerId));

            foreach (KeyValuePair<int,
                     PlayerPanelUi> pair
                     in _playerPanels)
            {
                if (pair.Value == null)
                    continue;

                pair.Value.gameObject.SetActive(
                    alive.Contains(pair.Key));
            }
        }

        private PlayerPanelUi
            FindOrCreatePlayerPanel(
                int playerId)
        {
            if (_playersContainer == null)
                return null;

            string wantedName =
                "PlayerPanel_" +
                playerId.ToString("00");

            Transform existing =
                FindChildRecursive(
                    _playersContainer,
                    wantedName);

            if (existing != null)
            {
                PlayerPanelUi panel =
                    existing.GetComponent<
                        PlayerPanelUi>();

                if (panel == null)
                {
                    panel =
                        existing.gameObject
                            .AddComponent<
                                PlayerPanelUi>();
                }

                return panel;
            }

            if (_playerPanelPrefab == null)
                return null;

            return Instantiate(
                _playerPanelPrefab,
                _playersContainer);
        }

        // ============================================================
        // CARDS
        // ============================================================

        private void OnDrawDeckClicked()
        {
            if (_viewModel == null)
                return;

            _viewModel.DrawCard();
        }

        private void OnCollectCardsClicked()
        {
            if (_viewModel == null)
                return;

            _viewModel.StopDrawing();
        }

        private void OnBackButtonClicked()
        {
            DeadManDrawMenuController menu =
                FindFirstObjectByType<
                    DeadManDrawMenuController>();

            if (menu != null)
            {
                //menu.ReturnToMenu();
                return;
            }

            Debug.LogWarning(
                "[GameUi] DeadManDrawMenuController " +
                "could not be found.");
        }

        // ============================================================
        // TARGET
        // ============================================================

        private void OnPlayerClicked(
            int playerId)
        {
            /*
             * برای انتخاب Target در نسخه فعلی
             * از CardInteraction استفاده می‌کنیم.
             */
        }

        private void ShowTargetPanel(
            IReadOnlyList<TargetViewData> targets)
        {
            if (_targetPanel == null)
                return;

            if (_targetContainer != null)
            {
                for (int i =
                     _targetContainer.childCount - 1;
                     i >= 0;
                     i--)
                {
                    Destroy(
                        _targetContainer
                            .GetChild(i)
                            .gameObject);
                }
            }

            if (_targetButtonPrefab != null &&
                _targetContainer != null &&
                targets != null)
            {
                for (int i = 0;
                     i < targets.Count;
                     i++)
                {
                    TargetViewData target =
                        targets[i];

                    Button button =
                        Instantiate(
                            _targetButtonPrefab,
                            _targetContainer);

                    TMP_Text text =
                        button.GetComponentInChildren<
                            TMP_Text>();

                    if (text != null)
                        text.text =
                            target.PlayerName;

                    int targetId =
                        target.PlayerId;

                    button.onClick.AddListener(
                        () =>
                        {
                            if (!_selectedCardId.HasValue)
                                return;

                            int cardId =
                                _selectedCardId.Value;

                            HideTargetPanel();

                            _viewModel.PlayCard(
                                cardId,
                                targetId);
                        });
                }
            }

            _targetPanel.SetActive(true);
        }

        private void HideTargetPanel()
        {
            _selectedCardId = null;

            if (_targetPanel != null)
                _targetPanel.SetActive(false);
        }

        // ============================================================
        // LOG
        // ============================================================

        private void RefreshLog()
        {
            if (_turnLogText == null)
                return;

            IReadOnlyList<string> log =
                _viewModel.TurnLog;

            if (log == null ||
                log.Count == 0)
            {
                _turnLogText.text =
                    string.Empty;

                return;
            }

            _turnLogText.text =
                string.Join(
                    "\n",
                    log);
        }

        // ============================================================
        // NOTIFICATION
        // ============================================================

        private void ShowNotificationIfNeeded(
            string message)
        {
            if (string.IsNullOrWhiteSpace(
                    message))
            {
                return;
            }

            if (message == _lastNotification)
                return;

            _lastNotification =
                message;

            if (_notificationText != null)
                _notificationText.text =
                    message;

            if (_notificationGroup == null)
                return;

            if (_notificationRoutine != null)
            {
                StopCoroutine(
                    _notificationRoutine);
            }

            _notificationRoutine =
                StartCoroutine(
                    NotificationRoutine());
        }

        private IEnumerator NotificationRoutine()
        {
            _notificationGroup.gameObject
                .SetActive(true);

            _notificationGroup.alpha =
                1f;

            yield return new WaitForSecondsRealtime(
                _notificationDuration);

            float time = 0f;

            while (time < 0.2f)
            {
                time +=
                    Time.unscaledDeltaTime;

                _notificationGroup.alpha =
                    Mathf.Lerp(
                        1f,
                        0f,
                        time / 0.2f);

                yield return null;
            }

            _notificationGroup.alpha =
                0f;

            _notificationGroup.gameObject
                .SetActive(false);

            _notificationRoutine = null;
        }

        // ============================================================
        // BANKS
        // ============================================================

        private void ClearInitialBanks()
        {
            if (_playerBankAnchors == null)
                return;

            for (int i = 0;
                 i < _playerBankAnchors.Length;
                 i++)
            {
                RectTransform bank =
                    _playerBankAnchors[i];

                if (bank == null)
                    continue;

                /*
                 * فقط Childهای runtime پاک می‌شوند.
                 * خود Anchor حفظ می‌شود.
                 */
                for (int j =
                     bank.childCount - 1;
                     j >= 0;
                     j--)
                {
                    Destroy(
                        bank.GetChild(j)
                            .gameObject);
                }
            }
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private Sprite FindAvatarSprite(
            int avatarIndex)
        {
            /*
             * Avatarهای از قبل موجود در Scene
             * دست‌نخورده باقی می‌مانند.
             *
             * اگر بعداً آرایه Avatar Sprite
             * اضافه شود می‌توانیم اینجا مستقیماً
             * آن را وصل کنیم.
             */
            return null;
        }

        private Button FindButton(
            string rootName)
        {
            Transform root =
                FindTransform(rootName);

            if (root == null)
                return null;

            return root.GetComponent<Button>();
        }

        private TMP_Text FindText(
            string parentName,
            string childName)
        {
            Transform parent =
                FindTransform(parentName);

            if (parent == null)
                return null;

            Transform child =
                FindChildRecursive(
                    parent,
                    childName);

            if (child == null)
                return null;

            return child.GetComponent<TMP_Text>();
        }

        private Transform FindTransform(
            string objectName)
        {
            GameObject objectFound =
                GameObject.Find(objectName);

            return objectFound != null
                ? objectFound.transform
                : null;
        }

        private static Transform
            FindChildRecursive(
                Transform parent,
                string childName)
        {
            if (parent == null)
                return null;

            if (parent.name == childName)
                return parent;

            for (int i = 0;
                 i < parent.childCount;
                 i++)
            {
                Transform result =
                    FindChildRecursive(
                        parent.GetChild(i),
                        childName);

                if (result != null)
                    return result;
            }

            return null;
        }

        private static int ExtractPlayerId(
            string objectName)
        {
            if (string.IsNullOrEmpty(
                    objectName))
            {
                return -1;
            }

            int underscore =
                objectName.LastIndexOf('_');

            if (underscore < 0)
                return -1;

            string number =
                objectName.Substring(
                    underscore + 1);

            int result;

            return int.TryParse(
                number,
                out result)
                ? result
                : -1;
        }

        public bool BeginNewMatch(int playerCount, int aiDifficulty)
        {
            if (_viewModel == null)
            {
                Debug.LogError("[GameUi] GameViewModel is NULL.");
                return false;
            }

            if (playerCount < 1)
            {
                Debug.LogError(
                    $"[GameUi] Invalid player count: {playerCount}");

                return false;
            }

            try
            {
                _viewModel.StartMatch(
                    playerCount,
                    aiDifficulty);

                Debug.Log(
                    $"[GameUi] BeginNewMatch: " +
                    $"players={playerCount}, " +
                    $"difficulty={aiDifficulty}");

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return false;
            }
        }
    }
}