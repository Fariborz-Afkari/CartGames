using System;
using System.Collections.Generic;
using CardGames.DeadManDraws.Core.AI;
using CardGames.DeadManDraws.Core.Cards;
using CardGames.DeadManDraws.Core.Game;
using CardGames.DeadManDraws.Core.Players;
using CardGames.DeadManDraws.Platform.Economy;
using CardGames.DeadManDraws.Platform.Iap;
using CardGames.DeadManDraws.Platform.Storage;

namespace CardGames.DeadManDraws.Presentation.Game
{
    public sealed class GamePresenter : IDisposable
    {
        private const int HumanPlayerId = 0;
        private const int PlayerCount = 4;
        private const int DeckCopies = 1;
        private const int CoinsPerPurchase = 10;
        private const int MaxTurnLogEntries = 30;

        private readonly PirateGameEngine _engine;
        private readonly GameFlowController _flowController;
        private readonly List<string> _turnLog;

        public IEconomyService Economy { get; }

        public IIapService Iap { get; }

        public event Action Changed;

        public string Status { get; private set; }

        public IReadOnlyList<string> TurnLog
        {
            get { return _turnLog; }
        }

        public GamePresenter()
        {
            _engine =
                new PirateGameEngine(
                    "PirateGame");

            Economy =
                new EconomyService(
                    new LocalPlayerData());

            Iap =
                new MockIapService();

            _flowController =
                new GameFlowController(
                    _engine,
                    new BasicAiStrategy());

            _turnLog =
                new List<string>();

            Status =
                "Ready.";

            _engine.EventProduced +=
                OnEngineEvent;
        }

        public void Dispose()
        {
            _engine.EventProduced -=
                OnEngineEvent;
        }

        public bool IsMatchRunning
        {
            get
            {
                return _engine.State.Players.Count > 0 &&
                       !_engine.State.IsGameOver;
            }
        }

        public bool CanStartMatch
        {
            get
            {
                return Coins >= 1 &&
                       !IsMatchRunning;
            }
        }

        public int Coins
        {
            get { return Economy.Coins; }
        }

        public bool IsPlayerTurn
        {
            get
            {
                return IsMatchRunning &&
                       _engine.State.CurrentPlayerId ==
                       HumanPlayerId;
            }
        }

        public bool IsTraitSelection
        {
            get
            {
                return _engine.State.Phase ==
                       GamePhase.TraitSelection;
            }
        }

        public bool IsGameOver
        {
            get { return _engine.State.IsGameOver; }
        }

        public int? WinnerId
        {
            get { return _engine.State.WinnerId; }
        }

        public string CurrentPlayerName
        {
            get
            {
                PlayerState player =
                    _engine.State.FindPlayer(
                        _engine.State.CurrentPlayerId);

                return player != null
                    ? player.Name
                    : string.Empty;
            }
        }

        public int CurrentPlayerId
        {
            get
            {
                return _engine.State.CurrentPlayerId;
            }
        }

        public int TurnNumber
        {
            get { return _engine.State.TurnNumber; }
        }

        public int DeckCount
        {
            get { return _engine.State.Deck.Count; }
        }

        public int DiscardCount
        {
            get { return _engine.State.DiscardPile.Count; }
        }

        public IReadOnlyList<PlayerTrait>
            GetHumanTraitOptions()
        {
            PlayerState player =
                _engine.State.FindPlayer(
                    HumanPlayerId);

            if (player == null)
                return Array.Empty<PlayerTrait>();

            return player.TraitOptions;
        }

        public PlayerTrait HumanTrait
        {
            get
            {
                PlayerState player =
                    _engine.State.FindPlayer(
                        HumanPlayerId);

                return player != null
                    ? player.Trait
                    : PlayerTrait.None;
            }
        }

        public void StartMatch()
        {
            if (!CanStartMatch)
            {
                Status =
                    Coins < 1
                        ? "Not enough coins."
                        : "Cannot start a new match.";

                NotifyChanged();
                return;
            }

            string error;

            if (!Economy.TryStartMatch(
                    out error))
            {
                Status =
                    string.IsNullOrEmpty(error)
                        ? "Cannot start match."
                        : error;

                NotifyChanged();
                return;
            }

            _turnLog.Clear();

            _engine.StartMatch(
                PlayerCount,
                DeckCopies);

            Status =
                "Choose your Trait.";

            NotifyChanged();

            /*
             * AI trait choices are resolved here.
             * Human choice remains visible through
             * GetHumanTraitOptions().
             */
            _flowController.RunAiTurns();

            NotifyChanged();
        }

        public bool SelectTrait(
            PlayerTrait trait)
        {
            if (!IsTraitSelection)
                return false;

            bool result =
                _engine.SubmitAction(
                    GameAction.SelectTrait(
                        HumanPlayerId,
                        trait));

            if (result)
            {
                Status =
                    "Trait selected.";
            }
            else
            {
                Status =
                    "Invalid Trait selection.";
            }

            if (result)
            {
                _flowController.RunAiTurns();
            }

            NotifyChanged();

            return result;
        }

