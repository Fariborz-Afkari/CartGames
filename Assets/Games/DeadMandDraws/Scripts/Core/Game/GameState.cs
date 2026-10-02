using System;
using System.Collections.Generic;
using CardGames.DeadManDraws.Core.Cards;
using CardGames.DeadManDraws.Core.Players;

namespace CardGames.DeadManDraws.Core.Game
{


    public sealed class GameState
    {
        private readonly List<PlayerState> _players;

        private readonly List<Card> _discard;

        public string GameId { get; }

        public IReadOnlyList<PlayerState> Players
        {
            get { return _players; }
        }

        public IReadOnlyList<Card> DiscardPile
        {
            get { return _discard; }
        }

        public Deck Deck { get; }

        public GamePhase Phase
        {
            get;
            internal set;
        }

        public int CurrentPlayerId
        {
            get;
            internal set;
        }

        public int TurnNumber
        {
            get;
            internal set;
        }

        public bool IsGameOver
        {
            get;
            internal set;
        }

        public int? WinnerId
        {
            get;
            internal set;
        }

        public GameState(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException(
                    "Game id cannot be empty.",
                    nameof(gameId));

            GameId = gameId;

            _players =
                new List<PlayerState>();

            _discard =
                new List<Card>();

            Deck =
                new Deck();

            Phase =
                GamePhase.Setup;

            CurrentPlayerId = -1;
            TurnNumber = 0;
            IsGameOver = false;
            WinnerId = null;
        }

        internal void AddPlayer(PlayerState player)
        {
            _players.Add(player);
        }

        public PlayerState FindPlayer(int id)
        {
            for (int i = 0;
                 i < _players.Count;
                 i++)
            {
                if (_players[i].Id == id)
                    return _players[i];
            }

            return null;
        }

        internal void AddToDiscard(Card card)
        {
            if (card != null)
                _discard.Add(card);
        }

        internal void AddToDiscard(IEnumerable<Card> cards)
        {
            if (cards == null)
                return;

            foreach (Card card in cards)
                AddToDiscard(card);
        }

        internal void ShuffleDiscard(Random random)
        {
            if (_discard.Count < 2)
                return;

            for (int i = _discard.Count - 1;
                 i > 0;
                 i--)
            {
                int j =
                    random.Next(i + 1);

                Card temp =
                    _discard[i];

                _discard[i] =
                    _discard[j];

                _discard[j] =
                    temp;
            }
        }

        internal Card DrawDiscard()
        {
            if (_discard.Count == 0)
                return null;

            int index =
                _discard.Count - 1;

            Card card =
                _discard[index];

            _discard.RemoveAt(index);

            return card;
        }

        internal List<Card>TakeTopDiscardCards(int count)
        {
            List<Card> result =
                new List<Card>();

            for (int i = 0;
                 i < count;
                 i++)
            {
                Card card =
                    DrawDiscard();

                if (card == null)
                    break;

                result.Add(card);
            }

            return result;
        }

        internal bool HasDiscard
        {
            get { return _discard.Count > 0; }
        }

        internal Card TakeDiscardCard(int instanceId)
        {
            for (int i = 0;
                 i < _discard.Count;
                 i++)
            {
                if (_discard[i].InstanceId !=
                    instanceId)
                {
                    continue;
                }

                Card card =
                    _discard[i];

                _discard.RemoveAt(i);

                return card;
            }

            return null;
        }
    }
}