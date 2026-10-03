using CardGames.DeadManDraws.Core.Cards;

namespace CardGames.DeadManDraws.Presentation.Game
{
    public sealed class CardViewData
    {
        public int InstanceId { get; }

        public string Name { get; }

        public int Value { get; }

        public CardType Type { get; }

        public string Description { get; }

        public bool RequiresTarget
        {
            get
            {
                return Type == CardType.Cannon ||
                       Type == CardType.Hook ||
                       Type == CardType.Map ||
                       Type == CardType.Oracle ||
                       Type == CardType.Sword;
            }
        }

        public CardViewData(
            int instanceId,
            string name,
            int value,
            CardType type,
            string description)
        {
            InstanceId = instanceId;
            Name = name;
            Value = value;
            Type = type;
            Description = description;
        }
    }
}