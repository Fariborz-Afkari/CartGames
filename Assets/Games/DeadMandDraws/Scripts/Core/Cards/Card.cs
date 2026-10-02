using System;

namespace CardGames.DeadManDraws.Core.Cards
{
    public sealed class Card
    {
        public int InstanceId { get; }

        public CardDefinition Definition { get; }

        public int Value { get; }

        public CardType Type
        {
            get { return Definition.Type; }
        }

        public Card(
            int instanceId,
            CardDefinition definition,
            int value)
        {
            if (definition == null)
                throw new ArgumentNullException(
                    nameof(definition));

            if (instanceId < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(instanceId));

            if (value <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(value));

            InstanceId = instanceId;
            Definition = definition;
            Value = value;
        }

        public override string ToString()
        {
            return Definition.Name + " (" + Value + ")";
        }
    }
}