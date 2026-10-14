using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeadManDraws
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private GameUIController _ui;
        [SerializeField] private UiAnimation _animation;
        [SerializeField] private UiAudio _audio;

        [Header("Data Pools")]
        [SerializeField] private List<CardDefinition> _cardPool = new List<CardDefinition>();
        [SerializeField] private List<TraitDefinition> _traitPool = new List<TraitDefinition>();

        public GameEngine Engine { get; private set; }
        public GameState State { get; private set; } = GameState.None;

        public event Action<GameState> OnStateChanged;

        [Header("Avatars")]
        [SerializeField] private List<AvatarDefinition> _avatarPool = new List<AvatarDefinition>();
        private AvatarDefinition _humanAvatar;

        [Header("Speed")]
        [Range(0.25f, 4f)]
        public float GameSpeed = 1f;   // 1 = معمولی، بیشتر = سریع‌تر

        private float D(float seconds) => seconds / Mathf.Max(0.01f, GameSpeed);

        private AiPlayer _ai;
        private List<TraitDefinition> _pendingTraits;

        private void Start()
        {
            if (_ui == null) { Debug.LogError("[DeadManDraws] GameUIController not assigned."); return; }

            Engine = new GameEngine();
            Engine.OnNotification += msg => _ui.ShowNotification(msg);
            Engine.OnChanged += () => _ui.RefreshAll(Engine);
            _ai = new AiPlayer(Engine);

            _ui.BindFlow(this, Engine, _animation, _audio);
            SetState(GameState.StartPanel);
        }

        public void SetState(GameState s)
        {
            State = s;
            OnStateChanged?.Invoke(s);
        }

        public TraitDefinition GetTraitDefinition(TraitType t)
        {
            foreach (var d in _traitPool) if (d.TraitType == t) return d;
            return null;
        }

        // ------------------------------------------------------------------
        // New game
        // ------------------------------------------------------------------
        //public void StartNewGame(int playerCount, int hardness)
        //{
        //    if (Engine == null) return;

        //    var players = new List<PlayerData>();
        //    for (int i = 0; i < playerCount; i++)
        //    {
        //        players.Add(new PlayerData
        //        {
        //            PlayerId = i,
        //            PlayerName = i == 0 ? "You" : "AI " + i,
        //            IsHuman = i == 0,
        //            AvatarIndex = i,
        //            Hardness = hardness
        //        });
        //    }

        //    Engine.InitGame(playerCount, hardness, players, _cardPool);

        //    // مرحله انتخاب آواتار
        //    _ui.OpenAvatarSelection(_humanAvatar, OnHumanAvatarChosen);
        //}
        public void StartNewGame(int playerCount, int hardness, float speed)
        {
            if (Engine == null) return;

            GameSpeed = Mathf.Clamp(speed, 0.25f, 4f);

            var players = new List<PlayerData>();
            for (int i = 0; i < playerCount; i++)
            {
                players.Add(new PlayerData
                {
                    PlayerId = i,
                    PlayerName = i == 0 ? "You" : "AI " + i,
                    IsHuman = i == 0,
                    AvatarIndex = i,
                    Hardness = hardness
                });
            }

            Engine.InitGame(playerCount, hardness, players, _cardPool);

            _ui.OpenAvatarSelection(_humanAvatar, OnHumanAvatarChosen);
        }
        private void OnHumanAvatarChosen(AvatarDefinition def)
        {
            
            _humanAvatar = def;

            // ذخیره در PlayerData
            if (def != null)
            {
                var human = Engine.Data.Players[0];
                human.AvatarIndex = def.AvatarId;
                //human.PlayerName = def.AvatarName;   // اگه می‌خوای اسم بازیکن هم آواتار بشه
            }

            // انتخاب آواتار برای AI‌ها
            AssignAiAvatars();

            // ادامه: انتخاب trait
            PickTraitChoices();
            SetState(GameState.SelectTrait);
            _ui.ShowSelectTraitPanel(_pendingTraits);
        }

        private void AssignAiAvatars()
        {
            if (_avatarPool == null || _avatarPool.Count == 0) return;

            // کپی و شافل
            var pool = new List<AvatarDefinition>(_avatarPool);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            // اول آواتار خودی رو از لیست حذف کن
            if (_humanAvatar != null) pool.Remove(_humanAvatar);

            int idx = 0;
            for (int i = 1; i < Engine.Data.Players.Count; i++)
            {
                if (idx >= pool.Count) idx = 0;   // اگه کم آمد دوباره بچرخ
                var p = Engine.Data.Players[i];
                var def = pool[idx % pool.Count];
                p.AvatarIndex = def.AvatarId;
                p.PlayerName = def.AvatarName;    // اسم AI = اسم آواتار
                idx++;
            }
            
        }
        private void PickTraitChoices()
        {
            _pendingTraits = new List<TraitDefinition>();
            if (_traitPool == null || _traitPool.Count == 0) return;

            var copy = new List<TraitDefinition>(_traitPool);
            for (int i = copy.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (copy[i], copy[j]) = (copy[j], copy[i]);
            }
            _pendingTraits.Add(copy[0]);
            _pendingTraits.Add(copy.Count > 1 ? copy[1] : copy[0]);
        }

        public void ConfirmTraitChoice(int index)
        {
            if (_pendingTraits == null || index < 0 || index >= _pendingTraits.Count) return;

            var chosen = _pendingTraits[index];
            Engine.Data.Players[0].Trait = chosen.TraitType;

            // random traits for AI
            var pool = new List<TraitDefinition>(_traitPool);
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            int idx = 0;
            for (int i = 1; i < Engine.Data.Players.Count; i++)
            {
                Engine.Data.Players[i].Trait = pool[idx % pool.Count].TraitType;
                idx++;
            }

            SetupDavyJonesTargets();

            _ui.HideSelectTraitPanel();
            _ui.SetTraitButtonImage(chosen.Image);
            SetState(GameState.Playing);
            _ui.RefreshAll(Engine);
            BeginTurn();
        }

        private void SetupDavyJonesTargets()
        {
            foreach (var p in Engine.Data.Players)
            {
                if (p.Trait != TraitType.DavyJonesLocker) continue;
                foreach (var o in Engine.Data.Players)
                {
                    if (o != p) { o.DavyJonesOwnerId = p.PlayerId; break; }
                }
            }
        }

        // ------------------------------------------------------------------
        // Turn
        // ------------------------------------------------------------------
        public void BeginTurn()
        {
            if (Engine.CheckGameOver()) { EndGame(); return; }

            var p = Engine.CurrentPlayer;
            _ui.HighlightTurn(p.PlayerId);
            _ui.RefreshAll(Engine);
            SetState(GameState.Playing);

            if (!p.IsHuman)
                StartCoroutine(AITurnRoutine(p));
        }

        public void PlayerDrawCard()
        {
            if (State != GameState.Playing && State != GameState.WaitingForInput) return;
            if (Engine.CurrentPlayer == null || !Engine.CurrentPlayer.IsHuman) return;
            if (Engine.AwaitingMapChoice || Engine.AwaitingCannonTarget ||
                Engine.AwaitingHookCard || Engine.AwaitingSwordTarget ||
                Engine.AwaitingOracleDecision) return;

            StartCoroutine(DrawRoutine());
        }

        private IEnumerator DrawRoutine()
        {
            SetState(GameState.ResolvingAbility);

            var c = Engine.DrawCard();
            if (c == null) { EndGame(); yield break; }

            if (_audio) _audio.PlayDraw();
            if (_animation) _animation.AnimateDrawCard(c);
            if (c == null) { EndGame(); yield break; }

            if (_animation) _animation.AnimateDrawCard(c);
            yield return new WaitForSeconds(D(0.35f));

            var res = Engine.PlayCardForCurrentPlayer(c);
            _ui.RefreshAll(Engine);
            yield return new WaitForSeconds(D(0.25f));

            //if (res.Busted)
            //{
            //    // کارت سوختنی همین الان روی BoardPanel نمایش داده شده
            //    _ui.NotifyBurn();
            //    yield return new WaitForSeconds(0.8f);

            //    Engine.ExecutePendingBust();
            //    _ui.RefreshAll(Engine);
            //    yield return new WaitForSeconds(0.6f);

            //    EndTurn();
            //    yield break;
            //}
            if (res.Busted)
            {
                Debug.Log("[Flow] DrawRoutine: bust detected → calling BustRoutine");
                yield return StartCoroutine(BustRoutine());
                yield break;
            }
            if (res.NeedsTarget)
            {
                SetState(GameState.WaitingForInput);
                HandlePendingUI(res);
                yield break;
            }

            if (Engine.AwaitingOracleDecision)
            {
                int n = (Engine.CurrentPlayer.Trait == TraitType.Mystic) ? 3 : 1;
                var peek = Engine.PeekCards(n);

                if (peek.Count > 0)
                {
                    _ui.ShowOraclePanel(peek);
                    SetState(GameState.WaitingForInput);
                    yield break;
                }
                Engine.CloseOracle();
                SetState(GameState.Playing);
            }

            if (Engine.IsDeckEmpty) { EndGame(); yield break; }
            SetState(GameState.Playing);
        }

        private void HandlePendingUI(PlayResult res)
        {
            if (_audio) _audio.PlayAbility();
            if (Engine.AwaitingCannonTarget)
            {
                if (HasAnyTargetWithCards())
                    _ui.ShowCannonTargetPanel();
                else
                {
                    Engine.AwaitingCannonTarget = false;
                    Engine.Notify("توپ: هدفی با کارت وجود ندارد.");
                    SetState(GameState.Playing);
                }
            }
            else if (Engine.AwaitingHookCard)
            {
                if (Engine.CurrentPlayer.Bank.Count > 0)
                    _ui.ShowHookSelectionPanel();
                else
                {
                    Engine.AwaitingHookCard = false;
                    Engine.Notify("قلاب: بانک شما خالی است.");
                    SetState(GameState.Playing);
                }
            }
            else if (Engine.AwaitingMapChoice)
            {
                if (Engine.PendingMapChoices != null && Engine.PendingMapChoices.Count > 0)
                    _ui.ShowMapChoicePanel(Engine.PendingMapChoices);
                else
                {
                    Engine.AwaitingMapChoice = false;
                    Engine.PendingMapChoices.Clear();
                    Engine.Notify("نقشه: دک سوخته خالی است.");
                    SetState(GameState.Playing);
                }
            }
            else if (Engine.AwaitingSwordTarget)
            {
                if (HasAnyTargetWithCards())
                    _ui.ShowSwordTargetPanel();
                else
                {
                    Engine.AwaitingSwordTarget = false;
                    Engine.Notify("شمشیر: هدفی با کارت وجود ندارد.");
                    SetState(GameState.Playing);
                }
            }
        }

        private bool HasAnyTargetWithCards()
        {
            foreach (var p in Engine.Data.Players)
            {
                if (p == Engine.CurrentPlayer) continue;
                if (p.Bank.Count > 0) return true;
            }
            return false;
        }

        public void PlayerCollect()
        {
            if (Engine.CurrentPlayer == null || !Engine.CurrentPlayer.IsHuman) return;
            if (Engine.DrawnThisTurn < 1) return;
            EndTurn();
        }

        private void EndTurn()
        {
            SetState(GameState.TurnEnd);

            var p = Engine.CurrentPlayer;

            // اول انیمیشن جمع‌آوری، بعد انتقال به بانک
            _ui.AnimateCollect(p, () =>
            {
                Engine.OnPlayerCollect();
                Engine.NextTurn();
                if (Engine.CheckGameOver()) { EndGame(); return; }
                BeginTurn();
            });
        }

        // ------------------------------------------------------------------
        // AI
        // ------------------------------------------------------------------
        private IEnumerator AITurnRoutine(PlayerData p)
        {
            yield return new WaitForSeconds(D(0.5f));

            int guard = 0;
            while (guard++ < 64)
            {
                if (Engine.IsDeckEmpty) { EndGame(); yield break; }
                // AI باید حداقل یک کارت در نوبتش بکشه
                bool mustDraw = Engine.DrawnThisTurn < 1;
                if (!mustDraw && !_ai.WantsToDraw(p) && Engine.KrakenForcedDraws == 0) break;

                SetState(GameState.ResolvingAbility);
                var c = Engine.DrawCard();
                if (c == null) { EndGame(); yield break; }

                if (_audio) _audio.PlayDraw();
                if (_animation) _animation.AnimateDrawCard(c);
                if (c == null) { EndGame(); yield break; }

                if (_animation) _animation.AnimateDrawCard(c);
                yield return new WaitForSeconds(D(0.35f));

                var res = Engine.PlayCardForCurrentPlayer(c);
                _ui.RefreshAll(Engine);
                yield return new WaitForSeconds(D(0.25f));

                //if (res.Busted)
                //{
                //    _ui.NotifyBurn();
                //    yield return new WaitForSeconds(0.8f);

                //    Engine.ExecutePendingBust();
                //    _ui.RefreshAll(Engine);
                //    yield return new WaitForSeconds(0.6f);

                //    EndTurn();
                //    yield break;
                //}
                if (res.Busted)
                {
                    Debug.Log("[Flow] AITurnRoutine: bust detected → calling BustRoutine");
                    yield return StartCoroutine(BustRoutine());
                    yield break;
                }
                if (Engine.AwaitingCannonTarget)
                {
                    var t = _ai.PickCannonTarget(c.CardType);
                    if (t != null && t.Bank.Count > 0)
                    {
                        var best = t.Bank[0];
                        foreach (var bc in t.Bank) if (bc.Value > best.Value) best = bc;
                        Engine.ResolveCannonCard(t, best);
                    }
                    else
                    {
                        Engine.ResolveCannonCard(null, null);
                    }
                    yield return new WaitForSeconds(D(0.35f));
                }

                if (Engine.AwaitingHookCard)
                {
                    var h = (p.Bank.Count > 0) ? _ai.PickHookCard() : null;
                    Engine.ResolveHookSelection(h);
                    yield return new WaitForSeconds(D(0.35f));

                    if (Engine.PendingBust)
                    {
                        yield return StartCoroutine(BustRoutine());
                        yield break;
                    }
                }

                if (Engine.AwaitingMapChoice)
                {
                    var m = (Engine.PendingMapChoices != null && Engine.PendingMapChoices.Count > 0)
                        ? _ai.PickMapCard() : null;
                    Engine.ResolveMapChoice(m);
                    yield return new WaitForSeconds(D(0.35f));

                    if (Engine.PendingBust)
                    {
                        yield return StartCoroutine(BustRoutine());
                        yield break;
                    }
                }

                if (Engine.AwaitingSwordTarget)
                {
                    var t = _ai.PickSwordTarget();
                    if (t != null && t.Bank.Count > 0)
                    {
                        var best = t.Bank[0];
                        foreach (var bc in t.Bank) if (bc.Value > best.Value) best = bc;
                        Engine.ResolveSwordCard(t, best);
                    }
                    else
                    {
                        Engine.ResolveSwordCard(null, null);
                    }
                    yield return new WaitForSeconds(D(0.35f));
                }

                if (Engine.AwaitingOracleDecision)
                {
                    int n = (p.Trait == TraitType.Mystic) ? 3 : 1;
                    var peek = Engine.PeekCards(n);
                    bool cont = peek.Count > 0 && _ai.WantsToContinueAfterOracle(p);
                    Engine.CloseOracle();
                    if (!cont) break;
                }

                if (Engine.KrakenForcedDraws > 0)
                {
                    Engine.KrakenForcedDraws--;
                    continue;
                }
            }

            EndTurn();
        }
        public void ContinueAfterResolution()
        {
            if (Engine.PendingBust)
            {
                StartCoroutine(BustRoutine());
                return;
            }

            if (Engine.AwaitingCannonTarget || Engine.AwaitingHookCard ||
                Engine.AwaitingMapChoice || Engine.AwaitingSwordTarget ||
                Engine.AwaitingOracleDecision)
            {
                SetState(GameState.WaitingForInput);
                HandlePendingUI(null);
                return;
            }

            if (Engine.IsDeckEmpty) { EndGame(); return; }
            SetState(GameState.Playing);
        }

        //private IEnumerator BustRoutine()
        //{
        //    _ui.NotifyBurn();

        //    // انیمیشن سوختن: کارت‌ها از تخته به سمت BurnDeck پرواز می‌کنند
        //    if (_animation != null)
        //        _animation.AnimateBurnCards(Engine.CurrentPlayer);

        //    yield return new WaitForSeconds(0.8f);

        //    Engine.ExecutePendingBust();
        //    _ui.RefreshAll(Engine);
        //    yield return new WaitForSeconds(0.5f);

        //    EndTurn();
        //}
        //private IEnumerator BustRoutine()
        //{
        //    Debug.Log("[Flow] BustRoutine START");

        //    if (_ui == null) { Debug.LogError("[Flow] _ui is NULL"); yield break; }
        //    if (_animation == null) { Debug.LogWarning("[Flow] _animation is NULL — only notification will play"); }

        //    _ui.NotifyBurn();

        //    if (_animation != null)
        //    {
        //        Debug.Log("[Flow] Calling AnimateBurnCards");
        //        _animation.AnimateBurnCards(Engine.CurrentPlayer);
        //    }

        //    yield return new WaitForSeconds(0.9f);

        //    Debug.Log("[Flow] Executing pending bust");
        //    Engine.ExecutePendingBust();
        //    _ui.RefreshAll(Engine);
        //    yield return new WaitForSeconds(0.5f);

        //    EndTurn();
        //}
        private IEnumerator BustRoutine()
        {
            if (_ui == null) { yield break; }

            _ui.NotifyBurn();
            if (_audio) _audio.PlayBurn();

            if (_animation != null)
                _animation.AnimateBurnCards(Engine.CurrentPlayer);

            yield return new WaitForSeconds(D(0.9f));

            Engine.ExecutePendingBust();
            _ui.RefreshAll(Engine);
            yield return new WaitForSeconds(D(0.5f));

            EndTurn();
        }
        private CardType PickBurnType(PlayerData t)
        {
            if (t.Bank.Count == 0) return CardType.Anchor;
            var best = t.Bank[0];
            foreach (var c in t.Bank) if (c.Value > best.Value) best = c;
            return best.CardType;
        }

        // ------------------------------------------------------------------
        private void EndGame()
        {
            SetState(GameState.GameOver);
            var winner = Engine.GetWinner();
            _ui.ShowGameOver(winner, Engine);
        }
    }
}