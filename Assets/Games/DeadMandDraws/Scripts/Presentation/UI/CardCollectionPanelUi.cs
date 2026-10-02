using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardGames.DeadManDraws.Presentation.Game;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class CardCollectionPanelUi : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Transform _content;
        [SerializeField] private Button _closeButton;
        [SerializeField] private CardWidget _fallbackCardPrefab;

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

        public void Bind(
            string title,
            IReadOnlyList<CardViewData> cards,
            CardWidget preferredPrefab)
        {
            if (_titleText != null)
                _titleText.text = title;

            Clear();

            CardWidget prefab =
                preferredPrefab != null
                    ? preferredPrefab
                    : _fallbackCardPrefab;

            if (prefab == null || _content == null || cards == null)
                return;

            for (int i = 0; i < cards.Count; i++)
            {
                CardWidget card =
                    Instantiate(prefab, _content);

                card.Bind(cards[i], _ => { });
            }
        }

        private void Clear()
        {
            if (_content == null)
                return;

            for (int i = _content.childCount - 1; i >= 0; i--)
                Destroy(_content.GetChild(i).gameObject);
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
