using UnityEngine;

namespace AngelVSZombie
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "AngelVSZombie/CardDefinition")]
    public class CardDefinition : ScriptableObject
    {
        [Header("Card Info")]
        public string CardName;
        public CardType CardType;
        public int Value; // همان Value که در کد شماست
        public bool Claw;
        [TextArea] public string Description;
        public bool Immediate;
        public bool WithDead;

        [Header("Visuals")]
        public Sprite Image; // تغییر نام به Image برای هماهنگی با کد شما
    }
}