using System;
using System.Collections.Generic;
using CardGames.DeadManDraws.Presentation.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class PlayerPanelUi : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private Image _avatarImage;

        [Header("Turn")]
        [SerializeField] private GameObject _turnGlow;

        private Button _button;

        public event Action Clicked;

        private void Awake()
        {
            ResolveReferences();

            if (_button != null)
            {
                _button.onClick.RemoveListener(
                    OnClicked);

                _button.onClick.AddListener(
                    OnClicked);
            }
        }

        private void ResolveReferences()
        {
            TMP_Text[] texts =
                GetComponentsInChildren<TMP_Text>(
                    true);

            if (_nameText == null &&
                texts.Length > 0)
            {
                _nameText = texts[0];
            }

            if (_scoreText == null &&
                texts.Length > 1)
            {
                _scoreText = texts[1];
            }

            if (_avatarImage == null)
            {
                Transform avatar =
                    FindChildRecursive(
                        transform,
                        "Avatar");

                if (avatar != null)
                {
                    _avatarImage =
                        avatar.GetComponent<Image>();
                }
            }

            if (_turnGlow == null)
            {
                Transform glow =
                    FindChildRecursive(
                        transform,
                        "TurnGlow");

                if (glow != null)
                    _turnGlow = glow.gameObject;
            }

            _button =
                GetComponent<Button>();
        }

        public void Bind(
            PlayerViewData player,
            bool isCurrentTurn,
            Sprite avatarSprite = null)
        {
            if (player == null)
                return;

            if (_nameText != null)
            {
                _nameText.text =
                    player.PlayerName;
            }

            if (_scoreText != null)
            {
                _scoreText.text =
                    player.Score.ToString();
            }

            if (_avatarImage != null &&
                avatarSprite != null)
            {
                _avatarImage.sprite =
                    avatarSprite;

                _avatarImage.enabled = true;
            }

            SetTurn(isCurrentTurn);
        }

        public void SetTurn(
            bool isCurrentTurn)
        {
            if (_turnGlow != null)
                _turnGlow.SetActive(
                    isCurrentTurn);
        }

        private void OnClicked()
        {
            Clicked?.Invoke();
        }

        private static Transform FindChildRecursive(
            Transform parent,
            string childName)
        {
            if (parent == null)
                return null;

            if (parent.name == childName)
                return parent;

            for (int i = 0;
                 i < parent.childCount;
                 i++)
            {
                Transform result =
                    FindChildRecursive(
                        parent.GetChild(i),
                        childName);

                if (result != null)
                    return result;
            }

            return null;
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(
                    OnClicked);
            }

            Clicked = null;
        }
    }
}