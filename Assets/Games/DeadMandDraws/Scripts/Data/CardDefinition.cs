using UnityEngine;

namespace DeadManDraws
{
    [CreateAssetMenu(fileName = "CardDefinition", menuName = "DeadManDraws/CardDefinition")]
    public class CardDefinition : ScriptableObject
    {
        public int CardId;
        public Sprite Image;
        public CardType CardType;
        public int Value;
        [TextArea] public string Description;
        public string Name;
    }
}