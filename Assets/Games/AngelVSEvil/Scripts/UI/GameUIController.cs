using RTLTMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AngelVSZombie
{
    public class GameUIController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject StartPanel;
        public GameObject NewGamePanel;
        public GameObject GroupGamePanel;
        public GameObject HelpPanel;

        [Header("Start Panel Buttons")]
        public Button NewGameButton;
        public Button ContinueButton;
        public Button GroupGameButton;
        public Button HelpButton;
        public Button StartPanelBackButton;

        [Header("New Game Panel")]
        public Slider PlayerNumSlider;
        public RTLTextMeshPro txtNumPlayer;
        public Slider HardnessSlider;
        public Slider SpeedSlider;
        public RTLTextMeshPro lblSpeed;
        public Button Start_Button;
        public Button NewGamePanelBackButton;

        [Header("Group Game Panel")]
        public Button GroupGameBackButton;

        [Header("Help Panel")]
        public Button HelpBackButton;

        [Header("Back Scene Settings")]
        public string BackSceneName = "MainMenu";

        [Header("Gameplay Elements")]
        public GameObject GameplayElements;

        [Header("Game UI Elements")]
        public RTLTextMeshPro DeckCountText;
        public RTLTextMeshPro StatusText;
        public Button EndTurnButton;

        [Header("Player Area (PlayerPanel ها)")]
        public Transform PlayerArea;
        public GameObject PlayerPanelPrefab;
        public Sprite[] Avatars;

        [Header("PlayerBank (دست بازیکن انسانی)")]
        public List<CardWidget> PlayerBankWidgets = new List<CardWidget>();

        [Header("Board & Discard")]
        public Transform BoardArea;
        public GameObject BoardCardPrefab;
        public CardWidget DiscardCardWidget;
        public List<CardWidget> BoardSlots = new List<CardWidget>(); // اسلات‌های تخته (مثلاً 3 عدد)

        [Header("Deck")]
        public Button DeckButton;

        [Header("References")]
        public GameFlowController FlowController;

        private List<PlayerPanel> _playerPanels = new List<PlayerPanel>();

        [Header("PlayerBank")]
        public Transform PlayerBankParent;   // ← آبجکت PlayerBank را اینجا بکشید

        [Header("Oracle Popup (پیش گویی)")]
        public GameObject OraclePopup;
        public List<CardWidget> OracleSlots = new List<CardWidget>(); // دقیقاً 3 اسلات
        public Button CloseOracleButton;

        [Header("Gift Target Popup")]
        public GameObject PlayerDetailsPopup;
        public Transform TargetContainer;

        [Header("Zombie Place (منطقه زامبی)")]
        public List<CardWidget> ZombiePlaceSlots = new List<CardWidget>();

        [Header("Select Dead Player Popup (انتخاب بازیکن مرده)")]
        public GameObject SelectDeadPlayerPopup;
        public Transform DeadPlayerContainer;

        [Header("Tunnel Popup")]
        public GameObject TunnelPopup;
        public CardWidget TunnelSlot_Top;    // اسلات اول (رو)
        public Button TunnelSlot_SecondBtn; // اسلات دوم (پشت)

        [Header("Feed Dead Popup")]
        public GameObject FeedDeadPopup;
        public Transform FeedDeadContainer;

        private void Awake()
        {
            // اگر لیست دستی خالی بود، از فرزندان PlayerBank خودکار پر شود
            if (PlayerBankWidgets.Count == 0 && PlayerBankParent != null)
            {
                PlayerBankWidgets.AddRange(PlayerBankParent.GetComponentsInChildren<CardWidget>(true));
                Debug.Log($"[GameUIController] Auto-populated {PlayerBankWidgets.Count} CardWidgets from PlayerBank.");
            }
        }

        private void Start()
        {
            // ---------- StartPanel ----------
            if (NewGameButton != null) NewGameButton.onClick.AddListener(() => ShowPanel(NewGamePanel));
            if (ContinueButton != null) ContinueButton.onClick.AddListener(OnContinueClicked);
            if (GroupGameButton != null) GroupGameButton.onClick.AddListener(() => ShowPanel(GroupGamePanel));
            if (HelpButton != null) HelpButton.onClick.AddListener(() => ShowPanel(HelpPanel));
            if (StartPanelBackButton != null) StartPanelBackButton.onClick.AddListener(OnBackClicked);

            // ---------- NewGamePanel ----------
            SetupPlayerNumSlider();
            SetupHardnessSlider();
            SetupSpeedSlider();
            if (Start_Button != null) Start_Button.onClick.AddListener(OnNewGameStartClicked);
            if (NewGamePanelBackButton != null) NewGamePanelBackButton.onClick.AddListener(() => ShowPanel(StartPanel));

            // ---------- Other Panels ----------
            if (GroupGameBackButton != null) GroupGameBackButton.onClick.AddListener(() => ShowPanel(StartPanel));
            if (HelpBackButton != null) HelpBackButton.onClick.AddListener(() => ShowPanel(StartPanel));

            // ---------- Gameplay ----------
            if (EndTurnButton != null) EndTurnButton.onClick.AddListener(OnEndTurnClicked);
            if (DeckButton != null) DeckButton.onClick.AddListener(OnDeckClicked);

            foreach (var widget in PlayerBankWidgets)
            {
                if (widget != null)
                {
                    widget.Show(false); // مخفی کردن اولیه (این متد والد یعنی Bank_00 را خاموش می‌کند)

                    // اتصال دکمه‌ی والد به رویداد کلیک
                    if (widget.Button != null)
                    {
                        widget.Button.onClick.RemoveAllListeners(); // جلوگیری از ثبت چندباره
                        widget.Button.onClick.AddListener(() =>
                        {
                            int index = PlayerBankWidgets.IndexOf(widget);
                            if (index != -1) OnBankCardClicked(index);
                        });
                    }
                }
            }

            ClearBoard();
            if (DiscardCardWidget != null) DiscardCardWidget.Bind(null);

            // ---------- Initial ----------
            UpdateContinueButtonState();
            ShowPanel(StartPanel);
            if (GameplayElements != null) GameplayElements.SetActive(false);

            if (CloseOracleButton != null)
            {
                CloseOracleButton.onClick.AddListener(() =>
                {
                    if (FlowController != null) FlowController.ResolveOracleEffect();
                });
            }

            // مخفی کردن اولیه پنل
            if (OraclePopup != null) OraclePopup.SetActive(false);
            if (TunnelPopup != null) TunnelPopup.SetActive(false);
        }

        // ------------------ Slider Setup ------------------

        private void SetupPlayerNumSlider()
        {
            if (PlayerNumSlider == null) return;
            PlayerNumSlider.minValue = 1;
            PlayerNumSlider.maxValue = 5;
            PlayerNumSlider.wholeNumbers = true;
            PlayerNumSlider.value = 2;
            PlayerNumSlider.onValueChanged.AddListener(OnPlayerNumChanged);
            OnPlayerNumChanged(PlayerNumSlider.value);
        }

        private void SetupHardnessSlider()
        {
            if (HardnessSlider == null) return;
            HardnessSlider.minValue = 1;
            HardnessSlider.maxValue = 3;
            HardnessSlider.wholeNumbers = true;
            HardnessSlider.value = 2;
        }

        private void SetupSpeedSlider()
        {
            if (SpeedSlider == null) return;
            SpeedSlider.minValue = 1;
            SpeedSlider.maxValue = 5;
            SpeedSlider.wholeNumbers = true;
            SpeedSlider.value = 3;
            SpeedSlider.onValueChanged.AddListener(OnSpeedChanged);
            OnSpeedChanged(SpeedSlider.value);
        }

        private void OnPlayerNumChanged(float value)
        {
            if (txtNumPlayer != null) txtNumPlayer.text = value.ToString();
        }

        private void OnSpeedChanged(float value)
        {
            if (lblSpeed != null) lblSpeed.text = $"سرعت: {value}";
        }

        // ------------------ Panel Management ------------------

        private void ShowPanel(GameObject panelToShow)
        {
            if (StartPanel != null) StartPanel.SetActive(false);
            if (NewGamePanel != null) NewGamePanel.SetActive(false);
            if (GroupGamePanel != null) GroupGamePanel.SetActive(false);
            if (HelpPanel != null) HelpPanel.SetActive(false);
            if (panelToShow != null) panelToShow.SetActive(true);
        }

        private void UpdateContinueButtonState()
        {
            bool hasSave = PlayerPrefs.HasKey("SavedGame");
            if (ContinueButton != null) ContinueButton.interactable = hasSave;
        }

        private void OnContinueClicked()
        {
            if (!PlayerPrefs.HasKey("SavedGame")) return;
            ShowPanel(null);
            if (GameplayElements != null) GameplayElements.SetActive(true);
            FlowController.LoadGame();
        }

        private void OnBackClicked()
        {
            if (!string.IsNullOrEmpty(BackSceneName))
                SceneManager.LoadScene(BackSceneName);
        }

        private void OnNewGameStartClicked()
        {
            int playerCount = Mathf.RoundToInt(PlayerNumSlider != null ? PlayerNumSlider.value : 2);
            int hardness = Mathf.RoundToInt(HardnessSlider != null ? HardnessSlider.value : 2);
            float speed = SpeedSlider != null ? SpeedSlider.value : 3f;

            PlayerPrefs.SetInt("SavedGame", 1);
            PlayerPrefs.Save();

            ShowPanel(null);
            if (GameplayElements != null) GameplayElements.SetActive(true);
            FlowController.StartGame(playerCount, hardness, speed);
        }

        // ------------------ Player Panels ------------------

        public void BuildPlayerPanels(List<PlayerData> players)
        {
            foreach (Transform child in PlayerArea) Destroy(child.gameObject);
            _playerPanels.Clear();

            foreach (var player in players)
            {
                GameObject go = Instantiate(PlayerPanelPrefab, PlayerArea);
                PlayerPanel panel = go.GetComponent<PlayerPanel>();
                if (panel == null) continue;
                panel.Setup(player, Avatars);
                _playerPanels.Add(panel);
            }
        }

        public void RefreshPlayerPanels()
        {
            foreach (var panel in _playerPanels)
                if (panel != null) panel.UpdateVisuals();
        }
        public void UpdateTurnIndicator(int currentPlayerIndex)
        {
            for (int i = 0; i < _playerPanels.Count; i++)
            {
                if (_playerPanels[i] != null)
                {
                    // اگر ایندکس پنل با بازیکن فعلی یکی بود، نشانگر روشن شود
                    _playerPanels[i].SetTurn(i == currentPlayerIndex);
                }
            }
        }

        // ------------------ Gameplay UI Updates ------------------

        public void UpdateDeckCount(int count)
        {
            if (DeckCountText != null) DeckCountText.text = $"دک: {count}";
        }

        public void UpdateStatus(string message)
        {
            if (StatusText != null) StatusText.text = message;
        }

        // ------------------ PlayerBank Render (همیشه دست بازیکن انسانی) ------------------

        public void RenderHand(PlayerData player)
        {
            if (player == null) return;

            bool isHumanTurn = FlowController != null && FlowController.CurrentState == GameState.PlayerTurn;
            bool isDying = FlowController != null && FlowController.IsPlayerDying;

            var groupedCards = System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.Select(
                    System.Linq.Enumerable.GroupBy(player.Hand, c => c.CardType),
                    g => new { CardDef = g.First(), Count = g.Count() }
                )
            );

            for (int i = 0; i < PlayerBankWidgets.Count; i++)
            {
                CardWidget widget = PlayerBankWidgets[i];
                if (widget == null) continue;

                if (i < groupedCards.Count)
                {
                    widget.Show(true);
                    widget.Bind(groupedCards[i].CardDef, groupedCards[i].Count);

                    if (widget.Button != null)
                    {
                        bool canPlay = false;

                        if (isDying)
                        {
                            // اگر در حال مردن هستیم، فقط فرشته قابل کلیک باشد
                            canPlay = (groupedCards[i].CardDef.CardType == CardType.Angel);
                        }
                        else
                        {
                            // حالت عادی
                            bool isImmediate = groupedCards[i].CardDef.Immediate;
                            canPlay = (isHumanTurn && player.IsAlive) || isImmediate;
                        }

                        widget.Button.interactable = canPlay;

                        // لاگ برای دیباگ
                        if (groupedCards[i].CardDef.CardType == CardType.Angel)
                        {
                            Debug.Log($"[RenderHand] کارت فرشته پیدا شد. isDying={isDying}, interactable={canPlay}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[RenderHand] دکمه برای کارت {groupedCards[i].CardDef.CardName} پیدا نشد!");
                    }
                }
                else
                {
                    widget.Show(false);
                    widget.Bind(null);
                }
            }

            if (DeckButton != null)
            {
                DeckButton.interactable = isHumanTurn && player.IsAlive && !isDying;
            }
        }
        public void ClearPlayerBank()
        {
            foreach (var w in PlayerBankWidgets)
            {
                if (w != null)
                {
                    w.Bind(null); // پاک کردن داده‌ها
                    w.gameObject.SetActive(false); // مخفی کردن
                }
            }
        }

        // ------------------ Board & Discard ------------------

        public void ShowCardOnBoard(CardDefinition card)
        {
            // برای شروع، همیشه تخته را پاک می‌کنیم تا فقط یک کارت نمایش داده شود
            ClearBoard();

            if (BoardSlots.Count > 0 && BoardSlots[0] != null)
            {
                BoardSlots[0].Show(true);           // نمایش اسلات (Image والد)
                BoardSlots[0].Bind(card);           // تنظیم تصویر و نام

                // اطمینان از اینکه کارت‌های روی تخته قابل کلیک نیستند
                if (BoardSlots[0].Button != null)
                    BoardSlots[0].Button.interactable = false;
            }
        }
        // نمایش چند کارت روی تخته (برای آینده - اگر بازیکن چند کارت همزمان بازی کرد)
        public void ShowCardsOnBoard(List<CardDefinition> cards)
        {
            ClearBoard();
            for (int i = 0; i < cards.Count && i < BoardSlots.Count; i++)
            {
                if (BoardSlots[i] != null)
                {
                    BoardSlots[i].Show(true);
                    BoardSlots[i].Bind(cards[i]);
                    if (BoardSlots[i].Button != null)
                        BoardSlots[i].Button.interactable = false;
                }
            }
        }
        // پاکسازی تخته
        public void ClearBoard()
        {
            foreach (var slot in BoardSlots)
            {
                if (slot != null)
                {
                    slot.Show(false);   // مخفی کردن والد (Image)
                    slot.Bind(null);    // پاک کردن داده‌های کارت
                }
            }
        }

        public void UpdateDiscardDisplay(CardDefinition topCard)
        {
            if (DiscardCardWidget == null) return;

            if (topCard == null)
            {
                DiscardCardWidget.Show(false);
                DiscardCardWidget.Bind(null);
            }
            else
            {
                DiscardCardWidget.Show(true);
                DiscardCardWidget.Bind(topCard);
                if (DiscardCardWidget.Button != null)
                    DiscardCardWidget.Button.interactable = false; // دک سوخته نباید کلیک شود
            }
        }

        // ------------------ Events ------------------

        private void OnBankCardClicked(int index)
        {
            if (FlowController != null && index >= 0 && index < PlayerBankWidgets.Count)
            {
                CardWidget widget = PlayerBankWidgets[index];

                // بررسی وجود کارت و پراپرتی BoundCard
                if (widget != null && widget.BoundCard != null)
                {
                    FlowController.OnPlayerSelectedBankCard(widget.BoundCard);
                }
            }
        }

        private void OnDeckClicked()
        {
            if (FlowController != null) FlowController.OnEndTurnClicked();
        }

        private void OnEndTurnClicked()
        {
            if (FlowController != null) FlowController.OnEndTurnClicked();
        }

        // ------------------ Oracle Popup ------------------

        public void ShowOraclePopup(List<CardDefinition> top3Cards)
        {
            if (OraclePopup != null) OraclePopup.SetActive(true);

            for (int i = 0; i < OracleSlots.Count; i++)
            {
                if (i < top3Cards.Count && top3Cards[i] != null)
                {
                    OracleSlots[i].Show(true);
                    OracleSlots[i].Bind(top3Cards[i]);
                    // کارت‌های داخل پنل پیش گویی نباید قابل کلیک باشند
                    if (OracleSlots[i].Button != null)
                        OracleSlots[i].Button.interactable = false;
                }
                else
                {
                    OracleSlots[i].Show(false);
                }
            }
        }

        public void HideOraclePopup()
        {
            if (OraclePopup != null) OraclePopup.SetActive(false);

            // پاکسازی اسلات‌ها
            foreach (var slot in OracleSlots)
            {
                if (slot != null) slot.Show(false);
            }
        }

        // متدهای جدید برای مدیریت پاپ‌آپ هدیه:
        public void ShowGiftTargetPopup(List<PlayerData> aliveTargets)
        {
            if (PlayerDetailsPopup != null) PlayerDetailsPopup.SetActive(true);

            // پاک کردن پنل‌های قبلی
            foreach (Transform child in TargetContainer)
                Destroy(child.gameObject);

            // ساخت پنل برای هر بازیکن زنده
            foreach (var target in aliveTargets)
            {
                GameObject go = Instantiate(PlayerPanelPrefab, TargetContainer);
                PlayerPanel panel = go.GetComponent<PlayerPanel>();
                if (panel != null)
                {
                    panel.Setup(target, Avatars);
                    // فعال کردن قابلیت کلیک
                    panel.SetAsTarget((clickedTarget) =>
                    {
                        FlowController.ResolveGiftSelection(clickedTarget);
                    });
                }
            }
        }

        public void HideGiftTargetPopup()
        {
            if (PlayerDetailsPopup != null) PlayerDetailsPopup.SetActive(false);

            // پاکسازی
            foreach (Transform child in TargetContainer)
                Destroy(child.gameObject);
        }

        // ------------------ Zombie Place ------------------
        public void ShowZombiePlace(CardDefinition zombieCard)
        {
            if (ZombiePlaceSlots.Count > 0 && ZombiePlaceSlots[0] != null)
            {
                ZombiePlaceSlots[0].Show(true);
                ZombiePlaceSlots[0].Bind(zombieCard);
                if (ZombiePlaceSlots[0].Button != null)
                    ZombiePlaceSlots[0].Button.interactable = false;
            }
        }

        public void ClearZombiePlace()
        {
            foreach (var slot in ZombiePlaceSlots)
            {
                if (slot != null) slot.Show(false);
            }
        }

        // ------------------ Select Dead Player ------------------
        public void ShowSelectDeadPlayerPopup(List<PlayerData> deadPlayers)
        {
            if (SelectDeadPlayerPopup != null) SelectDeadPlayerPopup.SetActive(true);

            foreach (Transform child in DeadPlayerContainer)
                Destroy(child.gameObject);

            foreach (var target in deadPlayers)
            {
                GameObject go = Instantiate(PlayerPanelPrefab, DeadPlayerContainer);
                PlayerPanel panel = go.GetComponent<PlayerPanel>();
                if (panel != null)
                {
                    panel.Setup(target, Avatars);
                    panel.SetAsTarget((clickedTarget) =>
                    {
                        if (FlowController != null) FlowController.ResolveSelectDeadPlayer(clickedTarget);
                    });
                }
            }
        }

        public void HideSelectDeadPlayerPopup()
        {
            if (SelectDeadPlayerPopup != null) SelectDeadPlayerPopup.SetActive(false);
            foreach (Transform child in DeadPlayerContainer)
                Destroy(child.gameObject);
        }

        // متدهای جدید برای مدیریت پنل تونل:
        public void ShowTunnelPopup(CardDefinition topCard, CardDefinition secondCard)
        {
            if (TunnelPopup != null) TunnelPopup.SetActive(true);

            // --- اسلات اول (رو) ---
            if (TunnelSlot_Top != null)
            {
                TunnelSlot_Top.Show(true);
                TunnelSlot_Top.Bind(topCard);
                if (TunnelSlot_Top.Button != null)
                {
                    TunnelSlot_Top.Button.onClick.RemoveAllListeners();
                    TunnelSlot_Top.Button.onClick.AddListener(() => FlowController.ResolveTunnelSelection(true));
                    TunnelSlot_Top.Button.interactable = true;
                }
            }

            // --- اسلات دوم (پشت - عکس ثابت) ---
            if (TunnelSlot_SecondBtn != null)
            {
                TunnelSlot_SecondBtn.gameObject.SetActive(true);
                TunnelSlot_SecondBtn.onClick.RemoveAllListeners();
                // با کلیک روی این دکمه، به کنترلر می‌گوییم کارت دوم انتخاب شده است
                TunnelSlot_SecondBtn.onClick.AddListener(() => FlowController.ResolveTunnelSelection(false));
            }
        }

        public void HideTunnelPopup()
        {
            if (TunnelPopup != null) TunnelPopup.SetActive(false);
            if (TunnelSlot_Top != null) TunnelSlot_Top.Show(false);
            if (TunnelSlot_SecondBtn != null) TunnelSlot_SecondBtn.gameObject.SetActive(false);
        }

        // متدهای جدید برای مدیریت پاپ‌آپ خیرات:
        public void ShowFeedDeadPopup(List<PlayerData> deadPlayers)
        {
            if (FeedDeadPopup != null) FeedDeadPopup.SetActive(true);

            // پاک کردن پنل‌های قبلی
            foreach (Transform child in FeedDeadContainer)
                Destroy(child.gameObject);

            // ساخت پنل برای هر بازیکن مرده
            foreach (var target in deadPlayers)
            {
                GameObject go = Instantiate(PlayerPanelPrefab, FeedDeadContainer);
                PlayerPanel panel = go.GetComponent<PlayerPanel>();
                if (panel != null)
                {
                    panel.Setup(target, Avatars);
                    // فعال کردن قابلیت کلیک
                    panel.SetAsTarget((clickedTarget) =>
                    {
                        if (FlowController != null) FlowController.ResolveFeedDeadSelection(clickedTarget);
                    });
                }
            }
        }

        public void HideFeedDeadPopup()
        {
            if (FeedDeadPopup != null) FeedDeadPopup.SetActive(false);
            foreach (Transform child in FeedDeadContainer)
                Destroy(child.gameObject);
        }
    }
}