using System;
using System.Collections.Generic;
using CardGames.DeadManDraws.Core.Cards;
using CardGames.DeadManDraws.Core.Players;
using UnityEngine;
namespace CardGames.DeadManDraws.Core.Game
{
    public sealed class PirateGameEngine
    {
        private readonly System.Random _random;

        private int _nextPlayerIndex;

        public GameState State { get; }

        public event Action<string> EventProduced;

        public PirateGameEngine(string gameId)
        {
            _random =
                new System.Random();

            State =
                new GameState(gameId);
        }

        public void StartMatch(
     int playerCount,
     int deckCopies = 1,
     string humanPlayerName = "You")
        {
            if (playerCount < 1)
                throw new ArgumentOutOfRangeException(
                    nameof(playerCount),
                    "Player count must be at least 1.");

            if (deckCopies < 1)
                throw new ArgumentOutOfRangeException(
                    nameof(deckCopies),
                    "Deck copies must be at least 1.");

            /*
             * State.Players از نوع IReadOnlyList است.
             * بنابراین اینجا نباید Clear() روی آن اجرا شود.
             *
             * همچنین این Engine در نسخه فعلی متد BuildDeck ندارد،
             * پس هیچ BuildDeck() را صدا نمی‌زنیم.
             */

            CreatePlayers(
                playerCount,
                humanPlayerName);

            /*
             * برای تمام بازیکنان دو Trait تصادفی
             * تولید می‌شود.
             */
            AssignTraitOptions();

            /*
             * بازیکن انسانی Player 0 است.
             */
            State.CurrentPlayerId = 0;

            /*
             * بسیار مهم:
             * بعد از ساخت بازیکنان باید وارد مرحله
             * انتخاب Trait شویم.
             *
             * SelectTrait() فقط در این Phase اجازه کار دارد.
             */
            State.Phase = GamePhase.TraitSelection;

            State.TurnNumber = 0;
            State.IsGameOver = false;
            State.WinnerId = null;

            Emit("MatchStarted");
        }

        private void CreatePlayers(int count,string humanPlayerName)
        {
            

            for (int i = 0;
                 i < count;
                 i++)
            {
                string playerName =
                    i == 0
                        ? string.IsNullOrWhiteSpace(
                            humanPlayerName)
                            ? "You"
                            : humanPlayerName
                        : "Player " + (i + 1);

                PlayerState player =
                    new PlayerState(
                        i,
                        playerName,
                        i == 0);

                State.AddPlayer(player);
            }
        }

        private void AssignTraitOptions()
        {
            Debug.Log("---- AssignTraitOptions");
            PlayerTrait[] traits =
                GetAllTraits();

            for (int i = 0;
                 i < State.Players.Count;
                 i++)
            {
                PlayerState player =
                    State.Players[i];

                PlayerTrait first =
                    traits[
                        _random.Next(
                            traits.Length)];

                PlayerTrait second;

                do
                {
                    second =
                        traits[
                            _random.Next(
                                traits.Length)];
                }
                while (second == first);
                Debug.Log("---- AssignTraitOptions"+first+second);
                player.SetTraitOptions(
                    first,
                    second);
            }
        }

        public bool SubmitAction(GameAction action)
        {
            if (action == null)
                return false;

            PlayerState player =
                State.FindPlayer(
                    action.PlayerId);

            if (player == null)
                return false;

            switch (action.Type)
            {
                case GameActionType.SelectTrait:
                    return SelectTrait(
                        player,
                        action.Trait);

                case GameActionType.DrawCard:
                    return DrawCard(player);

                case GameActionType.StopDrawing:
                    return StopDrawing(player);

                case GameActionType.BankPlayArea:
                    return BankPlayArea(player);

                case GameActionType.SelectCannonTarget:
                    return UseCannon(
                        player,
                        action.TargetPlayerId,
                        null);

                case GameActionType.SelectCannonCard:
                    return UseCannon(
                        player,
                        action.TargetPlayerId,
                        action.TargetCardId);

                case GameActionType.SelectHookCard:
                    return UseHook(
                        player,
                        action.CardId);

                case GameActionType.SelectSwordCard:
                    return UseSword(
                        player,
                        action.TargetPlayerId,
                        action.TargetCardId);

                case GameActionType.SelectMapCard:
                    return UseMap(
                        player,
                        action.CardId);

                case GameActionType.SelectPlundererTarget:
                    player.PlundererTargetId =
                        action.TargetPlayerId;

                    return true;

                case GameActionType.SelectDavyJonesTarget:
                    player.DavyJonesTargetId =
                        action.TargetPlayerId;

                    return true;

                default:
                    return false;
            }
        }

