using System;
using CardGames.DeadManDraws.Presentation.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class CardWidget : MonoBehaviour
    {
        [SerializeField]
        private Button _button;

        [SerializeField]
        private TMP_Text _nameText;

        [SerializeField]
        private TMP_Text _valueText;

        [SerializeField]
        private TMP_Text _descriptionText;

        [SerializeField]
        private TMP_Text _typeText;

        private int _cardId;
        private Action<int> _onClicked;

        public void Bind(
            CardViewData card,
            Action<int> onClicked)
        {
            if (card == null)
                return;

            _cardId = card.InstanceId;
            _onClicked = onClicked;

            if (_nameText != null)
            {
                _nameText.text =
                    card.Name.ToUpperInvariant();
            }

            if (_valueText != null)
            {
                _valueText.text =
                    card.Value.ToString();
            }

            if (_descriptionText != null)
            {
                _descriptionText.text =
                    card.Description;
            }

            if (_typeText != null)
            {
                _typeText.text =
                    card.Type
                        .ToString()
                        .ToUpperInvariant();
            }

            if (_button != null)
            {
                _button.onClick.RemoveListener(
                    OnClicked);

                _button.onClick.AddListener(
                    OnClicked);
            }
        }

        private void OnClicked()
        {
            if (_onClicked != null)
            {
                _onClicked(_cardId);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(
                    OnClicked);
            }

            _onClicked = null;
        }
    }
}