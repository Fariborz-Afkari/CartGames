using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AngelVSZombie
{
    public class GameFlowController : MonoBehaviour
    {
        public GameUIController UI;
        public GameState CurrentState;

        [Header("Available Player Templates")]
        public List<PlayerDefinition> PlayerDefinitions;

        [Header("All Cards")]
        public List<CardDefinition> AllCards;

        private GameData _gameData;
        private GameEngine _engine;
        private AiPlayer _aiPlayer;

        private bool _isDying = false;
        private bool _angelClicked = false;
        public bool IsPlayerDying => _isDying;

        private bool _waitingForInput = false;

        // نوبت‌های اضافه‌ای که باید به بازیکن بعدی داده شود (ناشی از Attack / DeadsAttack)
        private int _extraTurnsForNextPlayer = 0;

        private int _initialPlayerCount = 2;
        private int _hardness = 2;
        private float _speed = 3f;

        private CardDefinition _tempTopCard;
        private CardDefinition _tempSecondCard;

        private void Start()
        {
            // منتظر تنظیمات از StartPanel
        }

        // ============================================================
        // شروع و بارگذاری بازی
        // ============================================================

        public void StartGame(int playerCount, int hardness, float speed)
        {
            _initialPlayerCount = playerCount;
            _hardness = hardness;
            _speed = speed;

            if (UI == null) UI = FindObjectOfType<GameUIController>();
            if (UI == null)
            {
                Debug.LogError("GameUIController در صحنه پیدا نشد!");
                return;
            }

            _gameData = new GameData();
            _engine = new GameEngine(_gameData, UI, this);
            _extraTurnsForNextPlayer = 0;
            _waitingForInput = false;

            if (AllCards.Count == 0)
            {
                Debug.LogError("لیست کارت‌ها خالی است!");
                return;
            }

            var defs = BuildPlayerDefinitions(playerCount);
            CurrentState = GameState.Setup;
            _engine.SetupGame(defs, AllCards);

            // ساخت پنل بازیکن‌ها
            UI.BuildPlayerPanels(_gameData.Players);

            _gameData.CurrentPlayerIndex = Random.Range(0, _gameData.Players.Count);
            UI.UpdateStatus($"بازی شروع شد! اولین بازیکن: {_gameData.Players[_gameData.CurrentPlayerIndex].Definition.PlayerName}");

            StartTurn();
        }

        public void LoadGame()
        {
            // فعلاً بازی از نو شروع می‌شود (بعداً می‌توانید منطق ذخیره/بارگذاری اضافه کنید)
            StartGame(2, 2, 3f);
        }

        private List<PlayerDefinition> BuildPlayerDefinitions(int count)
        {
            var result = new List<PlayerDefinition>();
            for (int i = 0; i < count; i++)
            {
                if (i < PlayerDefinitions.Count)
                {
                    result.Add(PlayerDefinitions[i]);
                }
                else
                {
                    var def = ScriptableObject.CreateInstance<PlayerDefinition>();
                    def.PlayerName = i == 0 ? "بازیکن" : $"هوش مصنوعی {i}";
                    def.IsHuman = i == 0;
                    def.AvatarIndex = i;
                    result.Add(def);
                }
            }
            return result;
        }

        // ============================================================
        // Helpers
        // ============================================================
        public void OnPlayerSelectedBankCard(CardDefinition card)
        {
            if (card == null) return;

            Debug.Log($"[CardClick] کارت کلیک شده: {card.CardName} | isDying: {_isDying}");

            // اگر بازیکن در حال جان دادن است و روی فرشته کلیک کرد
            if (_isDying)
            {
                if (card.CardType == CardType.Angel)
                {
                    _angelClicked = true;
                    UI.UpdateStatus("فرشته انتخاب شد! در حال نجات...");
                    Debug.Log("[CardClick] _angelClicked = true");
                }
                else
                {
                    UI.UpdateStatus("شما در حال جان دادن هستید! فقط می‌توانید از فرشته استفاده کنید.");
                }
                return; // از ادامه جلوگیری می‌کند
            }

            PlayerData human = GetHumanPlayer();
            if (human == null) return;
            if (!human.Hand.Contains(card)) return;

            bool isHumanTurn = (CurrentState == GameState.PlayerTurn);
            if (!isHumanTurn && !card.Immediate) return;
            if (!human.IsAlive && !card.Immediate) return;

            StartCoroutine(PlayCardRoutine(card, human, isHumanTurn));
        }

        public void HandleZombieDraw(PlayerData player, CardDefinition zombieCard)
        {
            UI.ShowZombiePlace(zombieCard);
            Debug.Log($"[Zombie] بازیکن {player.Definition.PlayerName} زامبی کشید.");

            // اگر هوش مصنوعی زامبی کشید
            if (!player.Definition.IsHuman)
            {
                StartCoroutine(AiZombieDecisionRoutine(player, zombieCard));
                return;
            }

            // اگر بازیکن انسانی زامبی کشید
            _angelClicked = false;
            _isDying = true;

            Debug.Log("[Zombie] _isDying = true. فرصت 5 ثانیه‌ای شروع شد.");

            // رفرش دست تا فرشته فعال شود
            RefreshHumanHandDisplay();

            CurrentState = GameState.WaitingForAction;

            StartCoroutine(ZombieDeathTimer(player, zombieCard));
        }
        private PlayerData GetHumanPlayer()
        {
            return _gameData.Players.FirstOrDefault(p => p.Definition.IsHuman);
        }

        /// <summary>
        /// همیشه دست بازیکن انسانی را در PlayerBank نمایش می‌دهد.
        /// در نوبت انسانی تعاملی، در غیر این‌صورت فقط قابل مشاهده.
        /// </summary>
        private void RefreshHumanHandDisplay()
        {
            PlayerData human = GetHumanPlayer();
            if (human != null) // شرط IsAlive را برداشتیم تا کارت‌های Immediate در حالت مرده هم دیده شوند
                UI.RenderHand(human);
            else
                UI.ClearPlayerBank();
        }

        private void UpdateDiscard()
        {
            if (_gameData.DiscardPile.Count > 0)
                UI.UpdateDiscardDisplay(_gameData.DiscardPile[_gameData.DiscardPile.Count - 1]);
            else
                UI.UpdateDiscardDisplay(null);
        }

        /// <summary>
        /// انتقال نوبت‌های اضافه‌ای که در اثر بازی یک کارت افزایش یافته‌اند به صف «بازیکن بعدی».
        /// </summary>
        private void ApplyExtrasDelta(int extrasBefore)
        {
            int delta = _gameData.ExtraTurns - extrasBefore;
            if (delta > 0)
            {
                _extraTurnsForNextPlayer += delta;
                _gameData.ExtraTurns = extrasBefore;
            }
        }

        // ============================================================
        // مدیریت نوبت
        // ============================================================

        public void StartTurn()
        {
            if (CurrentState == GameState.GameOver) return;

            // نمایش نشانگر نوبت برای بازیکن فعلی
            UI.UpdateTurnIndicator(_gameData.CurrentPlayerIndex);

            PlayerData currentPlayer = _gameData.Players[_gameData.CurrentPlayerIndex];
            if (!currentPlayer.IsAlive) { NextTurn(); return; }

            _gameData.SkipDraw = false;

            if (currentPlayer.Definition.IsHuman)
            {
                CurrentState = GameState.PlayerTurn;
                RefreshHumanHandDisplay();
                UI.UpdateStatus($"نوبت {currentPlayer.Definition.PlayerName} است. کارت بازی کنید یا دک بکشید.");
            }
            else
            {
                CurrentState = GameState.AiTurn;
                RefreshHumanHandDisplay(); // فقط نمایش
                UI.UpdateStatus($"نوبت {currentPlayer.Definition.PlayerName} (هوش مصنوعی).");
                StartCoroutine(AiTurnCoroutine(currentPlayer));
            }
        }

        public void NextTurn()
        {
            CheckWinCondition();
            if (CurrentState == GameState.GameOver) return;

            // 1) اگر بازیکن فعلی نوبت اضافه دارد، یکی مصرف کن و دوباره بازی کن
            if (_gameData.ExtraTurns > 0)
            {
                _gameData.ExtraTurns--;
                UI.UpdateStatus($"نوبت اضافه! {_gameData.Players[_gameData.CurrentPlayerIndex].Definition.PlayerName} دوباره بازی می‌کند.");
                StartTurn();
                return;
            }

            // 2) انتقال به بازیکن زنده بعدی
            int safety = 0;
            do
            {
                _gameData.CurrentPlayerIndex = (_gameData.CurrentPlayerIndex + 1) % _gameData.Players.Count;
                safety++;
                if (safety > _gameData.Players.Count + 1) { EndGame(); return; }
            } while (!_gameData.Players[_gameData.CurrentPlayerIndex].IsAlive);

            // 3) انتقال نوبت‌های اضافه به بازیکن جدید
            if (_extraTurnsForNextPlayer > 0)
            {
                _gameData.ExtraTurns += _extraTurnsForNextPlayer;
                _extraTurnsForNextPlayer = 0;
            }

            StartTurn();
        }

        // ============================================================
        // تعاملات بازیکن انسانی
        // ============================================================

       
        private IEnumerator PlayCardRoutine(CardDefinition card, PlayerData player, bool isHumanTurn)
        {
            GameState previousState = CurrentState;

            // 1. قفل کردن ورودی‌ها (تا زمانی که افکت اجرا نشده، بازیکن نتواند کارت دیگری بزند)
            CurrentState = GameState.WaitingForAction;
            _waitingForInput = false;

            // 2. نمایش کارت روی تخته
            UI.ShowCardOnBoard(card);

            // 3. تاخیر 3 ثانیه‌ای قبل از اجرای قابلیت
            yield return new WaitForSeconds(3f);

            // 4. اجرای قابلیت کارت در موتور بازی
            int extrasBefore = _gameData.ExtraTurns;
            _engine.PlayCards(player, new List<CardDefinition> { card });
            ApplyExtrasDelta(extrasBefore);

            // آپدیت UI
            RefreshHumanHandDisplay();
            UI.RefreshPlayerPanels();

            // 5. اگر کارت نیاز به تعامل دارد (مثل اوراکل، هدیه، غیب بین و...)
            if (_waitingForInput)
            {
                // روتین متوقف می‌شود تا پرامپت (پنل) باز شود. 
                // پاک کردن تخته به عهده متد Resolve آن پرامپت است.
                yield break;
            }

            // اگر افکت فوری تمام شد
            UpdateDiscard();

            if (!isHumanTurn)
            {
                // اگر کارت Immediate خارج از نوبت بازی شده بود
                yield return new WaitForSeconds(1f);
                UI.ClearBoard();

                // بازگرداندن وضعیت به حالت قبل (مثلاً نوبت هوش مصنوعی)
                CurrentState = previousState;
                RefreshHumanHandDisplay();
            }
            else
            {
                // اگر در نوبت خودمان بودیم
                yield return new WaitForSeconds(1f); // کمی صبر برای دیدن افکت
                UI.ClearBoard();

                if (_gameData.SkipDraw)
                {
                    EndTurn(false);
                }
                else
                {
                    CurrentState = GameState.PlayerTurn;
                    RefreshHumanHandDisplay();
                    UI.UpdateStatus($"نوبت {player.Definition.PlayerName} است. کارت دیگری بازی کنید یا دک بکشید.");
                }
            }
        }
        // این متد را برای مدیریت کارت‌های Immediate خارج از نوبت اضافه کنید
        private IEnumerator FinishImmediatePlayOutOfTurn(GameState previousState)
        {
            yield return new WaitForSeconds(1.2f);
            UI.ClearBoard();
            UpdateDiscard();

            // بازگرداندن وضعیت بازی به حالت قبل (مثلاً نوبت هوش مصنوعی)
            CurrentState = previousState;
            RefreshHumanHandDisplay();
        }

        public void OnEndTurnClicked()
        {
            if (CurrentState != GameState.PlayerTurn) return;
            EndTurn(true);
        }

        private void EndTurn(bool shouldDraw)
        {
            if (CurrentState == GameState.GameOver) return;
            UI.ClearBoard();

            PlayerData currentPlayer = _gameData.Players[_gameData.CurrentPlayerIndex];

            if (shouldDraw && !_gameData.SkipDraw)
            {
                bool isZombie = _engine.DrawCardFromDeck(currentPlayer);
                RefreshHumanHandDisplay();
                UpdateDiscard();

                // اگر زامبی کشیده شد، روتین زامبی مدیریت نوبت را بر عهده می‌گیرد
                if (isZombie) return;

                if (CurrentState == GameState.WaitingForAction) return;
                if (!currentPlayer.IsAlive) { NextTurn(); return; }
            }

            NextTurn();
        }

        private IEnumerator FinishImmediatePlay()
        {
            yield return new WaitForSeconds(1.2f);
            UI.ClearBoard();
            UpdateDiscard();

            if (_gameData.SkipDraw)
            {
                EndTurn(false);
            }
            else
            {
                CurrentState = GameState.PlayerTurn;
                RefreshHumanHandDisplay();
                UI.UpdateStatus($"نوبت {_gameData.Players[_gameData.CurrentPlayerIndex].Definition.PlayerName} است. کارت دیگری بازی کنید یا دک بکشید.");
            }
        }

        private void ResumeCurrentPlayerTurn()
        {
            if (CurrentState == GameState.GameOver) return;
            if (CurrentState == GameState.WaitingForAction) return; // پرامپت باز است

            PlayerData currentPlayer = _gameData.Players[_gameData.CurrentPlayerIndex];
            if (!currentPlayer.IsAlive) { NextTurn(); return; }

            UI.ClearBoard();
            UpdateDiscard();

            CurrentState = GameState.PlayerTurn;
            RefreshHumanHandDisplay();
            UI.UpdateStatus($"نوبت {currentPlayer.Definition.PlayerName} است.");
        }

        // ============================================================
        // هوش مصنوعی
        // ============================================================

        private IEnumerator AiTurnCoroutine(PlayerData aiPlayer)
        {
            float thinkTime = 3f / _speed;
            yield return new WaitForSeconds(thinkTime);

            _aiPlayer = new AiPlayer(aiPlayer, _engine, _hardness);
            CardDefinition cardToPlay = _aiPlayer.DecideCardToPlay();

            if (cardToPlay != null)
            {
                // 1. نمایش کارت روی تخته
                UI.ShowCardOnBoard(cardToPlay);

                // 2. تاخیر 3 ثانیه‌ای (تقسیم بر سرعت بازی می‌شود تا اگر سرعت را زیاد کردید، سریع‌تر شود)
                float delayBeforeEffect = 3f / _speed;
                yield return new WaitForSeconds(delayBeforeEffect);

                // 3. اجرای افکت
                int extrasBefore = _gameData.ExtraTurns;
                _engine.PlayCards(aiPlayer, new List<CardDefinition> { cardToPlay });
                ApplyExtrasDelta(extrasBefore);

                UI.RefreshPlayerPanels();
                UpdateDiscard();

                // 4. کمی صبر بعد از افکت برای درک بهتر
                yield return new WaitForSeconds(1f / _speed);

                // پاک کردن تخته
                UI.ClearBoard();
                UpdateDiscard();
            }

            if (_gameData.SkipDraw) EndTurn(false);
            else EndTurn(true);
        }

        // ============================================================
        // پرامپت‌های تعاملی
        // ============================================================

        // ---------- دزدیدن کارت تصادفی (2 کارت همسان) ----------
        public void PromptRandomSteal()
        {
            CurrentState = GameState.WaitingForAction;
            _waitingForInput = true;
            UI.UpdateStatus("یک بازیکن را انتخاب کنید تا بطور شانسی یک کارت او را بردارید.");
        }

        public void ResolveRandomSteal(PlayerData target)
        {
            if (!_waitingForInput || target == null || target.Hand.Count == 0) return;
            _waitingForInput = false;

            PlayerData currentPlayer = _gameData.Players[_gameData.CurrentPlayerIndex];
            int idx = Random.Range(0, target.Hand.Count);
            CardDefinition stolen = target.Hand[idx];
            target.Hand.RemoveAt(idx);
            currentPlayer.Hand.Add(stolen);

            UI.UpdateStatus($"یک کارت از {target.Definition.PlayerName} گرفته شد.");
            UI.ClearBoard();      // پاک کردن کارتی که 3 ثانیه مونده بود
            UpdateDiscard();      // فرستادن کارت به دک سوخته
            ResumeCurrentPlayerTurn();
        }

        // ---------- دزدیدن کارت مشخص (3 کارت همسان) ----------
        public void PromptSpecificSteal()
        {
            CurrentState = GameState.WaitingForAction;
            _waitingForInput = true;
            UI.UpdateStatus("یک بازیکن را انتخاب کنید و کارت مشخصی از او بخواهید.");
        }

        public void ResolveSpecificSteal(PlayerData target, CardDefinition requestedCard)
        {
            if (!_waitingForInput || target == null || requestedCard == null) return;
            _waitingForInput = false;

            PlayerData currentPlayer = _gameData.Players[_gameData.CurrentPlayerIndex];
            CardDefinition cardFound = target.Hand.FirstOrDefault(c => c.CardType == requestedCard.CardType);

            if (cardFound != null)
            {
                target.Hand.Remove(cardFound);
                currentPlayer.Hand.Add(cardFound);
                UI.UpdateStatus($"کارت {requestedCard.CardName} از {target.Definition.PlayerName} گرفته شد.");
            }
            else
            {
                UI.UpdateStatus($"بازیکن {target.Definition.PlayerName} آن کارت را نداشت.");
            }
            UI.ClearBoard();      // پاک کردن کارتی که 3 ثانیه مونده بود
            UpdateDiscard();      // فرستادن کارت به دک سوخته
            ResumeCurrentPlayerTurn();
        }

        // ---------- فرشته (نجات از زامبی) ----------
        public void PromptUseAngel(PlayerData player)
        {
            if (player.Definition.IsHuman)
            {
                CurrentState = GameState.WaitingForAction;
                _waitingForInput = true;
                UI.UpdateStatus($"{player.Definition.PlayerName} زامبی کشید! آیا از فرشته استفاده می‌کند؟");
            }
            else
            {
                ResolveAngelUse(true);
            }
        }

        public void ResolveAngelUse(bool useAngel)
        {
            if (!_waitingForInput && _gameData.Players[_gameData.CurrentPlayerIndex].Definition.IsHuman) return;
            _waitingForInput = false;

            PlayerData player = _gameData.Players[_gameData.CurrentPlayerIndex];

            if (useAngel)
            {
                var angel = player.Hand.FirstOrDefault(c => c.CardType == CardType.Angel);
                if (angel != null)
                {
                    player.Hand.Remove(angel);
                    _gameData.DiscardPile.Add(angel);
                    player.HasAngel = false;
                    UI.UpdateStatus($"{player.Definition.PlayerName} با فرشته نجات یافت!");
                }
            }
            else
            {
                player.IsAlive = false;
                UI.UpdateStatus($"{player.Definition.PlayerName} مرد!");
            }

            RefreshHumanHandDisplay();
            UI.RefreshPlayerPanels();
            UpdateDiscard();

            // اگر انسان بود، نوبتش را ببند؛ اگر AI بود، EndTurn بیرونی خودش ادامه می‌دهد
            if (CurrentState == GameState.WaitingForAction)
            {
                EndTurn(false);
            }
            UI.ClearBoard();      // پاک کردن کارتی که 3 ثانیه مونده بود
            UpdateDiscard();      // فرستادن کارت به دک سوخته
        }

       
        private System.Collections.IEnumerator AiZombieDecisionRoutine(PlayerData aiPlayer, CardDefinition zombieCard)
        {
            yield return new UnityEngine.WaitForSeconds(2f);

            _aiPlayer = new AiPlayer(aiPlayer, _engine, _hardness);
            bool useAngel = _aiPlayer.DecideUseAngel();

            UI.ClearZombiePlace();

            if (useAngel)
            {
                // 1. زامبی دوباره به دک برمی گردد
                _gameData.Deck.Add(zombieCard);

                // 2. فرشته کلا خارج می شود
                var angel = aiPlayer.Hand.FirstOrDefault(c => c.CardType == CardType.Angel);
                if (angel != null) aiPlayer.Hand.Remove(angel);
                aiPlayer.HasAngel = false;

                UI.UpdateStatus($"{aiPlayer.Definition.PlayerName} با فرشته نجات یافت!");

                // 3. زنده کردن یک بازیکن مرده
                var deadPlayers = _gameData.Players.Where(p => !p.IsAlive).ToList();
                if (deadPlayers.Count > 0)
                {
                    var target = deadPlayers[UnityEngine.Random.Range(0, deadPlayers.Count)];
                    target.IsAlive = true;
                    UI.UpdateStatus($"... و {target.Definition.PlayerName} را زنده کرد!");
                }

                // 4. ایجاد تعادل زامبی‌ها
                BalanceZombiesInDeck();

                UI.RefreshPlayerPanels();
                NextTurn();
            }
            else
            {
                // هوش مصنوعی مرد
                aiPlayer.IsAlive = false;
                UI.RefreshPlayerPanels();
                UI.UpdateStatus($"{aiPlayer.Definition.PlayerName} مرد!");

                // زامبی به گورستان می رود
                _gameData.Graveyard.Add(zombieCard);

                CheckWinCondition();
                if (CurrentState != GameState.GameOver) NextTurn();
            }
        }
        private IEnumerator ZombieDeathTimer(PlayerData player, CardDefinition zombieCard)
        {
            float timer = 5f;

            while (timer > 0f)
            {
                if (_angelClicked) break;
                UI.UpdateStatus($"زامبی کشیدید! {timer:F1} ثانیه فرصت دارید تا با کلیک روی فرشته نجات یابید.");
                yield return new WaitForSeconds(0.1f);
                timer -= 0.1f;
            }

            _isDying = false;
            UI.ClearZombiePlace();

            if (_angelClicked)
            {
                // 1. زامبی دوباره به دک برمی گردد و دک شافل می شود
                _gameData.Deck.Add(zombieCard);
                _engine.ShuffleDeck();
                UI.UpdateDeckCount(_gameData.Deck.Count);

                // 2. کارت فرشته کلا از بازی خارج می شود (به DiscardPile اضافه نمی شود)
                var angel = player.Hand.FirstOrDefault(c => c.CardType == CardType.Angel);
                if (angel != null)
                {
                    player.Hand.Remove(angel);
                    // _gameData.DiscardPile.Add(angel); // این خط حذف شد تا فرشته از بازی خارج شود
                }

                UI.UpdateStatus($"{player.Definition.PlayerName} با فرشته نجات یافت! زامبی به دک برگشت.");
                RefreshHumanHandDisplay();

                // بررسی مرده‌ها برای زنده کردن
                var deadPlayers = _gameData.Players.Where(p => !p.IsAlive).ToList();
                if (deadPlayers.Count > 0)
                {
                    UI.ShowSelectDeadPlayerPopup(deadPlayers);
                }
                else
                {
                    UI.UpdateStatus("هیچ بازیکن مرده‌ای برای زنده کردن وجود ندارد.");
                    CurrentState = GameState.PlayerTurn;
                    NextTurn();
                }
            }
            else
            {
                // بازیکن مرد
                player.IsAlive = false;
                UI.RefreshPlayerPanels();
                UI.UpdateStatus($"{player.Definition.PlayerName} مرد!");

                // زامبی به گورستان می رود (نه دک سوخته)
                _gameData.Graveyard.Add(zombieCard);

                RefreshHumanHandDisplay();
                CheckWinCondition();
                if (CurrentState != GameState.GameOver) NextTurn();
            }
        }
        public void ResolveSelectDeadPlayer(PlayerData target)
        {
            UI.HideSelectDeadPlayerPopup();
            if (target != null && !target.IsAlive)
            {
                target.IsAlive = true;
                UI.RefreshPlayerPanels();
                UI.UpdateStatus($"{target.Definition.PlayerName} زنده شد!");
            }

            // ایجاد تعادل: چون یک نفر زنده شد، باید یک زامبی به دک برگردد
            BalanceZombiesInDeck();

            CurrentState = GameState.PlayerTurn;
            NextTurn();
        }

        private void BalanceZombiesInDeck()
        {
            int aliveCount = _gameData.Players.Count(p => p.IsAlive);
            int targetZombies = Mathf.Max(0, aliveCount - 1); // باید یکی کمتر از زنده‌ها باشد

            int currentZombies = _gameData.Deck.Count(c => c.CardType == CardType.Zombie);

            // اگر زامبی‌های داخل دک کمتر از حد نیاز بود، از گورستان به دک اضافه کن
            while (currentZombies < targetZombies && _gameData.Graveyard.Count > 0)
            {
                CardDefinition zombie = _gameData.Graveyard[0];
                _gameData.Graveyard.RemoveAt(0);
                _gameData.Deck.Add(zombie);
                currentZombies++;
                Debug.Log($"[Balance] یک زامبی از گورستان به دک برگشت. تعداد زامبی‌های دک: {currentZombies}");
            }

            _engine.ShuffleDeck();
            UI.UpdateDeckCount(_gameData.Deck.Count);
        }

        // ---------- غیب بین ----------
        public void PromptEyeEffect(CardDefinition topCard)
        {
            CurrentState = GameState.WaitingForAction;
            _waitingForInput = true;
            UI.UpdateStatus($"غیب بین: کارت رویی {topCard.CardName} است. آیا آن را برمی‌دارید؟");
        }

        public void ResolveEyeEffect(bool takeCard)
        {
            if (!_waitingForInput) return;
            _waitingForInput = false;

            PlayerData player = _gameData.Players[_gameData.CurrentPlayerIndex];
            if (takeCard)
            {
                _engine.DrawCardFromDeck(player);
                UpdateDiscard();
            }
            else
            {
                UI.UpdateStatus("کارت رویی دک برداشته نشد.");
            }
            UI.ClearBoard();      // پاک کردن کارتی که 3 ثانیه مونده بود
            UpdateDiscard();      // فرستادن کارت به دک سوخته
            ResumeCurrentPlayerTurn();
        }

        // ---------- پیش گویی ----------
        // ---------- پیش گویی (Oracle) ----------
        public void PromptOracleEffect(List<CardDefinition> top3)
        {
            CurrentState = GameState.WaitingForAction;
            _waitingForInput = true;

            UI.UpdateStatus("پیش گویی: 3 کارت رویی دک را مشاهده می‌کنید.");

            // باز کردن پنل و نمایش 3 کارت
            UI.ShowOraclePopup(top3);
        }

        public void ResolveOracleEffect()
        {
            if (!_waitingForInput) return;
            _waitingForInput = false;

            // بستن پنل
            UI.HideOraclePopup();

            // چون کارت پیش گویی در GameEngine بلافاصله به DiscardPile اضافه شده بود،
            // حالا که پنل بسته شد، تخته را پاک کرده و کارت را رو به دک سوخته منتقل می‌کنیم
            UI.ClearBoard();
            UpdateDiscard();

            // این خط بسیار مهم است!(وضعیت باید به نوبت بازیکن برگردد)
    CurrentState = GameState.PlayerTurn;

            UI.UpdateStatus("پیش گویی تمام شد.");

            // بازگشت به نوبت بازیکن برای ادامه بازی
            ResumeCurrentPlayerTurn();
        }

        // ---------- هدیه ----------
        public void PromptGiftSelection()
        {
            CurrentState = GameState.WaitingForAction;
            _waitingForInput = true;
            UI.UpdateStatus("هدیه: یک بازیکن زنده را انتخاب کنید تا یک کارت از او بگیرید.");

            PlayerData currentPlayer = _gameData.Players[_gameData.CurrentPlayerIndex];

            // پیدا کردن بازیکنان زنده به جز خودمان
            var aliveTargets = _gameData.Players.Where(p => p.IsAlive && p != currentPlayer).ToList();

            // نمایش پاپ‌آپ با لیست بازیکنان زنده
            UI.ShowGiftTargetPopup(aliveTargets);
        }


        public void ResolveGiftSelection(PlayerData target)
        {
            // بررسی ایمنی
            if (!_waitingForInput || target == null) return;
            _waitingForInput = false;

            // 1. بستن فوری پنل
            UI.HideGiftTargetPopup();
            Debug.Log("[Gift] پنل بسته شد.");

            PlayerData currentPlayer = _gameData.Players[_gameData.CurrentPlayerIndex];

            // 2. جابه‌جایی کارت‌ها
            if (target.Hand.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, target.Hand.Count);
                CardDefinition stolenCard = target.Hand[idx];

                target.Hand.RemoveAt(idx);
                currentPlayer.Hand.Add(stolenCard);

                UI.UpdateStatus($"کارت {stolenCard.CardName} از {target.Definition.PlayerName} گرفته شد.");
            }
            else
            {
                UI.UpdateStatus($"بازیکن {target.Definition.PlayerName} کارتی نداشت.");
            }

            // 3. آپدیت UI بازی
            RefreshHumanHandDisplay();
            UI.RefreshPlayerPanels();

            // 4. پاک کردن تخته (کارت هدیه را به دک سوخته می‌فرستد)
            UI.ClearBoard();
            UpdateDiscard();

            // 5. برگرداندن نوبت
            CurrentState = GameState.PlayerTurn;
            ResumeCurrentPlayerTurn();
        }

        // ---------- تونل زن ----------
        public void PromptTunnelEffect(CardDefinition topCard)
        {
            CurrentState = GameState.WaitingForAction;
            _waitingForInput = true;

            CardDefinition secondCard = _gameData.Deck.Count > 1 ? _gameData.Deck[1] : null;

            // ذخیره موقت کارت‌ها برای استفاده در زمان انتخاب
            _tempTopCard = topCard;
            _tempSecondCard = secondCard;

            // خارج کردن کارت‌ها از دک
            _gameData.Deck.RemoveAt(0);
            if (secondCard != null) _gameData.Deck.RemoveAt(0);

            UI.UpdateDeckCount(_gameData.Deck.Count);

            // نمایش پنل تونل
            UI.ShowTunnelPopup(topCard, secondCard);
        }

        public void ResolveTunnelSelection(bool takeTopCard)
        {
            if (!_waitingForInput) return;
            _waitingForInput = false;

            UI.HideTunnelPopup();

            PlayerData player = _gameData.Players[_gameData.CurrentPlayerIndex];

            CardDefinition selectedCard = takeTopCard ? _tempTopCard : _tempSecondCard;
            CardDefinition unselectedCard = takeTopCard ? _tempSecondCard : _tempTopCard;

            // اگر کارت انتخاب شده زامبی بود
            if (selectedCard != null && selectedCard.CardType == CardType.Zombie)
            {
                UI.UpdateStatus("شما زامبی کشیدید!");

                // کارت دیگر به بالای دک برمی‌گردد
                if (unselectedCard != null) _gameData.Deck.Insert(0, unselectedCard);

                HandleZombieDraw(player, selectedCard);
                return;
            }

            // اگر کارت معمولی بود، به دست بازیکن اضافه می‌شود
            if (selectedCard != null)
            {
                player.Hand.Add(selectedCard);
                UI.UpdateStatus($"کارت {selectedCard.CardName} به دست شما اضافه شد.");
            }

            // کارت دیگر به بالای دک برمی‌گردد
            if (unselectedCard != null) _gameData.Deck.Insert(0, unselectedCard);

            RefreshHumanHandDisplay();
            UpdateDiscard();
            UI.ClearBoard();

            // چون کارت تونل یک نوبت بازی حساب می‌شود، نوبت را رد می‌کنیم
            EndTurn(false);
        }

        // ---------- خیرات ----------
        // ---------- خیرات (FeedDead) ----------
        public void PromptFeedDeadSelection()
        {
            CurrentState = GameState.WaitingForAction;
            _waitingForInput = true;

            // پیدا کردن بازیکنان مرده
            var deadPlayers = _gameData.Players.Where(p => !p.IsAlive).ToList();

            if (deadPlayers.Count == 0)
            {
                UI.UpdateStatus("خیرات: هیچ بازیکن مرده‌ای برای انتخاب وجود ندارد.");
                // اگر مرده‌ای نبود، کارت باطل می‌شود و بازی ادامه پیدا می‌کند
                UI.ClearBoard();
                UpdateDiscard();
                CurrentState = GameState.PlayerTurn;
                ResumeCurrentPlayerTurn();
                return;
            }

            UI.UpdateStatus("خیرات: یک بازیکن مرده را انتخاب کنید تا بقیه به او کارت بدهند.");

            // نمایش پنل انتخاب
            UI.ShowFeedDeadPopup(deadPlayers);
        }

        public void ResolveFeedDeadSelection(PlayerData deadTarget)
        {
            if (!_waitingForInput || deadTarget == null) return;
            _waitingForInput = false;

            // بستن پنل
            UI.HideFeedDeadPopup();

            int cardsGiven = 0;

            // همه بازیکنان زنده (به جز خود بازیکن مرده) یک کارت به او می‌دهند
            foreach (var player in _gameData.Players)
            {
                if (player == deadTarget) continue; // به خود مرده کارت نمی‌دهیم
                if (!player.IsAlive) continue;      // از بازیکنان مرده کارت کم نمی‌شود
                if (player.Hand.Count == 0) continue; // اگر کارتی ندارد، رد می‌شود

                // انتخاب یک کارت تصادفی از بازیکن زنده
                int randomIdx = Random.Range(0, player.Hand.Count);
                CardDefinition givenCard = player.Hand[randomIdx];

                // انتقال کارت از بازیکن زنده به بازیکن مرده
                player.Hand.RemoveAt(randomIdx);
                deadTarget.Hand.Add(givenCard);
                cardsGiven++;
            }

            UI.UpdateStatus($"تعداد {cardsGiven} کارت از بازیکنان زنده به {deadTarget.Definition.PlayerName} داده شد.");

            // آپدیت UI (چون ممکن است کارت‌های بازیکن انسانی کم شده باشد)
            RefreshHumanHandDisplay();
            UI.RefreshPlayerPanels();

            // پاک کردن تخته و فرستادن کارت خیرات به دک سوخته
            UI.ClearBoard();
            UpdateDiscard();

            // پایان نوبت (چون کارت خیرات بازی شده است)
            CurrentState = GameState.PlayerTurn;
            EndTurn(false);
        }

        // ============================================================
        // پایان بازی
        // ============================================================

        public void CheckWinCondition()
        {
            int aliveCount = _gameData.Players.Count(p => p.IsAlive);

            if (_initialPlayerCount == 1)
            {
                if (aliveCount == 0) EndGame();
            }
            else
            {
                if (aliveCount <= 1) EndGame();
            }
        }

        public void EndGame()
        {
            CurrentState = GameState.GameOver;
            PlayerPrefs.DeleteKey("SavedGame");
            PlayerPrefs.Save();

            UI.UpdateTurnIndicator(-1); // خاموش کردن همه نشانگرها

            UI.RefreshPlayerPanels();
            UI.ClearBoard();

            PlayerData winner = _gameData.Players.FirstOrDefault(p => p.IsAlive);
            if (winner != null)
                UI.UpdateStatus($"بازی تمام شد! برنده: {winner.Definition.PlayerName}");
            else
                UI.UpdateStatus("بازی تمام شد! همه مردند.");
        }
    }
}