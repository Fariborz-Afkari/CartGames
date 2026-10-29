using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;

namespace AngelVSZombie
{
    public class CardWidget : MonoBehaviour
    {
        public Image CardImage;
        public RTLTextMeshPro ValueText;
        public Button Button;
        public GameObject CardBackVisual; // آبجکتی برای نمایش پشت کارت (اختیاری)

        public CardDefinition BoundCard { get; private set; }
        private GameObject _root;

        private void Awake()
        {
            InitReferences();
        }

        private void InitReferences()
        {
            if (_root == null)
                _root = transform.parent != null ? transform.parent.gameObject : gameObject;

            if (CardImage == null) CardImage = _root.GetComponent<Image>();
            if (CardImage == null) CardImage = GetComponent<Image>();
            if (CardImage == null) CardImage = GetComponentInChildren<Image>(true);

            if (Button == null) Button = _root.GetComponent<Button>();
            if (Button == null) Button = GetComponent<Button>();
            if (Button == null) Button = GetComponentInChildren<Button>(true);

            if (ValueText == null) ValueText = _root.GetComponentInChildren<RTLTextMeshPro>(true);
            if (ValueText == null) ValueText = GetComponentInChildren<RTLTextMeshPro>(true);
        }

        public void Show(bool show)
        {
            if (_root != null) _root.SetActive(show);
            else gameObject.SetActive(show);
        }

        // متد جدید برای پشت و رو کردن کارت
        public void SetFaceDown(bool faceDown)
        {
            if (CardImage != null) CardImage.enabled = !faceDown;
            if (ValueText != null) ValueText.gameObject.SetActive(!faceDown);

            // اگر آبجکتی برای پشت کارت تعریف کرده باشید، آن را فعال می‌کند
            if (CardBackVisual != null) CardBackVisual.SetActive(faceDown);
        }

        public void Bind(CardDefinition c, int count = 1)
        {
            BoundCard = c;
            SetFaceDown(false); // به طور پیش‌فرض رو باشد

            if (c == null)
            {
                if (CardImage != null) CardImage.enabled = false;
                if (ValueText != null) ValueText.text = "";
                return;
            }

            if (CardImage != null)
            {
                CardImage.enabled = true;
                CardImage.sprite = c.Image;
                CardImage.preserveAspect = true;
                CardImage.color = Color.white;
            }

            if (ValueText != null)
            {
                ValueText.text = $"{ToPersianNumber(count)}";
            }
        }

        private string ToPersianNumber(int number)
        {
            string english = number.ToString();
            string persian = "";
            foreach (char c in english)
            {
                if (c >= '0' && c <= '9')
                    persian += char.ConvertFromUtf32(c - '0' + '۰');
                else
                    persian += c;
            }
            return persian;
        }
    }
}