using UnityEngine;
using UnityEngine.UI;

namespace DeadManDraws
{
    public class CardWidget : MonoBehaviour
    {
        public Image CardImage;
        public Text ValueText;
        public Button Button;

        public CardDefinition BoundCard { get; private set; }

        private void Awake()
        {
            if (CardImage == null) CardImage = GetComponent<Image>();
            if (CardImage == null)
            {
                var t = transform.Find("CardImage");
                if (t) CardImage = t.GetComponent<Image>();
            }
            if (Button == null) Button = GetComponent<Button>();
            if (ValueText == null) ValueText = GetComponentInChildren<Text>(true);
        }

        public void Bind(CardDefinition c)
        {
            BoundCard = c;

            if (CardImage == null) CardImage = GetComponent<Image>();
            if (ValueText == null) ValueText = GetComponentInChildren<Text>(true);

            if (c == null)
            {
                if (CardImage) CardImage.enabled = false;
                if (ValueText) ValueText.text = "";
                return;
            }

            if (CardImage) { CardImage.enabled = true; CardImage.sprite = c.Image; }
            if (ValueText) ValueText.text = c.Value.ToString();
        }
    }
}