        private bool SelectTrait(
            PlayerState player,
            PlayerTrait? trait)
        {
            if (State.Phase !=
                GamePhase.TraitSelection)
            {
                return false;
            }

            if (!trait.HasValue)
                return false;

            bool valid =
                false;

            for (int i = 0;
                 i < player.TraitOptions.Count;
                 i++)
            {
                if (player.TraitOptions[i] ==
                    trait.Value)
                {
                    valid = true;
                    break;
                }
            }

            if (!valid)
                return false;

            player.Trait =
                trait.Value;

            Emit(
                "TraitSelected:" +
                player.Id +
                ":" +
                trait.Value);

            if (AllTraitsSelected())
                StartFirstTurn();

            return true;
        }

        private bool AllTraitsSelected()
        {
            for (int i = 0;
                 i < State.Players.Count;
                 i++)
            {
                if (State.Players[i].Trait ==
                    PlayerTrait.None)
                {
                    return false;
                }
            }

            return true;
        }

        private void StartFirstTurn()
        {
            if (State.Players.Count == 0)
                return;

            _nextPlayerIndex =
                _random.Next(
                    State.Players.Count);

            State.CurrentPlayerId =
                State.Players[
                    _nextPlayerIndex].Id;

            State.TurnNumber = 1;

            State.Phase =
                GamePhase.Turn;

            ResetTurn();

            Emit(
                "TurnStarted:" +
                State.CurrentPlayerId);
        }

        private bool DrawCard(
            PlayerState player)
        {
            if (State.Phase !=
                GamePhase.Turn)
            {
                return false;
            }

            if (State.CurrentPlayerId !=
                player.Id)
            {
                return false;
            }

            if (!GameRules.IsValidDraw(
                    State,
                    player))
            {
                return false;
            }

            Card card =
                State.Deck.Draw();

            if (card == null)
                return false;

            player.HasDrawnAtLeastOne =
                true;

            /*
             * کارت ابتدا رو می‌شود و وارد
             * Play Area می‌شود.
             */
            player.PlayArea.Add(card);

            Emit(
                "CardDrawn:" +
                player.Id +
                ":" +
                card.InstanceId);

            /*
             * قانون جدید:
             * قبل از اجرای Effect، Bust بررسی می‌شود.
             */
            if (GameRules.IsBust(
                    player,
                    card))
            {
                ResolveBust(
                    player,
                    card);

                return true;
            }

            /*
             * کارت‌هایی که توسط Trait
             * بلافاصله Bank می‌شوند.
             */
            if (player.HasTrait(
                    PlayerTrait.Casanova) &&
                card.Type ==
                CardType.Mermaid)
            {
                BankCard(
                    player,
                    card);

                return true;
            }

            if (player.HasTrait(
                    PlayerTrait.Fisherman) &&
                card.Type ==
                CardType.Kraken)
            {
                BankCard(
                    player,
                    card);

                return true;
            }

            /*
             * Safe Harbor
             */
            if (card.Type ==
                CardType.Anchor &&
                player.HasTrait(
                    PlayerTrait.SafeHarbor))
            {
                player.SafeHarborCardsLeft =
                    3;
            }

            ResolveCardEffect(
                player,
                card);

            return true;
        }

