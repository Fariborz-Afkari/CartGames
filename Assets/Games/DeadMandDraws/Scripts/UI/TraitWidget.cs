using UnityEngine;
using UnityEngine.UI;

namespace DeadManDraws
{
    public class TraitWidget : MonoBehaviour
    {
        public Image TraitImage;
        public Button Button;

        public TraitDefinition BoundTrait { get; private set; }

        public void Bind(TraitDefinition t)
        {
            BoundTrait = t;
            if (t == null) return;
            if (TraitImage) TraitImage.sprite = t.Image;
        }
    }
}