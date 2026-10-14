using UnityEngine;

namespace DeadManDraws
{
    [CreateAssetMenu(fileName = "AvatarDefinition", menuName = "DeadManDraws/AvatarDefinition")]
    public class AvatarDefinition : ScriptableObject
    {
        public int AvatarId;          // 1..16
        public string AvatarName;
        [TextArea] public string Description;
        public Sprite Image;
    }
}