        public IReadOnlyList<PlayerViewData>
            GetPlayers()
        {
            List<PlayerViewData> result =
                new List<PlayerViewData>();

            for (int i = 0;
                 i < _engine.State.Players.Count;
                 i++)
            {
                PlayerState player =
                    _engine.State.Players[i];

                result.Add(
                    new PlayerViewData(
                        player.Id,
                        player.Name,
                        player.IsHuman,
                        GameRules.CalculateScore(
                            player),
                        player.Bank.Count,
                        player.PlayArea.Count,
                        player.Trait));
            }

            return result;
        }

        public IReadOnlyList<CardViewData>
            GetPlayerPlayArea()
        {
            PlayerState player =
                _engine.State.FindPlayer(
                    HumanPlayerId);

            if (player == null)
                return Array.Empty<CardViewData>();

            List<CardViewData> result =
                new List<CardViewData>();

            for (int i = 0;
                 i < player.PlayArea.Count;
                 i++)
            {
                Card card =
                    player.PlayArea.Cards[i];

                if (card == null ||
                    card.Definition == null)
                {
                    continue;
                }

                result.Add(
                    CreateCardViewData(card));
            }

            return result;
        }

        public IReadOnlyList<CardViewData>
            GetPlayerBank()
        {
            PlayerState player =
                _engine.State.FindPlayer(
                    HumanPlayerId);

            if (player == null)
                return Array.Empty<CardViewData>();

            List<CardViewData> result =
                new List<CardViewData>();

            for (int i = 0;
                 i < player.Bank.Count;
                 i++)
            {
                Card card =
                    player.Bank.Cards[i];

                if (card == null ||
                    card.Definition == null)
                {
                    continue;
                }

                result.Add(
                    CreateCardViewData(card));
            }

            return result;
        }

        public IReadOnlyList<CardViewData>
            GetPlayerHand()
        {
            /*
             * Compatibility name for the current UI.
             *
             * The new game has no traditional Hand.
             * Cards being displayed to the player are
             * the current PlayArea cards.
             */
            return GetPlayerPlayArea();
        }

        public CardInteraction GetCardInteraction(
            int cardId)
        {
            PlayerState player =
                _engine.State.FindPlayer(
                    HumanPlayerId);

            if (player == null)
            {
                return new CardInteraction(
                    cardId,
                    false,
                    Array.Empty<TargetViewData>());
            }

            Card card =
                player.PlayArea.Find(cardId);

            if (card == null)
            {
                return new CardInteraction(
                    cardId,
                    false,
                    Array.Empty<TargetViewData>());
            }

            List<TargetViewData> targets =
                new List<TargetViewData>();

            switch (card.Type)
            {
                case CardType.Cannon:
                case CardType.Sword:

                    for (int i = 0;
                         i < _engine.State.Players.Count;
                         i++)
                    {
                        PlayerState target =
                            _engine.State.Players[i];

                        if (target.Id ==
                            player.Id)
                        {
                            continue;
                        }

                        targets.Add(
                            new TargetViewData(
                                target.Id,
                                target.Name));
                    }

                    return new CardInteraction(
                        cardId,
                        targets.Count > 0,
                        targets);

                default:

                    return new CardInteraction(
                        cardId,
                        false,
                        targets);
            }
        }

        public bool DrawCard()
        {
            if (!IsPlayerTurn)
            {
                Status =
                    "It is not your turn.";

                NotifyChanged();
                return false;
            }

            bool result =
                _engine.SubmitAction(
                    GameAction.DrawCard(
                        HumanPlayerId));

            Status =
                result
                    ? "Card drawn."
                    : "Cannot draw a card.";

            NotifyChanged();

            return result;
        }

        public bool StopDrawing()
        {
            if (!IsPlayerTurn)
            {
                Status =
                    "It is not your turn.";

                NotifyChanged();
                return false;
            }

            bool result =
                _engine.SubmitAction(
                    GameAction.StopDrawing(
                        HumanPlayerId));

            if (result)
            {
                _flowController.RunAiTurns();

                Status =
                    IsGameOver
                        ? GetGameOverStatus()
                        : "Turn ended.";
            }
            else
            {
                Status =
                    "Cannot end drawing.";
            }

            NotifyChanged();

            return result;
        }