        private void ResolveBust(
            PlayerState player,
            Card causingCard)
        {
            Emit(
                "Bust:" +
                player.Id +
                ":" +
                causingCard.InstanceId);

            /*
             * کارت ایجادکننده Bust نیز
             * قابلیت ندارد.
             *
             * Anchor:
             * کارت‌های قبل از Anchor حفظ می‌شوند.
             */
            int anchorIndex =
                FindAnchorIndex(player);

            int safeCount =
                player.ProtectedPlayCards;

            List<Card> keep =
                new List<Card>();

            if (anchorIndex >= 0)
            {
                for (int i = 0;
                     i < anchorIndex;
                     i++)
                {
                    keep.Add(
                        player.PlayArea.Cards[i]);
                }
            }

            /*
             * Safe Harbor کارت‌های محافظت‌شده
             * را نیز حفظ می‌کند.
             */
            for (int i = 0;
                 i < safeCount &&
                 i < player.PlayArea.Count;
                 i++)
            {
                Card card =
                    player.PlayArea.Cards[i];

                if (!keep.Contains(card))
                    keep.Add(card);
            }

            /*
             * Davy Jones:
             * در زمان Bust بازیکن هدف،
             * Play Area او را مالک Trait
             * می‌گیرد.
             */
            PlayerState davy =
                FindDavyJonesOwner(
                    player.Id);

            for (int i = 0;
                 i < player.PlayArea.Count;
                 i++)
            {
                Card card =
                    player.PlayArea.Cards[i];

                if (card == causingCard)
                {
                    State.AddToDiscard(card);
                    continue;
                }

                if (keep.Contains(card))
                    continue;

                if (davy != null)
                {
                    davy.Bank.Add(card);
                }
                else
                {
                    State.AddToDiscard(card);
                }
            }

            player.PlayArea.Clear();

            /*
             * کارت‌های Anchor-protected
             * و Safe Harbor را به Play Area
             * برمی‌گردانیم.
             */
            for (int i = 0;
                 i < keep.Count;
                 i++)
            {
                player.PlayArea.Add(
                    keep[i]);
            }

            player.HasDrawnAtLeastOne =
                true;

            Emit(
                "BustResolved:" +
                player.Id);
        }

        private int FindAnchorIndex(
            PlayerState player)
        {
            for (int i = 0;
                 i < player.PlayArea.Count;
                 i++)
            {
                if (player.PlayArea
                        .Cards[i].Type ==
                    CardType.Anchor)
                {
                    return i;
                }
            }

            return -1;
        }

        private PlayerState
            FindDavyJonesOwner(
                int targetPlayerId)
        {
            for (int i = 0;
                 i < State.Players.Count;
                 i++)
            {
                PlayerState player =
                    State.Players[i];

                if (player.HasTrait(
                        PlayerTrait.DavyJonesLocker) &&
                    player.DavyJonesTargetId ==
                        targetPlayerId)
                {
                    return player;
                }
            }

            return null;
        }

        private void ResolveCardEffect(
            PlayerState player,
            Card card)
        {
            switch (card.Type)
            {
                case CardType.Anchor:
                    ResolveAnchor(
                        player);
                    break;

                case CardType.Cannon:
                    Emit(
                        "CannonRequiresTarget:" +
                        player.Id);
                    break;

                case CardType.Chest:
                    ResolveChest(
                        player);
                    break;

                case CardType.Hook:
                    Emit(
                        "HookRequiresSelection:" +
                        player.Id);
                    break;

                case CardType.Key:
                    break;

                case CardType.Kraken:
                    ResolveKraken(
                        player);
                    break;

                case CardType.Map:
                    ResolveMapPreview(
                        player);
                    break;

                case CardType.Mermaid:
                    break;

                case CardType.Oracle:
                    ResolveOracle(
                        player);
                    break;

                case CardType.Sword:
                    Emit(
                        "SwordRequiresTarget:" +
                        player.Id);
                    break;
            }
        }

