using System;
using System.Collections.Generic;

namespace CardGames.DeadManDraws.Core.Cards
{
    /// <summary>
    /// Represents a deck of concrete card instances.
    ///
    /// Deck owns cards that have not yet been drawn.
    /// It does not know anything about players or game rules.
    /// </summary>
    public sealed class Deck
    {
        private readonly List<Card> _cards;

        public int Count
        {
            get { return _cards.Count; }
        }

        public bool IsEmpty
        {
            get { return _cards.Count == 0; }
        }

        public Deck()
        {
            _cards = new List<Card>();
        }

        public void Add(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            _cards.Add(card);
        }

        public void AddRange(IEnumerable<Card> cards)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            foreach (Card card in cards)
                Add(card);
        }

        /// <summary>
        /// Randomizes the order of the cards in the deck.
        /// </summary>
        public void Shuffle(Random random = null)
        {
            if (_cards.Count < 2)
                return;

            random = random ?? new Random();

            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);

                Card temporary = _cards[i];
                _cards[i] = _cards[j];
                _cards[j] = temporary;
            }
        }

        /// <summary>
        /// Draws the top card from the deck.
        ///
        /// The last item is treated as the top of the deck.
        /// </summary>
        public Card Draw()
        {
            if (_cards.Count == 0)
                return null;

            int lastIndex = _cards.Count - 1;

            Card card = _cards[lastIndex];

            _cards.RemoveAt(lastIndex);

            return card;
        }

        public void Clear()
        {
            _cards.Clear();
        }

        public bool Contains(int instanceId)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].InstanceId == instanceId)
                    return true;
            }

            return false;
        }
    }
}
