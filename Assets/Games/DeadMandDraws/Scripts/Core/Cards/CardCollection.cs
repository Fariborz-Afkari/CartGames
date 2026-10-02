using System;
using System.Collections.Generic;

namespace CardGames.DeadManDraws.Core.Cards
{
    public sealed class CardCollection
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

        public CardCollection()
        {
            _cards = new List<Card>();
        }

        public void Add(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(
                    nameof(card));

            _cards.Add(card);
        }

        public void AddRange(
            IEnumerable<Card> cards)
        {
            if (cards == null)
                throw new ArgumentNullException(
                    nameof(cards));

            foreach (Card card in cards)
                Add(card);
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

        public Card RemoveAt(int index)
        {
            if (index < 0 ||
                index >= _cards.Count)
            {
                return null;
            }

            Card card = _cards[index];

            _cards.RemoveAt(index);

            return card;
        }

        public void Clear()
        {
            _cards.Clear();
        }

        public bool ContainsType(CardType type)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].Type == type)
                    return true;
            }

            return false;
        }

        public Card FindHighest(CardType type)
        {
            Card result = null;

            for (int i = 0; i < _cards.Count; i++)
            {
                Card card = _cards[i];

                if (card.Type != type)
                    continue;

                if (result == null ||
                    card.Value > result.Value)
                {
                    result = card;
                }
            }

            return result;
        }

        public List<Card> FindAll(CardType type)
        {
            List<Card> result =
                new List<Card>();

            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].Type == type)
                    result.Add(_cards[i]);
            }

            return result;
        }
    }
}