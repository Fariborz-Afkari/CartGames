using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardGames.DeadManDraws.Presentation.Game;

namespace CardGames.DeadManDraws.Presentation.UI
{
    /// <summary>
    /// Optional visual binder for the CardWidget prefab.
    ///
    /// It intentionally uses reflection for the card fields so the UI does
    /// not force a specific CardViewData property naming convention.
    ///
    /// Supported common names:
    /// background/sprite/artwork, value/points, type/cardType.
    /// </summary>
    public sealed class CardVisualBinder : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private TMP_Text _typeText;

        public void Bind(CardViewData card)
        {
            if (card == null)
                return;

            object spriteValue =
                Read(card, "Background", "BackgroundSprite",
                    "Sprite", "Artwork", "Art");

            if (_background != null &&
                spriteValue is Sprite sprite)
            {
                _background.sprite = sprite;
                _background.enabled = true;
            }

            object value =
                Read(card, "Value", "Points", "Score");

            if (_valueText != null)
                _valueText.text =
                    value == null ? string.Empty : value.ToString();

            object type =
                Read(card, "Type", "CardType");

            if (_typeText != null)
                _typeText.text =
                    type == null ? string.Empty : type.ToString();
        }

        private static object Read(
            object source,
            params string[] names)
        {
            Type type = source.GetType();

            foreach (string name in names)
            {
                PropertyInfo property =
                    type.GetProperty(
                        name,
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic);

                if (property != null)
                    return property.GetValue(source);

                FieldInfo field =
                    type.GetField(
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
