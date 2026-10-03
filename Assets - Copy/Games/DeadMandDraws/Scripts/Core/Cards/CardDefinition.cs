using System;

namespace CardGames.DeadManDraws.Core.Cards
{
    public sealed class CardDefinition
    {
        public string Id { get; }

        public string Name { get; }

        public CardType Type { get; }

        public CardDefinition(
            string id,
            string name,
            CardType type)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "Card id cannot be empty.",
                    nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Card name cannot be empty.",
                    nameof(name));

            Id = id;
            Name = name;
            Type = type;
        }
    }
}