        private void ResolveAnchor(
            PlayerState player)
        {
            /*
             * همه کارت‌های قبل از Anchor
             * در Bust حفظ می‌شوند.
             */
            player.ProtectedPlayCards =
                FindAnchorIndex(player);

            Emit(
                "AnchorActivated:" +
                player.Id);
        }

        private void ResolveChest(
            PlayerState player)
        {
            if (!player.PlayArea
                    .ContainsType(
                        CardType.Key))
            {
                return;
            }

            /*
             * مقدار واقعی انتقال در پایان
             * Turn انجام می‌شود.
             */
            Emit(
                "ChestReady:" +
                player.Id);
        }

        private void ResolveKraken(
            PlayerState player)
        {
            int extraCards = 2;

            if (player.HasTrait(
                    PlayerTrait.Beastmaster))
            {
                extraCards = 4;
            }

            if (player.HasTrait(
                    PlayerTrait.Fisherman))
            {
                extraCards = 0;
            }

            Emit(
                "Kraken:" +
                player.Id +
                ":" +
                extraCards);

            for (int i = 0;
                 i < extraCards;
                 i++)
            {
                if (State.IsGameOver)
                    return;

                if (!IsCurrentPlayer(player))
                    return;

                int playAreaBefore =
                    player.PlayArea.Count;

                DrawCard(player);

                /*
                 * اگر Draw باعث Bust شده باشد،
                 * ResolveBust وضعیت PlayArea را تغییر می‌دهد.
                 * بنابراین Kraken دیگر نباید کارت بعدی بکشد.
                 */
                if (player.PlayArea.Count <
                    playAreaBefore)
                {
                    return;
                }
            }
        }

        private void ResolveMapPreview(
            PlayerState player)
        {
            State.ShuffleDiscard(_random);

            int count =
                player.HasTrait(
                    PlayerTrait.Navigator)
                    ? State.DiscardPile.Count
                    : 3;

            Emit(
                "MapSelection:" +
                player.Id +
                ":" +
                count);
        }

        private void ResolveOracle(
            PlayerState player)
        {
            int count =
                player.HasTrait(
                    PlayerTrait.Mystic)
                    ? 3
                    : 1;

            Emit(
                "OraclePreview:" +
                player.Id +
                ":" +
                count);
        }

        private bool StopDrawing(
            PlayerState player)
        {
            if (!IsCurrentPlayer(player))
                return false;

            if (!player.HasDrawnAtLeastOne)
                return false;

            return BankPlayArea(player);
        }

        private bool BankPlayArea(
            PlayerState player)
        {
            if (!IsCurrentPlayer(player))
                return false;

            if (!player.HasDrawnAtLeastOne)
                return false;

            /*
             * Chest/Key.
             */
            int playAreaCount =
                player.PlayArea.Count;

            bool hasChest =
                player.PlayArea.ContainsType(
                    CardType.Chest);

            bool hasKey =
                player.PlayArea.ContainsType(
                    CardType.Key);

            MovePlayAreaToBank(player);

            if (hasChest && hasKey)
            {
                ResolveChestReward(
                    player,
                    playAreaCount);
            }

            EndTurn();

            return true;
        }

        private void MovePlayAreaToBank(
            PlayerState player)
        {
            if (player == null)
                return;

            int count =
                player.PlayArea.Count;

            if (count <= 0)
                return;

            for (int i = 0;
                 i < count;
                 i++)
            {
                Card card =
                    player.PlayArea.Cards[i];

                if (card != null)
                    player.Bank.Add(card);
            }

            player.PlayArea.Clear();

            Emit(
                "CardsBanked:" +
                player.Id +
                ":" +
                count);
        }

        private void ResolveChestReward(
            PlayerState player,
            int amount)
        {
            if (amount <= 0)
                return;

            if (player.HasTrait(
                    PlayerTrait.TreasureHunter))
            {
                amount *= 3;
            }

            if (player.HasTrait(
                    PlayerTrait.Plunderer))
            {
                Emit(
                    "PlundererRequiresTarget:" +
                    player.Id +
                    ":" +
                    amount);

                return;
            }

            for (int i = 0;
                 i < amount;
                 i++)
            {
                State.ShuffleDiscard(
                    _random);

                Card card =
                    State.DrawDiscard();

                if (card == null)
                    break;

                player.Bank.Add(card);
            }
        }

