using UnityEngine;
using UnityEngine.UI;

namespace DeadManDraws
{
    public class PlayerPanelWidget : MonoBehaviour
    {
        [Header("State Image")]
        public Image StateImage;

        [Header("Sprites")]
        public Sprite NormalSprite;
        public Sprite ActiveSprite;

        public void SetActive(bool isActive)
        {
            if (StateImage == null) return;
            var s = isActive ? ActiveSprite : NormalSprite;
            if (s != null) StateImage.sprite = s;
        }
    }
}