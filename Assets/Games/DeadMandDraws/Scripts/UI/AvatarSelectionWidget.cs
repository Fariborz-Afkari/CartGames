using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;

namespace DeadManDraws
{
    public class AvatarSelectionWidget : MonoBehaviour
    {
        public Image AvatarImage;
        public RTLTextMeshPro NameText;      // Text یا RTLTextMeshPro
        public RTLTextMeshPro DescText;      // Text یا RTLTextMeshPro
        public Button SelectButton;

        public AvatarDefinition Bound { get; private set; }

        public void Bind(AvatarDefinition def, System.Action<AvatarDefinition> onClick)
        {
            Bound = def;
            if (AvatarImage) AvatarImage.sprite = def.Image;
            NameText.text= def.AvatarName;
            DescText.text= def.Description;

            if (SelectButton)
            {
                SelectButton.onClick.RemoveAllListeners();
                SelectButton.onClick.AddListener(() => onClick?.Invoke(def));
            }
        }
    }
}