        private bool UseCannon(
            PlayerState player,
            int? targetPlayerId,
            int? targetCardId)
        {
            if (!IsCurrentPlayer(player))
                return false;

            if (!targetPlayerId.HasValue)
                return false;

            PlayerState target =
                State.FindPlayer(
                    targetPlayerId.Value);

            if (target == null ||
                target.Id == player.Id)
            {
                return false;
            }

            if (target.HasTrait(
                    PlayerTrait.Misfire))
            {
                return ResolveMisfire(
                    player,
                    target);
            }

            Card selected =
                targetCardId.HasValue
                    ? target.Bank.Find(
                        targetCardId.Value)
                    : FindHighestBankCard(
                        target);

            if (selected == null)
                return false;

            if (player.HasTrait(
                    PlayerTrait.MasterGunner))
            {
                List<Card> all =
                    target.Bank.FindAll(
                        selected.Type);

                for (int i = 0;
                     i < all.Count;
                     i++)
                {
                    target.Bank.Remove(
                        all[i].InstanceId);

                    State.AddToDiscard(
                        all[i]);
                }
            }
            else
            {
                target.Bank.Remove(
                    selected.InstanceId);

                if (player.HasTrait(
                        PlayerTrait.Scavenger))
                {
                    player.Bank.Add(
                        selected);
                }
                else
                {
                    State.AddToDiscard(
                        selected);
                }
            }

            return true;
        }

        private Card FindHighestBankCard(
            PlayerState player)
        {
            Card highest = null;

            for (int i = 0;
                 i < player.Bank.Count;
                 i++)
            {
                Card card =
                    player.Bank.Cards[i];

                if (highest == null ||
                    card.Value > highest.Value)
                {
                    highest = card;
                }
            }

            return highest;
        }

        private bool ResolveMisfire(
            PlayerState attacker,
            PlayerState defender)
        {
            Card highest =
                FindHighestBankCard(
                    attacker);

            if (highest == null)
                return false;

            attacker.Bank.Remove(
                highest.InstanceId);

            State.AddToDiscard(
                highest);

            return true;
        }

        private bool UseHook(
            PlayerState player,
            int? cardId)
        {
            if (!cardId.HasValue)
                return false;

            Card card =
                player.Bank.Find(
                    cardId.Value);

            if (card == null)
                return false;

            if (player.HasTrait(
                    PlayerTrait.CaptainsHook))
            {
                Emit(
                    "CaptainHookRequiresSecondCard:" +
                    player.Id);
            }

            player.Bank.Remove(
                card.InstanceId);

            player.PlayArea.Add(card);

            /*
             * Miser:
             * کارت Hook و کارت‌هایی که Hook
             * وارد Play Area می‌کند ایمن هستند.
             */
            if (player.HasTrait(
                    PlayerTrait.Miser))
            {
                player.ProtectedPlayCards =
                    player.PlayArea.Count;
            }

            ResolveCardEffect(
                player,
                card);

            return true;
        }