        /*
         * Compatibility method for the current UI.
         *
         * Card effects such as Cannon/Sword require
         * explicit target/card selection and are handled
         * through the corresponding Core actions.
         */
        public bool PlayCard(
            int cardId,
            int? targetPlayerId = null)
        {
            CardInteraction interaction =
                GetCardInteraction(cardId);

            if (interaction == null)
                return false;

            if (!interaction.RequiresTarget)
            {
                Status =
                    "Draw or stop drawing to resolve cards.";

                NotifyChanged();
                return false;
            }

            if (!targetPlayerId.HasValue)
                return false;

            PlayerState player =
                _engine.State.FindPlayer(
                    HumanPlayerId);

            if (player == null)
                return false;

            Card card =
                player.PlayArea.Find(cardId);

            if (card == null)
                return false;

            bool result = false;

            if (card.Type ==
                CardType.Cannon)
            {
                result =
                    _engine.SubmitAction(
                        GameAction.Cannon(
                            HumanPlayerId,
                            targetPlayerId.Value));
            }

            if (result)
            {
                _flowController.RunAiTurns();
            }

            Status =
                result
                    ? "Card action resolved."
                    : "Card action failed.";

            NotifyChanged();

            return result;
        }

        public void EndTurn()
        {
            StopDrawing();
        }

        public void BuyCoins()
        {
            Iap.PurchaseCoins(
                CoinsPerPurchase,
                success =>
                {
                    if (!success)
                    {
                        Status =
                            "Purchase failed.";

                        NotifyChanged();
                        return;
                    }

                    Economy.GrantCoins(
                        CoinsPerPurchase);

                    Status =
                        "Coins purchased.";

                    NotifyChanged();
                });
        }

        private CardViewData CreateCardViewData(
            Card card)
        {
            return new CardViewData(
                card.InstanceId,
                card.Definition.Name,
                card.Value,
                card.Type,
                GetDescription(card.Type));
        }

        private static string GetDescription(
            CardType type)
        {
            switch (type)
            {
                case CardType.Anchor:
                    return "Protects cards before it.";

                case CardType.Cannon:
                    return "Burn a card from an opponent's Bank.";

                case CardType.Chest:
                    return "A treasure card.";

                case CardType.Hook:
                    return "Take a card from your Bank.";

                case CardType.Key:
                    return "Used with treasure.";

                case CardType.Kraken:
                    return "Draw additional cards.";

                case CardType.Map:
                    return "Choose from the discard pile.";

                case CardType.Mermaid:
                    return "A valuable treasure card.";

                case CardType.Oracle:
                    return "Preview upcoming cards.";

                case CardType.Sword:
                    return "Take a card from an opponent.";

                default:
                    return string.Empty;
            }
        }

        private string GetGameOverStatus()
        {
            if (!WinnerId.HasValue)
                return "Match ended.";

            PlayerState winner =
                _engine.State.FindPlayer(
                    WinnerId.Value);

            return winner == null
                ? "Match ended."
                : winner.Name + " wins.";
        }

        private void OnEngineEvent(
            string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            AddTurnLog(
                FormatEvent(message));

            NotifyChanged();
        }

        private string FormatEvent(
            string message)
        {
            string[] parts =
                message.Split(':');

            if (parts.Length == 0)
                return message;

            if (parts[0] ==
                "CardDrawn" &&
                parts.Length >= 3)
            {
                PlayerState player =
                    _engine.State.FindPlayer(
                        ParseInt(parts[1]));

                Card card =
                    FindCardById(
                        ParseInt(parts[2]));

                if (player != null &&
                    card != null)
                {
                    return player.Name +
                           " drew " +
                           card.Definition.Name +
                           " (" +
                           card.Value +
                           ").";
                }
            }

            if (parts[0] ==
                "TurnStarted" &&
                parts.Length >= 2)
            {
                PlayerState player =
                    _engine.State.FindPlayer(
                        ParseInt(parts[1]));

                return player == null
                    ? "Turn started."
                    : player.Name +
                      "'s turn.";
            }

            if (parts[0] ==
                "TraitSelected" &&
                parts.Length >= 3)
            {
                PlayerState player =
                    _engine.State.FindPlayer(
                        ParseInt(parts[1]));

                return player == null
                    ? "Trait selected."
                    : player.Name +
                      " selected " +
                      parts[2] +
                      ".";
            }

            return message;
        }

        private Card FindCardById(
            int id)
        {
            for (int i = 0;
                 i < _engine.State.DiscardPile.Count;
                 i++)
            {
                Card card =
                    _engine.State.DiscardPile[i];

                if (card.InstanceId == id)
                    return card;
            }

            for (int i = 0;
                 i < _engine.State.Players.Count;
                 i++)
            {
                PlayerState player =
                    _engine.State.Players[i];

                Card card =
                    player.PlayArea.Find(id);

                if (card != null)
                    return card;

                card =
                    player.Bank.Find(id);

                if (card != null)
                    return card;
            }

            return null;
        }

        private static int ParseInt(
            string value)
        {
            int result;

            if (int.TryParse(
                    value,
                    out result))
            {
                return result;
            }

            return -1;
        }

        private void AddTurnLog(
            string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            _turnLog.Add(message);

            while (_turnLog.Count >
                   MaxTurnLogEntries)
            {
                _turnLog.RemoveAt(0);
            }
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}