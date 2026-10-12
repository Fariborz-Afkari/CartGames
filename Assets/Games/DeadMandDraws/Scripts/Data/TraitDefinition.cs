using UnityEngine;

namespace DeadManDraws
{
    [CreateAssetMenu(fileName = "TraitDefinition", menuName = "DeadManDraws/TraitDefinition")]
    public class TraitDefinition : ScriptableObject
    {
        public int TraitId;
        public Sprite Image;
        public TraitType TraitType;
        [TextArea] public string Description;
        public string Name;
    }
}