using RTLTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DeadManDraws
{
    public class GameUIController : MonoBehaviour
    {
        [Header("Header")]
        public Button SceneBackButton;
        public RTLTextMeshPro NotificationText;
        public RTLTextMeshPro txtScore;

        [Header("Players")]
        public Transform PlayerArea;
        public GameObject PlayerPanelPrefab;
        public float PlayerPanelSpacing = 250f;
        public float PlayerPanelAnimDuration = 0.4f;

        [Header("Board")]
        public Transform BoardPanel;
        public List<Image> Slots = new List<Image>(12);

        [Header("Decks")]
        public Button DrawDeckButton;
        public RTLTextMeshPro DrawDeckCountText;
        public Image DrawDeckImage;
        public Image BurnDeckImage;
        public RTLTextMeshPro BurnDeckCountText;

        [Header("Trait")]
        public Button TraitButton;
        public Image TraitImage;

        [Header("Collect")]
        public Button CollectButton;

        [Header("Bank")]
        public List<Image> BankSlots = new List<Image>(10);
        public List<RTLTextMeshPro> BankCountTexts = new List<RTLTextMeshPro>(10);

        [Header("Start Panel")]
        public GameObject StartPanel;
        public Button NewGame_Button;
        public Button Continue_Button;
        public Button Help_Button;
        public Button GroupGame_Button;
        public Button StartPanel_BackButton;

        [Header("New Game Panel")]
        public GameObject NewGamePanel;
        public Slider PlayerNumSlider;
        public RTLTextMeshPro lblNumPlayer;
        public Slider HardnessSlider;
        public RTLTextMeshPro lblHardness;
        public Button Start_Button;
        public Button NewGamePanelBackButton;
        public Slider SpeedSlider;              // ← جدید
        public RTLTextMeshPro lblSpeed;                   // ← جدید

        [Header("Select Trait Panel")]
        public GameObject SelectTraitPanel;
        public Button Trait1_Button;
        public Button Trait2_Button;
        public Image Trait1_Image;
        public Image Trait2_Image;
        public Button Trait1Help_Button;
        public Button Trait2Help_Button;
        public RTLTextMeshPro Trait1Name;
        public RTLTextMeshPro Trait2Name;

        [Header("Trait Help Panel")]
        public GameObject TraitHelpPanel;
        public RTLTextMeshPro TraitHelpText;
        public Button TraitHelpPanel_BackButton;

        [Header("Help / Group Panels")]
        public GameObject HelpPanel;
        public Button HelpPanelBackButton;
        public GameObject GroupGamePanel;
        public Button GroupGameBackButton;

        [Header("Map /  Hook / Oracle")]
        public GameObject MapChoicePanel;
        public Transform MapChoiceContainer;
        public CardWidget MapChoicePrefab;

        public GameObject HookSelectPanel;
        public Transform HookContainer;
        public Button HookCardPrefab;

        public GameObject OraclePanel;
        //public RTLTextMeshPro OracleText;
        public Button OracleBackButton;
        public List<Image> OracleCards = new List<Image>(3);

        [Header("Game Over")]
        public GameObject GameOverPanel;
        public Image PlayerImage;                        // آواتار برنده
        public GameObject PlayerName;                    // Text یا RTLTextMeshPro
        public GameObject PlayerScore;                   // Text یا RTLTextMeshPro
        public Transform PlayerAreaEndGame;              // محل نمایش بقیه بازیکن‌ها
        public GameObject EndGamePlayerPanelPrefab;      // Prefab پنل بازیکن (مثل PlayerPanelPrefab)
        public Button EndGamePopup_CloseButton;

        [Header("Target Select Panel")]
        public GameObject TargetSelectPanel;
        public GameObject TargetPlayerStage;      // زیرپنل انتخاب بازیکن
        public GameObject TargetCardStage;        // زیرپنل انتخاب کارت
        public RTLTextMeshPro TargetTitleText;              // (اختیاری) عنوان
        public Transform TargetContainer;         // محل دکمه‌های بازیکن
        public Button TargetPlayerButtonPrefab;
        public Transform TargetCardContainer;     // محل کارت‌های بازیکن انتخاب‌شده
        public Button TargetCardPrefab;           // یک Button + Image
        public Button TargetCancelButton;         // دکمه انصراف
        public Button TargetConfirmButton;


        [Header("Avatar Selection")]
        public GameObject AvatarSelectPanel;
        public Transform AvatarGridContainer;
        public AvatarSelectionWidget AvatarItemPrefab;
        public Button AvatarConfirmButton;
        public Button AvatarCancelButton;
        public Image AvatarPreviewImage;           // تصویر بزرگ آواتار انتخاب‌شده
        public RTLTextMeshPro AvatarPreviewNameText;
        public RTLTextMeshPro AvatarPreviewDescText;

        [Header("Avatar Pool")]
        public List<AvatarDefinition> AvatarPool = new List<AvatarDefinition>();
        // runtime
        private PlayerData _selectedTarget;
        private GameFlowController _flow;
        private GameEngine _engine;
        private UiAnimation _anim;
        private UiAudio _audio;

        private readonly List<RectTransform> _playerPanelRTs = new List<RectTransform>();
        private readonly List<GameObject> _endGamePanels = new List<GameObject>();
        private readonly List<float> _targetPanelY = new List<float>();
        private Coroutine _panelAnimCoroutine;
        private readonly List<GameObject> _playerPanels = new List<GameObject>();
        private List<TraitDefinition> _currentTraitChoices;
        private PlayerData _pendingTarget;
        private bool _wired;
        private AvatarDefinition _pendingAvatar;
        private System.Action<AvatarDefinition> _avatarOnSelected;
        // ==================================================================
        public void BindFlow(GameFlowController flow,GameEngine engine,UiAnimation anim,UiAudio audio)
        {
            _flow = flow;
            _engine = engine;
            _anim = anim;
            _audio = audio;

            if (_anim != null) _anim.Controller = this;

            WireButtons();
            HideAllPanels();
            
            if (StartPanel) StartPanel.SetActive(true);
        }

        // ==================================================================
        // Buttons
        // ==================================================================

        private void WireButtons()
        {
            if (_wired) return;
            _wired = true;
            //Debug.Log("TraitHelpText00");
            Add(SceneBackButton, () => { Click(); On(StartPanel, true); });
            Add(DrawDeckButton, () => { Click(); _flow.PlayerDrawCard(); });
            Add(CollectButton, () => { Click(); _flow.PlayerCollect(); });
            Add(TraitButton, () => {
                Click();
                //Debug.Log("TraitHelpText0");
                ShowTraitHelp(_engine.CurrentPlayer); 
            });

            Add(NewGame_Button, () =>
            {
                Click(); 
                On(StartPanel, false); 
                On(NewGamePanel, true); 
                InitNewGameSliders();
            });
            Add(Continue_Button, () => { Click(); On(StartPanel, false); });
            Add(Help_Button, () => { Click(); On(HelpPanel, true); });
            Add(GroupGame_Button, () => { Click(); On(GroupGamePanel, true); });
            Add(StartPanel_BackButton, () => { Click(); /* مسیر سین اصلی را اینجا ست کن */ });

            Add(Start_Button, () =>
            {
                Click();
                int n = PlayerNumSlider ? Mathf.RoundToInt(PlayerNumSlider.value) : 2;
                int h = HardnessSlider ? Mathf.RoundToInt(HardnessSlider.value) : 2;
                float s = SpeedSlider ? SpeedSlider.value : 1f;
                On(NewGamePanel, false);
                _flow.StartNewGame(n, h,s);
            });
            Add(NewGamePanelBackButton, () => { Click(); On(NewGamePanel, false); On(StartPanel, true); });

            if (PlayerNumSlider)
            {
                PlayerNumSlider.wholeNumbers = true;
                PlayerNumSlider.onValueChanged.AddListener(v =>
                {
                    if (lblNumPlayer) lblNumPlayer.text = Mathf.RoundToInt(v).ToString();
                });
            }
            if (HardnessSlider)
            {
                HardnessSlider.wholeNumbers = true;
                HardnessSlider.onValueChanged.AddListener(v =>
                {
                    if (lblHardness) lblHardness.text = HardnessWord(Mathf.RoundToInt(v));
                });
            }

            Add(Trait1_Button, () => ChooseTrait(0));
            Add(Trait2_Button, () => ChooseTrait(1));
            Add(Trait1Help_Button, () => { Click(); ShowTraitChoiceHelp(0); });
            Add(Trait2Help_Button, () => { Click(); ShowTraitChoiceHelp(1); });

            Add(TraitHelpPanel_BackButton, () => { Click(); On(TraitHelpPanel, false); });
            Add(HelpPanelBackButton, () => { Click(); On(HelpPanel, false); });
            Add(GroupGameBackButton, () => { Click(); On(GroupGamePanel, false); });

            Add(OracleBackButton, () =>
            {
                Click();
                On(OraclePanel, false);
                _engine.CloseOracle();
                _flow.SetState(GameState.Playing);
            });

            Add(TargetCancelButton, () =>
            {
                Click();
                On(TargetSelectPanel, false);
                _selectedTarget = null;
                if (_engine.AwaitingCannonTarget) _engine.ResolveCannonCard(null, null);
                else if (_engine.AwaitingSwordTarget) _engine.ResolveSwordCard(null, null);
                _flow.ContinueAfterResolution();
            });
            
            Add(AvatarConfirmButton, ConfirmAvatar);
            Add(AvatarCancelButton, CancelAvatar);
            if (SpeedSlider)
            {
                SpeedSlider.onValueChanged.AddListener(v =>
                {
                    if (lblSpeed) lblSpeed.text = v.ToString("0.##") + "x";
                });
            }
            Add(EndGamePopup_CloseButton, () =>
            {
                Click();
                CloseGameOver();
            });
        }

        private void Add(Button b, UnityEngine.Events.UnityAction a)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(a);
        }

        private void Click() { if (_audio) _audio.PlayClick(); }
        private void On(GameObject go, bool v) { if (go) go.SetActive(v); }

        private string HardnessWord(int v)
        {
            switch (v) { case 1: return "Easy"; case 2: return "Normal"; case 3: return "Hard"; }
            return v.ToString();
        }

        // ==================================================================
        // Panels
        // ==================================================================
        private void HideAllPanels()
        {
            On(StartPanel, false);
            On(NewGamePanel, false);
            On(SelectTraitPanel, false);
            On(TraitHelpPanel, false);
            On(HelpPanel, false);
            On(GroupGamePanel, false);
            On(MapChoicePanel, false);
            On(TargetSelectPanel, false);
            On(HookSelectPanel, false);
            On(OraclePanel, false);
            On(GameOverPanel, false);
            On(AvatarSelectPanel, false);
        }

        private void InitNewGameSliders()
        {
            if (PlayerNumSlider)
            {
                PlayerNumSlider.minValue = 2;
                PlayerNumSlider.maxValue = 8;
                PlayerNumSlider.wholeNumbers = true;
                PlayerNumSlider.value = 2;
                if (lblNumPlayer) lblNumPlayer.text = "2";
            }
            if (HardnessSlider)
            {
                HardnessSlider.minValue = 1;
                HardnessSlider.maxValue = 3;
                HardnessSlider.wholeNumbers = true;
                HardnessSlider.value = 2;
                if (lblHardness) lblHardness.text = "Normal";
            }
            if (SpeedSlider)
            {
                SpeedSlider.minValue = 0.25f;
                SpeedSlider.maxValue = 4f;
                SpeedSlider.wholeNumbers = false;
                SpeedSlider.value = 1f;
                if (lblSpeed) lblSpeed.text = "1.0x";
            }
        }

        public void ShowSelectTraitPanel(List<TraitDefinition> traits)
        {
            ClearPlayerPanels();
            _currentTraitChoices = traits;
            if (traits == null || traits.Count < 2) return;
            if (Trait1_Image) Trait1_Image.sprite = traits[0].Image;
            if (Trait2_Image) Trait2_Image.sprite = traits[1].Image;
            if (Trait1Name) Trait1Name.text = traits[0].Name;
            if (Trait2Name) Trait2Name.text = traits[1].Name;
            On(SelectTraitPanel, true);
        }

        public void HideSelectTraitPanel() { On(SelectTraitPanel, false); }

        private void ChooseTrait(int i) { 
            Click();
            if (_audio) _audio.PlayTrait();
            _flow.ConfirmTraitChoice(i); 
        }

        private void ShowTraitChoiceHelp(int i)
        {
            if (_currentTraitChoices == null) return;
            if (TraitHelpText) TraitHelpText.text = _currentTraitChoices[i].Description;
            On(TraitHelpPanel, true);
        }

        private void ShowTraitHelp(PlayerData p)
        {
            //Debug.Log("TraitHelpText");
            if (p == null || _flow == null) return;
            //Debug.Log("TraitHelpText2");
            var def = _flow.GetTraitDefinition(p.Trait);
            //Debug.Log("TraitHelpText3");
            if (TraitHelpText)
                TraitHelpText.text = def != null ? def.Description : p.Trait.ToString();
            //Debug.Log("TraitHelpText4");
            On(TraitHelpPanel, true);
        }

        public void SetTraitButtonImage(Sprite s) { if (TraitImage) TraitImage.sprite = s; }
        private void SetText(Transform parent, string childName, string value)
        {
            var child = FindChild(parent, childName);
            if (child == null) return;

            // اول RTLTextMeshPro رو چک کن
            var rtl = child.GetComponent<RTLTextMeshPro>();
            if (rtl != null) { rtl.text = value; return; }

            // بعد Text معمولی
            var txt = child.GetComponent<Text>();
            if (txt != null) { txt.text = value; return; }
        }

        // ==================================================================
        // Refresh
        // ==================================================================
        public void RefreshAll(GameEngine engine)
        {
            if (engine == null || engine.Data == null || engine.Data.Players == null || engine.Data.Players.Count == 0) return;
            var cur = engine.CurrentPlayer;
            if (cur == null) return;

            if (Slots != null)
            {
                for (int i = 0; i < Slots.Count; i++)
                {
                    if (Slots[i] == null) continue;
                    if (i < cur.Board.Count)
                    {
                        Slots[i].enabled = true;
                        Slots[i].sprite = cur.Board[i].Image;
                    }
                    else Slots[i].enabled = false;
                }
            }

            if (DrawDeckCountText) DrawDeckCountText.text = engine.Data.DrawDeck.Count.ToString();
            if (BurnDeckCountText) BurnDeckCountText.text = engine.Data.BurnDeck.Count.ToString();
            if (txtScore) txtScore.text = cur.ComputeScore().ToString();

            RefreshPlayers(engine);
            RefreshBank(engine);
            // دکمه Collect فقط وقتی فعاله که بازیکن خودی حداقل یک کارت کشیده باشه
            if (CollectButton != null)
                CollectButton.interactable = cur.IsHuman && engine.DrawnThisTurn >= 1;
        }

        private void RefreshPlayers(GameEngine engine)
        {
            if (PlayerPanelPrefab == null || PlayerArea == null) return;

            // ساخت پنل‌های جدید در صورت نیاز
            while (_playerPanels.Count < engine.Data.Players.Count)
            {
                var go = Instantiate(PlayerPanelPrefab, PlayerArea);
                var rt = go.GetComponent<RectTransform>();
                _playerPanels.Add(go);
                _playerPanelRTs.Add(rt);
                _targetPanelY.Add(0f);
            }

            // به‌روزرسانی متن‌ها و حالت
            for (int i = 0; i < _playerPanels.Count; i++)
            {
                if (i >= engine.Data.Players.Count) { _playerPanels[i].SetActive(false); continue; }
                var p = engine.Data.Players[i];
                var go = _playerPanels[i];
                go.SetActive(true);

                var nameT = FindChild(go.transform, "txtPlayerName")?.GetComponent<RTLTextMeshPro>();
                var scoreT = FindChild(go.transform, "txtPlayerScore")?.GetComponent<RTLTextMeshPro>();
                var widget = go.GetComponent<PlayerPanelWidget>();
                if (widget == null) widget = go.GetComponentInChildren<PlayerPanelWidget>(true);

                if (nameT) nameT.text = p.PlayerName;
                if (scoreT) scoreT.text = p.ComputeScore().ToString();
                if (widget) widget.SetActive(i == engine.Data.CurrentPlayerIndex);

                // آواتار
                var avatarChild = FindChild(go.transform, "Avatar");
                if (avatarChild != null)
                {
                    var avatarImg = avatarChild.GetComponent<Image>();
                    if (avatarImg != null)
                    {
                        var def = GetAvatarById(p.AvatarIndex);
                        if (def != null && def.Image != null)
                        {
                            avatarImg.enabled = true;
                            avatarImg.sprite = def.Image;
                        }
                        else
                        {
                            avatarImg.enabled = false;
                            avatarImg.sprite = null;
                        }
                    }
                }
            }

            // مرتب‌سازی نمایش بر اساس امتیاز (بالاترین بالاتر)
            // در صورت تساوی: کارت بیشتر → بالاتر
            var order = new List<int>();
            for (int i = 0; i < engine.Data.Players.Count; i++) order.Add(i);
            order.Sort((a, b) =>
            {
                int sa = engine.Data.Players[a].ComputeScore();
                int sb = engine.Data.Players[b].ComputeScore();
                if (sa != sb) return sb.CompareTo(sa);

                int ca = engine.Data.Players[a].Bank.Count;
                int cb = engine.Data.Players[b].Bank.Count;
                if (ca != cb) return cb.CompareTo(ca);

                return a.CompareTo(b); // پایدار
            });

            // محاسبه موقعیت هدف هر پنل
            bool changed = false;
            for (int rank = 0; rank < order.Count; rank++)
            {
                int playerId = order[rank];
                float newY = -rank * PlayerPanelSpacing;
                if (Mathf.Abs(_targetPanelY[playerId] - newY) > 0.5f) changed = true;
                _targetPanelY[playerId] = newY;
            }

            if (changed)
            {
                if (_panelAnimCoroutine != null) StopCoroutine(_panelAnimCoroutine);
                _panelAnimCoroutine = StartCoroutine(AnimatePanelsRoutine());
            }
        }

        private void RefreshBank(GameEngine engine)
        {
            if (BankSlots == null || BankSlots.Count == 0) return;
            if (engine == null || engine.Data == null || engine.Data.Players == null || engine.Data.Players.Count == 0) return;

            var p = engine.Data.Players[0];
            if (p == null) return;

            var counts = new Dictionary<CardType, int>();
            var bestByType = new Dictionary<CardType, CardDefinition>();
            foreach (var c in p.Bank)
            {
                counts.TryGetValue(c.CardType, out var n);
                counts[c.CardType] = n + 1;
                if (!bestByType.TryGetValue(c.CardType, out var cur) || c.Value > cur.Value)
                    bestByType[c.CardType] = c;
            }

            int slot = 0;
            foreach (CardType type in Enum.GetValues(typeof(CardType)))
            {
                if (slot >= BankSlots.Count) break;

                var img = BankSlots[slot];
                if (img != null)
                {
                    if (counts.TryGetValue(type, out var count) && count > 0)
                    {
                        img.enabled = true;
                        img.sprite = bestByType[type].Image;
                    }
                    else
                    {
                        img.enabled = false;
                        img.sprite = null;
                    }
                }

                if (slot < BankCountTexts.Count && BankCountTexts[slot] != null)
                    BankCountTexts[slot].text = counts.TryGetValue(type, out var c2) ? c2.ToString() : "0";

                slot++;
            }
        }

        public void HighlightTurn(int playerId)
        {
            for (int i = 0; i < _playerPanels.Count; i++)
            {
                var widget = _playerPanels[i].GetComponent<PlayerPanelWidget>();
                if (widget == null) widget = _playerPanels[i].GetComponentInChildren<PlayerPanelWidget>(true);
                if (widget) widget.SetActive(i == playerId);
            }
        }

        public void ShowNotification(string msg)
        {
            if (NotificationText == null) return;
            NotificationText.text = msg;
            StopCoroutine(nameof(ClearNotificationRoutine));
            StartCoroutine(nameof(ClearNotificationRoutine));
        }

        private IEnumerator ClearNotificationRoutine()
        {
            yield return new WaitForSeconds(3f);
            if (NotificationText) NotificationText.text = "";
        }

        private IEnumerator AnimatePanelsRoutine()
        {
            int n = _playerPanelRTs.Count;
            var startY = new List<float>(n);
            for (int i = 0; i < n; i++)
                startY.Add(_playerPanelRTs[i] != null ? _playerPanelRTs[i].anchoredPosition.y : 0f);

            float t = 0f;
            while (t < PlayerPanelAnimDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / PlayerPanelAnimDuration);
                float eased = Mathf.SmoothStep(0f, 1f, k);

                for (int i = 0; i < n; i++)
                {
                    var rt = _playerPanelRTs[i];
                    if (rt == null) continue;
                    var pos = rt.anchoredPosition;
                    pos.y = Mathf.Lerp(startY[i], _targetPanelY[i], eased);
                    rt.anchoredPosition = pos;
                }
                yield return null;
            }

            // Snap نهایی
            for (int i = 0; i < n; i++)
            {
                var rt = _playerPanelRTs[i];
                if (rt == null) continue;
                var pos = rt.anchoredPosition;
                pos.y = _targetPanelY[i];
                rt.anchoredPosition = pos;
            }

            _panelAnimCoroutine = null;
        }

        public void ClearPlayerPanels()
        {
            if (_panelAnimCoroutine != null) { StopCoroutine(_panelAnimCoroutine); _panelAnimCoroutine = null; }
            foreach (var go in _playerPanels) if (go) Destroy(go);
            _playerPanels.Clear();
            _playerPanelRTs.Clear();
            _targetPanelY.Clear();
        }

        // ==================================================================
        // Target panels
        // ==================================================================
        public void ShowCannonTargetPanel()
        {
            _selectedTarget = null;
            ShowTargetPanel("یک بازیکن را انتخاب کنید");
            BuildTargetPlayerButtons();
        }
