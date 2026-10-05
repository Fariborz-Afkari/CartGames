using CardGames.DeadManDraws.Core.Players;
using CardGames.DeadManDraws.Presentation.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGames.DeadManDraws.Presentation.UI
{
    /// <summary>
    /// Main UI controller for the card-game board.
    ///
    /// Important:
    /// - GameUi only talks to GameViewModel.
    /// - GamePresenter/GameEngine should call the public animation hooks
    ///   when a state transition actually happens.
    /// - Card artwork/background remains inside CardWidget/card prefabs.
    /// </summary>
    public sealed class GameUi : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _turnText;

        [Header("Players")]
        [SerializeField] private Transform _playersContainer;
        [SerializeField] private PlayerPanelUi _playerPanelPrefab;

        [Header("Hand")]
        [SerializeField] private Transform _handContainer;
        [SerializeField] private CardWidget _cardWidgetPrefab;

        [Header("Board - 8 Slots")]
        [SerializeField] private BoardCardSlotUi[] _boardSlots = new BoardCardSlotUi[8];

        [Header("Decks")]
        [SerializeField] private Button _drawDeckButton;
        [SerializeField] private RectTransform _drawDeckAnchor;
        [SerializeField] private Button _burnDeckButton;
        [SerializeField] private RectTransform _burnDeckAnchor;

        [Header("Player Banks - 6 Blue Piles")]
        [SerializeField] private RectTransform[] _playerBankAnchors = new RectTransform[6];

        [Header("Actions")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private Button _buyCoinsButton;
        [SerializeField] private Button _collectCardsButton;

        [Header("Economy")]
        [SerializeField] private TMP_Text _coinsText;
        [SerializeField] private TMP_Text _scoreText;

        [Header("Deck")]
        [SerializeField] private TMP_Text _deckText;
        [SerializeField] private TMP_Text _burnText;

        [Header("Log")]
        [SerializeField] private TMP_Text _turnLogText;

        [Header("Target Selection")]
        [SerializeField] private GameObject _targetPanel;
        [SerializeField] private Transform _targetContainer;
        [SerializeField] private Button _targetButtonPrefab;

        [Header("Popup Panels")]
        [SerializeField] private GameObject _playerPanelRoot;
        [SerializeField] private PlayerDetailsPanelUi _playerDetailsPanel;
        [SerializeField] private GameObject _cardsPanelRoot;
        [SerializeField] private CardCollectionPanelUi _cardCollectionPanel;

        [Header("Animation")]
        [SerializeField, Min(0.05f)] private float _drawDuration = 0.42f;
        [SerializeField, Min(0.05f)] private float _burnDuration = 0.45f;
        [SerializeField, Min(0.05f)] private float _collectDuration = 0.55f;
        [SerializeField, Min(0.05f)] private float _playerReorderDuration = 0.35f;
        [SerializeField] private AnimationCurve _moveCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Audio")]
        [SerializeField] private UiAudio _uiAudio;

        private GameViewModel _viewModel;
        private readonly Dictionary<int, PlayerPanelUi> _playerPanels = new();
        private readonly List<Coroutine> _animations = new();
        private int? _selectedCardId;

        private void Awake()
        {
            _viewModel = new GameViewModel();

            RegisterListeners();

            if (_drawDeckButton != null)
                _drawDeckButton.onClick.AddListener(OnDrawDeckClicked);

            if (_burnDeckButton != null)
                _burnDeckButton.onClick.AddListener(OnBurnDeckClicked);

            if (_collectCardsButton != null)
                _collectCardsButton.onClick.AddListener(OnCollectCardsClicked);

            HideTargetPanel();
            CloseAllPopups();
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

            if (_drawDeckButton != null)
                _drawDeckButton.onClick.RemoveListener(OnDrawDeckClicked);

            if (_burnDeckButton != null)
                _burnDeckButton.onClick.RemoveListener(OnBurnDeckClicked);

            if (_collectCardsButton != null)
                _collectCardsButton.onClick.RemoveListener(OnCollectCardsClicked);

            StopAllAnimations();

            if (_viewModel != null)
            {
                _viewModel.Dispose();
                _viewModel = null;
            }
        }

        private void RegisterListeners()
        {
            // IMPORTANT:
            // NewGamePanel/Start_Button is owned by
            // DeadManDrawMenuController.
            //
            // Do NOT register GameUi.OnStartClicked() here.

            if (_endTurnButton != null)
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);

            if (_buyCoinsButton != null)
                _buyCoinsButton.onClick.AddListener(OnBuyCoinsClicked);
        }
        private void UnregisterListeners()
        {
            if (_endTurnButton != null)
                _endTurnButton.onClick.RemoveListener(OnEndTurnClicked);

            if (_buyCoinsButton != null)
                _buyCoinsButton.onClick.RemoveListener(OnBuyCoinsClicked);
        }
        private void Refresh()
        {
            if (_viewModel == null)
                return;

            RefreshStatus();
            RefreshActions();
            RefreshEconomy();
            RefreshDeck();
            RefreshPlayers();
            RefreshHand();
            RefreshLog();

            if (_viewModel.IsGameOver)
                HideTargetPanel();
        }

        private void RefreshStatus()
        {
            if (_statusText != null)
                _statusText.text = _viewModel.Status;

            if (_turnText != null)
            {
                _turnText.text = _viewModel.IsGameOver
                    ? "GAME OVER"
                    : _viewModel.IsPlayerTurn
                        ? "YOUR TURN"
                        : "OPPONENT TURN";
            }
        }

        private void RefreshActions()
        {
            bool playerTurn = _viewModel.IsPlayerTurn && !_viewModel.IsGameOver;

            if (_endTurnButton != null)
                _endTurnButton.interactable = playerTurn;

            if (_buyCoinsButton != null)
                _buyCoinsButton.interactable = playerTurn;

            if (_collectCardsButton != null)
                _collectCardsButton.interactable =
                    playerTurn && HasBoardCards();
        }

        private void RefreshEconomy()
        {
            if (_coinsText != null)
                _coinsText.text = $"COINS  {_viewModel.Coins}";

            if (_scoreText != null)
            {
                object score = ReadMember(_viewModel, "Score", "PlayerScore");
                _scoreText.text = score != null ? $"SCORE  {score}" : string.Empty;
            }
        }

        private void RefreshDeck()
        {
            if (_deckText != null)
                _deckText.text = $"DECK  {_viewModel.DeckCount}";

            if (_burnText != null)
            {
                object burnCount = ReadMember(
                    _viewModel,
                    "BurnedCount",
                    "DiscardCount",
                    "BurnCount");

                _burnText.text = burnCount != null
                    ? $"BURN  {burnCount}"
                    : "BURN";
            }
        }

        private void RefreshPlayers()
        {
            IReadOnlyList<PlayerViewData> source = _viewModel.Players;

            if (source == null || source.Count == 0)
                return;

            List<PlayerViewData> sorted = source
                .Select((player, index) => new { player, index })
                .OrderByDescending(x => ToInt(ReadMember(
                    x.player, "Score", "Points", "Health", "Resource")))
                .ThenBy(x => x.index)
                .Select(x => x.player)
                .ToList();

            // Create missing player panels.
            for (int i = 0; i < sorted.Count; i++)
            {
                PlayerViewData player = sorted[i];

                if (!_playerPanels.TryGetValue(player.PlayerId, out PlayerPanelUi panel))
                {
                    if (_playerPanelPrefab == null || _playersContainer == null)
                        continue;

                    panel = Instantiate(_playerPanelPrefab, _playersContainer);
                    _playerPanels[player.PlayerId] = panel;

                    int capturedId = player.PlayerId;
                    panel.Clicked += () => OnPlayerClicked(capturedId);
                }

                panel.Bind(
                    player,
                    player.PlayerId == _viewModel.CurrentPlayerId);
            }

            // Remove panels that no longer exist.
            HashSet<int> aliveIds = new(sorted.Select(p => p.PlayerId));
            List<int> staleIds = _playerPanels.Keys
                .Where(id => !aliveIds.Contains(id))
                .ToList();

            for (int i = 0; i < staleIds.Count; i++)
            {
                PlayerPanelUi panel = _playerPanels[staleIds[i]];
                if (panel != null)
                    Destroy(panel.gameObject);

                _playerPanels.Remove(staleIds[i]);
            }

            // Score order: highest score first.
            for (int i = 0; i < sorted.Count; i++)
            {
                if (_playerPanels.TryGetValue(
                    sorted[i].PlayerId,
                    out PlayerPanelUi panel))
                {
                    panel.transform.SetSiblingIndex(i);
                }
            }
        }

        private void RefreshHand()
        {
            if (_handContainer == null)
                return;

            ClearContainer(_handContainer);

            IReadOnlyList<CardViewData> hand = _viewModel.Hand;
            if (hand == null)
                return;

            for (int i = 0; i < hand.Count; i++)
                CreateCardWidget(hand[i], _handContainer, OnCardClicked);
        }

        private void CreateCardWidget(
            CardViewData card,
            Transform parent,
            Action<int> callback)
        {
            if (_cardWidgetPrefab == null || parent == null)
                return;

            CardWidget widget = Instantiate(_cardWidgetPrefab, parent);
            widget.Bind(card, callback);
        }

        private void OnCardClicked(int cardId)
        {
            if (!_viewModel.IsPlayerTurn || _viewModel.IsGameOver)
                return;

            CardInteraction interaction =
                _viewModel.GetCardInteraction(cardId);

            if (interaction == null)
                return;

            if (interaction.RequiresTarget)
            {
                _selectedCardId = cardId;
                ShowTargetPanel(interaction.Targets);
                return;
            }

            _viewModel.PlayCard(cardId);
        }

        private void ShowTargetPanel(
            IReadOnlyList<TargetViewData> targets)
        {
            if (_targetPanel == null)
                return;

            ClearContainer(_targetContainer);

            if (targets != null)
            {
                for (int i = 0; i < targets.Count; i++)
                    CreateTargetButton(targets[i]);
            }

            _targetPanel.SetActive(true);
        }

        private void CreateTargetButton(TargetViewData target)
        {
            if (_targetButtonPrefab == null ||
                _targetContainer == null ||
                target == null)
                return;

            Button button = Instantiate(
                _targetButtonPrefab,
                _targetContainer);

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>();

            if (text != null)
                text.text = target.PlayerName;

            int targetId = target.PlayerId;

            button.onClick.AddListener(() =>
            {
                if (!_selectedCardId.HasValue)
                    return;

                int cardId = _selectedCardId.Value;

                HideTargetPanel();
                _viewModel.PlayCard(cardId, targetId);
            });
        }

        private void HideTargetPanel()
        {
            _selectedCardId = null;

            if (_targetPanel != null)
                _targetPanel.SetActive(false);
        }

        private void RefreshLog()
        {
            if (_turnLogText == null)
                return;

            IReadOnlyList<string> log = _viewModel.TurnLog;

            if (log == null || log.Count == 0)
            {
                _turnLogText.text = string.Empty;
                return;
            }

            _turnLogText.text = string.Join("\n", log);
        }

        // ---------------------------------------------------------------------
        // PUBLIC ANIMATION HOOKS
        // Call these from GamePresenter after the corresponding game event.
        // ---------------------------------------------------------------------

        public void AnimateDraw(
            CardViewData card,
            int slotIndex,
            float duration = -1f)
        {
            if (!IsValidSlot(slotIndex) || card == null)
                return;

            BoardCardSlotUi slot = _boardSlots[slotIndex];

            if (_drawDeckAnchor == null || slot == null)
            {
                slot?.ShowCard(card, OnBoardCardClicked);
                return;
            }

            CardWidget widget = CreateFloatingCard(card);

            if (widget == null)
            {
                slot.ShowCard(card, OnBoardCardClicked);
                return;
            }

            _animations.Add(StartCoroutine(
                AnimateCardTo(
                    widget.gameObject,
                    _drawDeckAnchor,
                    slot.CardAnchor,
                    duration > 0 ? duration : _drawDuration,
                    false,
                    () =>
                    {
                        slot.ShowCard(card, OnBoardCardClicked);
                        slot.Attach(widget, card);
                        _uiAudio?.PlayDraw();
                    })));
        }

        public void AnimateBurn(
            int slotIndex,
            CardViewData card,
            float duration = -1f)
        {
            if (!IsValidSlot(slotIndex))
                return;

            BoardCardSlotUi slot = _boardSlots[slotIndex];

            if (card == null)
            {
                slot.Clear();
                return;
            }

            CardWidget widget = CreateFloatingCard(card);

            if (widget == null || _burnDeckAnchor == null)
            {
                slot.Clear();
                return;
            }

            widget.transform.position = slot.CardAnchor.position;
            slot.Clear();

            _animations.Add(StartCoroutine(
                AnimateCardTo(
                    widget.gameObject,
                    slot.CardAnchor,
                    _burnDeckAnchor,
                    duration > 0 ? duration : _burnDuration,
                    true,
                    () =>
                    {
                        Destroy(widget.gameObject);
                        _uiAudio?.PlayBurn();
                    })));
        }

        public void AnimateCollect(
            int slotIndex,
            int playerId,
            CardViewData card,
            float duration = -1f)
        {
            if (!IsValidSlot(slotIndex) || card == null)
                return;

            BoardCardSlotUi slot = _boardSlots[slotIndex];
            RectTransform target = GetBankAnchor(playerId);

            if (target == null)
            {
                slot.Clear();
                return;
            }

            CardWidget widget = CreateFloatingCard(card);

            if (widget == null)
            {
                slot.Clear();
                return;
            }

            widget.transform.position = slot.CardAnchor.position;
            slot.Clear();

            _animations.Add(StartCoroutine(
                AnimateCardTo(
                    widget.gameObject,
                    slot.CardAnchor,
                    target,
                    duration > 0 ? duration : _collectDuration,
                    true,
                    () =>
                    {
                        Destroy(widget.gameObject);
                        _uiAudio?.PlayCollect();
                    })));
        }

        public void AnimateAiCollection(
            int slotIndex,
            int playerId,
            CardViewData card)
        {
            // Same visual destination as the player's bank.
            // The AI decision/timing must remain in the AI/game layer.
            AnimateCollect(slotIndex, playerId, card);
        }

        public void OpenPlayerPanel(PlayerViewData player)
        {
            if (_playerDetailsPanel == null || player == null)
                return;

            if (_playerPanelRoot != null)
                _playerPanelRoot.SetActive(true);

            _playerDetailsPanel.Bind(player);
        }

        public void OpenCardBankPanel(
            string title,
            IReadOnlyList<CardViewData> cards)
        {
            if (_cardCollectionPanel == null)
                return;

            if (_cardsPanelRoot != null)
                _cardsPanelRoot.SetActive(true);

            _cardCollectionPanel.Bind(title, cards, _cardWidgetPrefab);
        }

        public void OpenBurnedCardsPanel(
            IReadOnlyList<CardViewData> cards)
        {
            OpenCardBankPanel("BURNED CARDS", cards);
        }

        public void CloseAllPopups()
        {
            if (_playerPanelRoot != null)
                _playerPanelRoot.SetActive(false);

            if (_cardsPanelRoot != null)
                _cardsPanelRoot.SetActive(false);

            HideTargetPanel();
        }

        // ---------------------------------------------------------------------
        // Button handlers
        // ---------------------------------------------------------------------


        private void OnEndTurnClicked()
        {
            if (_viewModel.IsPlayerTurn)
                _viewModel.EndTurn();
        }

        private void OnBuyCoinsClicked() =>
            _viewModel.BuyCoins();

        private void OnDrawDeckClicked()
        {
            _uiAudio?.PlayDeckClick();

            // Keeps this UI compatible with the current ViewModel while
            // allowing the game layer to expose DrawCard()/Draw().
            InvokeNoArg(
                _viewModel,
                "DrawCard",
                "Draw",
                "DrawFromDeck");
        }

        private void OnBurnDeckClicked()
        {
            _uiAudio?.PlayDeckClick();

            IReadOnlyList<CardViewData> cards =
                ReadCardList(
                    _viewModel,
                    "BurnedCards",
                    "DiscardPile",
                    "DiscardCards");

            if (cards != null)
                OpenBurnedCardsPanel(cards);
        }

        private void OnCollectCardsClicked()
        {
            _uiAudio?.PlayCollect();

            InvokeNoArg(
                _viewModel,
                "CollectCards",
                "CollectBoardCards",
                "CollectCardsToBank");
        }

        private void OnPlayerClicked(int playerId)
        {
            IReadOnlyList<PlayerViewData> players = _viewModel.Players;
            if (players == null)
                return;

            PlayerViewData player = players
                .FirstOrDefault(p => p.PlayerId == playerId);

            if (player != null)
                OpenPlayerPanel(player);
        }

        private void OnBoardCardClicked(int slotIndex)
        {
            // Board cards can be expanded later to support card detail/effect UI.
            BoardCardSlotUi slot = _boardSlots[slotIndex];
            slot?.OpenDetail();
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

        private CardWidget CreateFloatingCard(CardViewData card)
        {
            if (_cardWidgetPrefab == null)
                return null;

            // Put the temporary card under the same canvas as the board.
            Transform canvasRoot = GetComponentInParent<Canvas>()?.transform;
            if (canvasRoot == null)
                canvasRoot = transform;

            CardWidget widget = Instantiate(
                _cardWidgetPrefab,
                canvasRoot);

            widget.Bind(card, _ => { });
            return widget;
        }

        private IEnumerator AnimateCardTo(
            GameObject cardObject,
            RectTransform from,
            RectTransform to,
            float duration,
            bool rotateAndFade,
            Action completed)
        {
            if (cardObject == null || from == null || to == null)
            {
                completed?.Invoke();
                yield break;
            }

            RectTransform rect = cardObject.transform as RectTransform;
            if (rect == null)
            {
                completed?.Invoke();
                yield break;
            }

            rect.position = from.position;
            rect.localScale = Vector3.one;

            CanvasGroup group = cardObject.GetComponent<CanvasGroup>();
            if (group == null)
                group = cardObject.AddComponent<CanvasGroup>();

            group.alpha = 1f;

            Vector3 start = from.position;
            Vector3 end = to.position;
            Vector3 arc = Vector3.up * Mathf.Max(40f, Vector3.Distance(start, end) * 0.12f);

            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);
                float e = _moveCurve.Evaluate(t);

                Vector3 linear = Vector3.LerpUnclamped(start, end, e);
                float arcAmount = Mathf.Sin(t * Mathf.PI);
                rect.position = linear + arc * arcAmount;

                if (rotateAndFade)
                {
                    rect.localRotation =
                        Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 18f, t));

                    group.alpha = Mathf.Lerp(1f, 0.05f, t);
                    rect.localScale =
                        Vector3.Lerp(Vector3.one, Vector3.one * 0.72f, t);
                }

                yield return null;
            }

            rect.position = end;
            completed?.Invoke();
        }

        private RectTransform GetBankAnchor(int playerId)
        {
            IReadOnlyList<PlayerViewData> players = _viewModel.Players;
            if (players == null || _playerBankAnchors == null)
                return null;

            List<PlayerViewData> sorted = players
                .OrderByDescending(p =>
                    ToInt(ReadMember(
                        p, "Score", "Points", "Health", "Resource")))
                .ToList();

            int index = sorted.FindIndex(p => p.PlayerId == playerId);

            if (index < 0 || index >= _playerBankAnchors.Length)
                return null;

            return _playerBankAnchors[index];
        }

        private bool HasBoardCards()
        {
            for (int i = 0; i < _boardSlots.Length; i++)
            {
                if (_boardSlots[i] != null && _boardSlots[i].HasCard)
                    return true;
            }

            return false;
        }

        private bool IsValidSlot(int index) =>
            _boardSlots != null &&
            index >= 0 &&
            index < _boardSlots.Length &&
            _boardSlots[index] != null;

        private void StopAllAnimations()
        {
            for (int i = 0; i < _animations.Count; i++)
            {
                if (_animations[i] != null)
                    StopCoroutine(_animations[i]);
            }

            _animations.Clear();
        }

        private static bool InvokeNoArg(
            object target,
            params string[] methodNames)
        {
            if (target == null)
                return false;

            Type type = target.GetType();

            foreach (string name in methodNames)
            {
                MethodInfo method = type.GetMethod(
                    name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic,
                    null,
                    Type.EmptyTypes,
                    null);

                if (method == null)
                    continue;

                method.Invoke(target, null);
                return true;
            }

            return false;
        }

        private static IReadOnlyList<CardViewData> ReadCardList(
            object source,
            params string[] names)
        {
            object value = ReadMember(source, names);

            if (value is IReadOnlyList<CardViewData> readOnly)
                return readOnly;

            if (value is IEnumerable<CardViewData> enumerable)
                return enumerable.ToList();

            return null;
        }

        private static void ClearContainer(Transform container)
        {
            if (container == null)
                return;

            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        private static object ReadMember(object source, params string[] names)
        {
            if (source == null || names == null)
                return null;

            Type type = source.GetType();

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];

                PropertyInfo property = type.GetProperty(
                    name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (property != null)
                    return property.GetValue(source);

                FieldInfo field = type.GetField(
                    name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (field != null)
                    return field.GetValue(source);
            }

            return null;
        }

        private static int ToInt(object value)
        {
            if (value == null)
                return 0;

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        public bool BeginNewMatch(
    int playerCount,
    int aiDifficulty)
        {
            if (_viewModel == null)
            {
                Debug.LogError(
                    "[GameUi] GameViewModel is NULL.");

                return false;
            }

            if (playerCount < 2 ||
                playerCount > 5)
            {
                Debug.LogError(
                    $"[GameUi] Invalid player count: {playerCount}");

                return false;
            }

            if (aiDifficulty < 0 ||
                aiDifficulty > 2)
            {
                Debug.LogError(
                    $"[GameUi] Invalid AI difficulty: {aiDifficulty}");

                return false;
            }

            Debug.Log(
                $"[GameUi] BeginNewMatch: " +
                $"players={playerCount}, " +
                $"difficulty={aiDifficulty}");

            _viewModel.StartMatch(
                playerCount,
                aiDifficulty);

            return true;
        }


        public IReadOnlyList<PlayerTrait> TraitOptions
        {
            get
            {
                return _viewModel != null
                    ? _viewModel.TraitOptions
                    : Array.Empty<PlayerTrait>();
            }
        }


        public bool SelectTrait(
            PlayerTrait trait)
        {
            if (_viewModel == null)
                return false;

            return _viewModel.SelectTrait(trait);
        }
        public void ContinueAfterTraitSelection()
        {
            if (_viewModel == null)
            {
                Debug.LogError(
                    "[GameUi] GameViewModel is NULL.");

                return;
            }

            _viewModel.ContinueAfterTraitSelection();
        }
    }
}
