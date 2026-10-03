using System;

namespace CardGames.DeadManDraws.Core.Game
{
    /// <summary>
    /// Represents a domain event produced by the game engine.
    ///
    /// GameEvent describes something that has already happened.
    /// </summary>
    public readonly struct GameEvent
    {
        public GameEventType Type { get; }

        public int PlayerId { get; }

        public int? CardId { get; }

        public int? TargetPlayerId { get; }

        public string Message { get; }

        private GameEvent(
            GameEventType type,
            int playerId = -1,
            int? cardId = null,
            int? targetPlayerId = null,
            string message = null)
        {
            Type = type;
            PlayerId = playerId;
            CardId = cardId;
            TargetPlayerId = targetPlayerId;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// Creates a MatchStarted event.
        /// </summary>
        public static GameEvent MatchStarted(string message = null)
        {
            return new GameEvent(
                GameEventType.MatchStarted,
                message: message ?? "Match started.");
        }

        /// <summary>
        /// Creates a TurnStarted event.
        /// </summary>
        public static GameEvent TurnStarted(
            int playerId,
            string message = null)
        {
            return new GameEvent(
                GameEventType.TurnStarted,
                playerId: playerId,
                message: message ?? "Turn started.");
        }

        /// <summary>
        /// Creates a CardDrawn event.
        /// </summary>
        public static GameEvent CardDrawn(
            int playerId,
            int cardId,
            string message = null)
        {
            return new GameEvent(
                GameEventType.CardDrawn,
                playerId: playerId,
                cardId: cardId,
                message: message ?? "Card drawn.");
        }

        /// <summary>
        /// Creates a CardPlayed event.
        /// </summary>
        public static GameEvent CardPlayed(
            int playerId,
            int cardId,
            int? targetPlayerId = null,
            string message = null)
        {
            return new GameEvent(
                GameEventType.CardPlayed,
                playerId: playerId,
                cardId: cardId,
                targetPlayerId: targetPlayerId,
                message: message ?? "Card played.");
        }

        /// <summary>
        /// Creates a TurnEnded event.
        /// </summary>
        public static GameEvent TurnEnded(
            int playerId,
            string message = null)
        {
            return new GameEvent(
                GameEventType.TurnEnded,
                playerId: playerId,
                message: message ?? "Turn ended.");
        }

        /// <summary>
        /// Creates a MatchEnded event.
        /// </summary>
        public static GameEvent MatchEnded(
            int? winnerId = null,
            string message = null)
        {
            return new GameEvent(
                GameEventType.MatchEnded,
                playerId: winnerId ?? -1,
                message: message ?? "Match ended.");
        }

        /// <summary>
        /// Creates a CoinConsumed event.
        /// </summary>
        public static GameEvent CoinConsumed(
            int playerId,
            string message = null)
        {
            return new GameEvent(
                GameEventType.CoinConsumed,
                playerId: playerId,
                message: message ?? "Coin consumed.");
        }

        /// <summary>
        /// Creates an ActionRejected event.
        /// </summary>
        public static GameEvent ActionRejected(
            int playerId,
            string message)
        {
            return new GameEvent(
                GameEventType.ActionRejected,
                playerId: playerId,
                message: message ?? "Action rejected.");
        }

        public override string ToString()
        {
            return Message;
        }
    }

    /// <summary>
    /// Identifies the type of a game event.
    /// </summary>
    public enum GameEventType
    {
        MatchStarted = 0,
        TurnStarted = 1,
        CardDrawn = 2,
        CardPlayed = 3,
        TurnEnded = 4,
        MatchEnded = 5,
        CoinConsumed = 6,
        ActionRejected = 7
    }
}
