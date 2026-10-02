using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardGames.DeadManDraws.Presentation.Game;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class PlayerDetailsPanelUi : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private TMP_Text _resourceText;
        [SerializeField] private TMP_Text _cardsText;
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Close);
        }

        public void Bind(PlayerViewData player)
        {
            if (player == null)
                return;

            if (_titleText != null)
                _titleText.text = player.PlayerName;

            SetText(_scoreText, Read(player, "Score", "Points"));
            SetText(_healthText, Read(player, "Health", "HP"));
            SetText(_resourceText, Read(player, "Resource", "Mana", "Energy"));
            SetText(_cardsText, Read(player, "CardCount", "HandCount"));
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }

        private static void SetText(TMP_Text text, object value)
        {
            if (text != null)
                text.text = value == null ? string.Empty : value.ToString();
        }

        private static object Read(object source, params string[] names)
        {
            if (source == null)
                return null;

            System.Type type = source.GetType();

            foreach (string name in names)
            {
                PropertyInfo property = type.GetProperty(
                    name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (property != null)
                    return property.GetValue(source);

                FieldInfo field = type.GetField(
                    name,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                if (field != null)
                    return field.GetValue(source);
            }

            return null;
        }
    }
}
