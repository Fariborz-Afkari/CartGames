using UnityEngine;

namespace AngelVSZombie
{
    [CreateAssetMenu(fileName = "NewPlayer", menuName = "AngelVSZombie/PlayerDefinition")]
    public class PlayerDefinition : ScriptableObject
    {
        public string PlayerName;
        public int AvatarIndex;
        public bool IsHuman;
    }
}