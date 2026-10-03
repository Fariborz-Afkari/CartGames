using System;
using System.Collections.Generic;

namespace CardGames.DeadManDraws.Core.Cards
{
    /// <summary>
    /// Represents the cards currently held by a player.
    ///
    /// Hand owns cards after they have been drawn from a deck.
    /// </summary>
    public sealed class Hand
    {
        private readonly List<Card> _cards;

        public int Count
        {
            get { return _cards.Count; }
        }

        public IReadOnlyList<Card> Cards
        {
            get { return _cards; }
        }

        public Hand()
        {
            _cards = new List<Card>();
        }

        public void Add(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            _cards.Add(card);
        }

        public bool Remove(int instanceId)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].InstanceId != instanceId)
                    continue;

                _cards.RemoveAt(i);
                return true;
            }

            return false;
        }

        public Card Find(int instanceId)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].InstanceId == instanceId)
                    return _cards[i];
            }

            return null;
        }

        public bool Contains(int instanceId)
        {
            return Find(instanceId) != null;
        }

        public void Clear()
        {
            _cards.Clear();
        }
    }
}