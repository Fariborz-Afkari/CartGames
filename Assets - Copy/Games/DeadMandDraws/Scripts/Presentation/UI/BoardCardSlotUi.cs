using CardGames.DeadManDraws.Presentation.Game;
using CardGames.DeadManDraws.Presentation.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class BoardCardSlotUi : MonoBehaviour
    {
        [SerializeField] private RectTransform _cardAnchor;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _selectionHighlight;

        private CardWidget _cardWidget;
        private CardViewData _card;
        private Action<int> _clicked;
        private int _slotIndex;

        public RectTransform CardAnchor =>
            _cardAnchor != null
                ? _cardAnchor
                : transform as RectTransform;

        public bool HasCard => _cardWidget != null;

        public void Initialize(int slotIndex)
        {
            _slotIndex = slotIndex;

            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClicked);
                _button.onClick.AddListener(OnClicked);
            }
        }

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClicked);
        }

        public void ShowCard(
            CardViewData card,
            Action<int> clicked)
        {
            Clear();

            _card = card;
            _clicked = clicked;

            if (_emptyState != null)
                _emptyState.SetActive(card == null);
        }

        public void Attach(CardWidget widget, CardViewData card)
        {
            Clear();

            _cardWidget = widget;
            _card = card;

            if (_emptyState != null)
                _emptyState.SetActive(false);

            if (_cardWidget != null)
                _cardWidget.transform.SetParent(CardAnchor, false);
        }

        public void Clear()
        {
            if (_cardWidget != null)
                Destroy(_cardWidget.gameObject);

            _cardWidget = null;
            _card = null;

            if (_emptyState != null)
                _emptyState.SetActive(true);

            SetHighlight(false);
        }

        public void SetHighlight(bool active)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(active);
        }

        public void OpenDetail()
        {
            // Reserved for card-detail popup.
            // Keep this method so board cards are independently clickable.
        }

        private void OnClicked()
        {
            _clicked?.Invoke(_slotIndex);
        }
    }
}