//var txt = b.GetComponentInChildren<RTLTextMeshPro>();
        public void ShowSwordTargetPanel()
        {
            _selectedTarget = null;
            ShowTargetPanel("یک بازیکن را انتخاب کنید");
            BuildTargetPlayerButtons();
        }
        private void BuildTargetPlayerButtons()
        {
            if (TargetContainer == null || TargetPlayerButtonPrefab == null) return;

            foreach (Transform t in TargetContainer) Destroy(t.gameObject);

            foreach (var p in _engine.Data.Players)
            {
                if (p == _engine.CurrentPlayer) continue;
                if (p.Bank.Count == 0) continue;

                var b = Instantiate(TargetPlayerButtonPrefab, TargetContainer);

                // آواتار
                var avatarChild = FindChild(b.transform, "Avatar");
                if (avatarChild != null)
                {
                    var avatarImg = avatarChild.GetComponent<Image>();
                    if (avatarImg != null)
                    {
                        var def = GetAvatarById(p.AvatarIndex);
                        if (def != null && def.Image != null)
                        {
                            avatarImg.enabled = true;
                            avatarImg.sprite = def.Image;
                        }
                        else
                        {
                            avatarImg.enabled = false;
                            avatarImg.sprite = null;
                        }
                    }
                }

                // اسم
                var nameChild = FindChild(b.transform, "txtPlayerName");
                if (nameChild != null) SetTextOn(nameChild.gameObject, p.PlayerName);

                // امتیاز
                var scoreChild = FindChild(b.transform, "txtPlayerScore");
                if (scoreChild != null) SetTextOn(scoreChild.gameObject, p.ComputeScore().ToString());

                // کلیک
                var pid = p.PlayerId;
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() =>
                {
                    Click();
                    _selectedTarget = _engine.Data.Players[pid];
                    BuildTargetCardButtons(_selectedTarget);
                });
            }
        }
        private void BuildTargetCardButtons(PlayerData target)
        {
            if (TargetCardContainer == null || TargetCardPrefab == null) return;

            foreach (Transform t in TargetCardContainer) Destroy(t.gameObject);

            //if (TargetPlayerStage) On(TargetPlayerStage, false);
            if (TargetCardStage) On(TargetCardStage, true);
            if (TargetTitleText) TargetTitleText.text = $"کارت‌های {target.PlayerName}";

            foreach (var c in target.Bank)
            {
                var b = Instantiate(TargetCardPrefab, TargetCardContainer);
                var img = b.GetComponent<Image>();
                if (img == null) img = b.GetComponentInChildren<Image>();
                if (img != null) img.sprite = c.Image;

                var card = c;
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() =>
                {
                    Click();
                    On(TargetSelectPanel, false);
                    if (_engine.AwaitingCannonTarget) _engine.ResolveCannonCard(target, card);
                    else if (_engine.AwaitingSwordTarget) _engine.ResolveSwordCard(target, card);
                    _selectedTarget = null;
                    RefreshAll(_engine);
                    _flow.ContinueAfterResolution();
                });
            }
        }
     

        private void ShowTargetPanel(string title)
        {
            On(TargetSelectPanel, true);
            if (TargetPlayerStage) On(TargetPlayerStage, true);
            if (TargetCardStage) On(TargetCardStage, false);
            if (TargetTitleText) TargetTitleText.text = title;
        }

        private void SetTextOn(GameObject go, string value)
        {
            if (go == null) return;
            value = value ?? "";

            var rtl = go.GetComponent<RTLTMPro.RTLTextMeshPro>();
            if (rtl != null) { rtl.text = value; return; }

            var txt = go.GetComponent<Text>();
            if (txt != null) txt.text = value;
        }
        // ==================================================================
        // Map / Hook / Oracle
        // ==================================================================
        public void ShowMapChoicePanel(List<CardDefinition> choices)
        {
            On(MapChoicePanel, true);
            if (MapChoiceContainer == null || MapChoicePrefab == null)
            {
                if (choices != null && choices.Count > 0) _engine.ResolveMapChoice(choices[0]);
                return;
            }
            foreach (Transform t in MapChoiceContainer) Destroy(t.gameObject);

            foreach (var c in choices)
            {
                var w = Instantiate(MapChoicePrefab, MapChoiceContainer);
                w.Bind(c);
                var card = c;
                if (w.Button != null)
                    w.Button.onClick.RemoveAllListeners();
                w.Button.onClick.AddListener(() =>
                    {
                        Click();
                        On(MapChoicePanel, false);
                        _engine.ResolveMapChoice(card);
                        RefreshAll(_engine);
                        _flow.ContinueAfterResolution();
                    });
            }
        }

        public void ShowHookSelectionPanel()
        {
            On(HookSelectPanel, true);

            if (HookContainer == null || HookCardPrefab == null)
            {
                // اگر پنل تنظیم نشده، اولین کارت بانک خودی رو خودکار بازی کن
                var self = _engine.CurrentPlayer;
                var fallback = (self.Bank.Count > 0) ? self.Bank[0] : null;
                _engine.ResolveHookSelection(fallback);
                RefreshAll(_engine);
                _flow.ContinueAfterResolution();
                return;
            }

            foreach (Transform t in HookContainer) Destroy(t.gameObject);

            // فقط کارت‌های بانک بازیکن خودی
            var p = _engine.CurrentPlayer;
            foreach (var c in p.Bank)
            {
                var btn = Instantiate(HookCardPrefab, HookContainer);

                var img = btn.GetComponent<Image>();
                if (img == null) img = btn.GetComponentInChildren<Image>();
                if (img != null) img.sprite = c.Image;

                var card = c;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    Click();
                    On(HookSelectPanel, false);
                    _engine.ResolveHookSelection(card);
                    RefreshAll(_engine);
                    _flow.ContinueAfterResolution();
                });
            }
        }
        public void NotifyBurn()
        {
            Debug.Log("[UI] NotifyBurn called");
            ShowNotification("سوخت!");
            if (_audio) _audio.PlayBurn();
        }
        public void ShowOraclePanel(List<CardDefinition> cards)
        {
            if (cards == null || cards.Count == 0) return;

            On(OraclePanel, true);

            if (OracleCards == null) return;

            for (int i = 0; i < OracleCards.Count; i++)
            {
                var img = OracleCards[i];
                if (img == null) continue;

                if (i < cards.Count)
                {
                    img.enabled = true;
                    img.sprite = cards[i].Image;
                }
                else
                {
                    img.enabled = false;
                    img.sprite = null;
                }
            }
        }
        public RectTransform GetBankSlotForType(CardType type)
        {
            int idx = (int)type;
            if (BankSlots != null && idx >= 0 && idx < BankSlots.Count && BankSlots[idx] != null)
                return BankSlots[idx].rectTransform;
            return null;
        }

        public RectTransform GetPlayerPanelRT(int playerIndex)
        {
            if (_playerPanels == null || playerIndex < 0 || playerIndex >= _playerPanels.Count) return null;
            var go = _playerPanels[playerIndex];
            if (go == null) return null;
            return go.GetComponent<RectTransform>();
        }

        public void AnimateCollect(PlayerData p, Action onDone)
        {
            if (_audio) _audio.PlayCollect();
            if (_anim != null) _anim.AnimateCollect(p, onDone);
            else StartCoroutine(CollectFallback(onDone));
        }

        private IEnumerator CollectFallback(Action onDone)
        {
            yield return new WaitForSeconds(0.5f);
            onDone?.Invoke();
        }

        private IEnumerator CollectRoutine(Action onDone)
        {
            yield return new WaitForSeconds(0.5f);
            onDone?.Invoke();
        }
        public RectTransform GetNextFreeSlot()
        {
            if (_engine == null || Slots == null) return null;
            var p = _engine.CurrentPlayer;
            if (p == null) return null;
            int idx = p.Board.Count;
            if (idx >= Slots.Count || Slots[idx] == null) return null;
            return Slots[idx].rectTransform;
        }

        public Vector3 GetBankTargetPosition()
        {
            if (BankSlots != null && BankSlots.Count > 0 && BankSlots[0] != null)
                return BankSlots[0].rectTransform.position;
            return transform.position;
        }
        public void ShowGameOver(PlayerData winner, GameEngine engine)
        {
            On(GameOverPanel, true);
            if (_audio) _audio.PlayWin();

            // ==================== برنده ====================
            if (PlayerImage)
            {
                var def = GetAvatarById(winner.AvatarIndex);
                PlayerImage.enabled = def != null && def.Image != null;
                PlayerImage.sprite = def != null ? def.Image : null;
            }
            SetTextOn(PlayerName, winner.PlayerName);
            SetTextOn(PlayerScore, winner.ComputeScore().ToString());

            // ==================== بقیه بازیکن‌ها ====================
            BuildEndGamePlayerList(engine, winner);
        }

        private void BuildEndGamePlayerList(GameEngine engine, PlayerData winner)
        {
            if (PlayerAreaEndGame == null || EndGamePlayerPanelPrefab == null) return;

            // پاک کردن قبلی‌ها
            foreach (var go in _endGamePanels) if (go) Destroy(go);
            _endGamePanels.Clear();
            foreach (Transform t in PlayerAreaEndGame) Destroy(t.gameObject);

            // بقیه بازیکن‌ها (به جز برنده) به ترتیب امتیاز نزولی
            var others = new List<PlayerData>();
            foreach (var p in engine.Data.Players)
                if (p != winner) others.Add(p);

            others.Sort((a, b) =>
            {
                int sa = a.ComputeScore();
                int sb = b.ComputeScore();
                if (sa != sb) return sb.CompareTo(sa);

                int ca = a.Bank.Count;
                int cb = b.Bank.Count;
                if (ca != cb) return cb.CompareTo(ca);

                return a.PlayerId.CompareTo(b.PlayerId);
            });

            for (int i = 0; i < others.Count; i++)
            {
                var p = others[i];
                var go = Instantiate(EndGamePlayerPanelPrefab, PlayerAreaEndGame);
                _endGamePanels.Add(go);

                var rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 1f);
                    rt.anchorMax = new Vector2(0.5f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f);
                    rt.anchoredPosition = new Vector2(0f, -i * PlayerPanelSpacing);
                }

                // آواتار
                var avatarChild = FindChild(go.transform, "Avatar");
                if (avatarChild != null)
                {
                    var img = avatarChild.GetComponent<Image>();
                    if (img != null)
                    {
                        var def = GetAvatarById(p.AvatarIndex);
                        if (def != null && def.Image != null)
                        {
                            img.enabled = true;
                            img.sprite = def.Image;
                        }
                        else
                        {
                            img.enabled = false;
                            img.sprite = null;
                        }
                    }
                }

                // اسم
                var nameChild = FindChild(go.transform, "txtPlayerName");
                if (nameChild != null) SetTextOn(nameChild.gameObject, p.PlayerName);

                // امتیاز
                var scoreChild = FindChild(go.transform, "txtPlayerScore");
                if (scoreChild != null) SetTextOn(scoreChild.gameObject, p.ComputeScore().ToString());

                // TurnGlow / PlayerPanelWidget در پایان بازی غیرفعال
                var widget = go.GetComponent<PlayerPanelWidget>();
                if (widget == null) widget = go.GetComponentInChildren<PlayerPanelWidget>(true);
                if (widget != null) widget.SetActive(false);

                var glow = FindChild(go.transform, "TurnGlow");
                if (glow != null) glow.gameObject.SetActive(false);
            }
        }

        private void CloseGameOver()
        {
            foreach (var go in _endGamePanels) if (go) Destroy(go);
            _endGamePanels.Clear();
            if (PlayerAreaEndGame != null)
                foreach (Transform t in PlayerAreaEndGame) Destroy(t.gameObject);

            On(GameOverPanel, false);
            On(StartPanel, true);

            // پاک کردن پنل‌های بازیکن‌های درون‌بازی
            ClearPlayerPanels();

        }
        
        // ==================================================================
        private static Transform FindChild(Transform parent, string name)
        {
            if (parent == null) return null;
            foreach (Transform c in parent)
            {
                if (c.name == name) return c;
                var r = FindChild(c, name);
                if (r != null) return r;
            }
            return null;
        }

        // ==================================================================
        // Avatar
        // ==================================================================

        public void OpenAvatarSelection(AvatarDefinition current, System.Action<AvatarDefinition> onSelected)
        {
            _pendingAvatar = current;
            _avatarOnSelected = onSelected;

            if (AvatarSelectPanel) AvatarSelectPanel.SetActive(true);
            if (AvatarGridContainer == null || AvatarItemPrefab == null) return;

            foreach (Transform t in AvatarGridContainer) Destroy(t.gameObject);

            if (AvatarPool != null)
            {
                foreach (var def in AvatarPool)
                {
                    if (def == null) continue;                     // ← اسکیپ آیتم‌های خالی
                    var w = Instantiate(AvatarItemPrefab, AvatarGridContainer);
                    if (w == null) continue;
                    w.Bind(def, OnAvatarItemClicked);
                }
            }

            UpdateAvatarPreview(current);
        }

        private void OnAvatarItemClicked(AvatarDefinition def)
        {
            Click();
            _pendingAvatar = def;
            UpdateAvatarPreview(def);
        }

        private void UpdateAvatarPreview(AvatarDefinition def)
        {
            if(def != null) { 
            if (AvatarPreviewImage != null)
            {
                AvatarPreviewImage.enabled = def != null && def.Image != null;
                AvatarPreviewImage.sprite = def != null ? def.Image : null;
            }
            AvatarPreviewNameText.text=def.AvatarName;
            AvatarPreviewDescText.text=  def.Description ;
        }}

        private void ConfirmAvatar()
        {
            Click();
            if (_pendingAvatar == null) return;
            var cb = _avatarOnSelected;
            _avatarOnSelected = null;
            if (AvatarSelectPanel) AvatarSelectPanel.SetActive(false);
            cb?.Invoke(_pendingAvatar);
        }

        private void CancelAvatar()
        {
            Click();
            _avatarOnSelected = null;
            if (AvatarSelectPanel) AvatarSelectPanel.SetActive(false);
        }
        public AvatarDefinition GetAvatarById(int avatarId)
        {
            if (AvatarPool == null) return null;
            foreach (var a in AvatarPool)
                if (a != null && a.AvatarId == avatarId) return a;
            return null;
        }
    }
}