using System.Collections.Generic;
using CardGames.DeadManDraws.Core.Cards;

namespace CardGames.DeadManDraws.Core.Game
{
    public static class PirateCardFactory
    {
        public static List<Card> CreateDeck(
            int copies,
            out List<Card> initialDiscard)
        {
            if (copies <= 0)
                copies = 1;

            List<Card> deck =
                new List<Card>();

            initialDiscard =
                new List<Card>();

            int instanceId = 0;

            CardType[] types =
            {
                CardType.Anchor,
                CardType.Cannon,
                CardType.Chest,
                CardType.Hook,
                CardType.Key,
                CardType.Kraken,
                CardType.Map,
                CardType.Mermaid,
                CardType.Oracle,
                CardType.Sword
            };

            for (int copy = 0;
                 copy < copies;
                 copy++)
            {
                for (int i = 0;
                     i < types.Length;
                     i++)
                {
                    CardType type =
                        types[i];

                    int minimum =
                        type == CardType.Mermaid
                            ? 4
                            : 2;

                    int maximum =
                        type == CardType.Mermaid
                            ? 9
                            : 7;

                    for (int value = minimum;
                         value <= maximum;
                         value++)
                    {
                        CardDefinition definition =
                            CreateDefinition(type);

                        Card card =
                            new Card(
                                instanceId++,
                                definition,
                                value);

                        if (value == minimum)
                            initialDiscard.Add(card);
                        else
                            deck.Add(card);
                    }
                }
            }

            return deck;
        }

        private static CardDefinition CreateDefinition(
            CardType type)
        {
            return new CardDefinition(
                type.ToString(),
                GetName(type),
                type);
        }

        private static string GetName(
            CardType type)
        {
            switch (type)
            {
                case CardType.Anchor:
                    return "Anchor";

                case CardType.Cannon:
                    return "Cannon";

                case CardType.Chest:
                    return "Chest";

                case CardType.Hook:
                    return "Hook";

                case CardType.Key:
                    return "Key";

                case CardType.Kraken:
                    return "Kraken";

                case CardType.Map:
                    return "Map";

                case CardType.Mermaid:
                    return "Mermaid";

                case CardType.Oracle:
                    return "Oracle";

                case CardType.Sword:
                    return "Sword";

                default:
                    return type.ToString();
            }
        }
    }
}