        private bool UseSword(
            PlayerState player,
            int? targetPlayerId,
            int? targetCardId)
        {
            if (!targetPlayerId.HasValue ||
                !targetCardId.HasValue)
            {
                return false;
            }

            PlayerState target =
                State.FindPlayer(
                    targetPlayerId.Value);

            if (target == null ||
                target.Id == player.Id)
            {
                return false;
            }

            Card card =
                target.Bank.Find(
                    targetCardId.Value);

            if (card == null)
                return false;

            /*
             * Parry
             */
            if (target.HasTrait(
                    PlayerTrait.Parry) &&
                card.Type != CardType.Kraken)
            {
                Card kraken =
                    target.Bank.FindHighest(
                        CardType.Kraken);

                if (kraken != null)
                {
                    target.Bank.Remove(
                        kraken.InstanceId);

                    target.PlayArea.Add(
                        kraken);
                }

                return false;
            }

            target.Bank.Remove(
                card.InstanceId);

            player.PlayArea.Add(card);

            if (player.HasTrait(
                    PlayerTrait.Swordsman))
            {
                /*
                 * Swordsman اجازه انتخاب
                 * هر کارت را می‌دهد؛ این انتخاب
                 * قبلاً توسط targetCardId انجام شده.
                 */
            }

            ResolveCardEffect(
                player,
                card);

            return true;
        }

        private bool UseMap(
            PlayerState player,
            int? cardId)
        {
            if (!cardId.HasValue)
                return false;

            State.ShuffleDiscard(
                _random);

            Card selected =
                State.TakeDiscardCard(
                    cardId.Value);

            if (selected == null)
                return false;

            player.PlayArea.Add(
                selected);

            if (GameRules.IsBust(
                    player,
                    selected))
            {
                ResolveBust(
                    player,
                    selected);

                return true;
            }

            ResolveCardEffect(
                player,
                selected);

            return true;
        }

        private void BankCard(
            PlayerState player,
            Card card)
        {
            player.PlayArea.Remove(
                card.InstanceId);

            player.Bank.Add(card);
        }

        private bool IsCurrentPlayer(
            PlayerState player)
        {
            return player != null &&
                   !State.IsGameOver &&
                   State.Phase ==
                       GamePhase.Turn &&
                   State.CurrentPlayerId ==
                       player.Id;
        }

        private void ResetTurn()
        {
            PlayerState player =
                State.FindPlayer(
                    State.CurrentPlayerId);

            if (player != null)
                player.ClearTurnState();
        }

        private void EndTurn()
        {
            PlayerState current =
                State.FindPlayer(
                    State.CurrentPlayerId);

            if (current != null)
            {
                ResolveEndTurnEffects(
                    current);
            }

            int currentIndex =
                FindPlayerIndex(
                    State.CurrentPlayerId);

            if (currentIndex < 0)
                return;

            _nextPlayerIndex =
                (currentIndex + 1) %
                State.Players.Count;

            State.CurrentPlayerId =
                State.Players[
                    _nextPlayerIndex].Id;

            State.TurnNumber++;

            ResetTurn();

            Emit(
                "TurnStarted:" +
                State.CurrentPlayerId);
        }

        private void ResolveEndTurnEffects(
            PlayerState player)
        {
            if (player == null)
                return;

            /*
             * PlayArea در Bank شدن قبلاً
             * توسط BankPlayArea مدیریت شده است.
             *
             * این متد محل اجرای Traitها و
             * Effectهای پایان Turn است.
             */
        }

        private int FindPlayerIndex(
            int playerId)
        {
            for (int i = 0;
                 i < State.Players.Count;
                 i++)
            {
                if (State.Players[i].Id ==
                    playerId)
                {
                    return i;
                }
            }

            return -1;
        }

        private PlayerTrait[] GetAllTraits()
        {
            return new[]
            {
                PlayerTrait.Beastmaster,
                PlayerTrait.CaptainsHook,
                PlayerTrait.Casanova,
                PlayerTrait.DavyJonesLocker,
                PlayerTrait.Fisherman,
                PlayerTrait.GoldenScales,
                PlayerTrait.Miser,
                PlayerTrait.Navigator,
                PlayerTrait.MasterGunner,
                PlayerTrait.Misfire,
                PlayerTrait.Mystic,
                PlayerTrait.Parry,
                PlayerTrait.Plunderer,
                PlayerTrait.SafeHarbor,
                PlayerTrait.Scavenger,
                PlayerTrait.Swordsman,
                PlayerTrait.TreasureHunter
            };
        }

        private void Emit(
            string message)
        {
            EventProduced?.Invoke(message);
        }
    }
}