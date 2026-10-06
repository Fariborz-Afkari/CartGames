using System;
using CardGames.DeadManDraws.Presentation.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class PlayerPanelUi : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField]
        private TMP_Text _nameText;

        [SerializeField]
        private TMP_Text _scoreText;

        [SerializeField]
        private Image _avatarImage;

        [Header("Turn")]
        [SerializeField]
        private GameObject _turnGlow;

        private Button _button;

        public event Action Clicked;

        // ============================================================
        // UNITY
        // ============================================================

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

        // ============================================================
        // REFERENCES
        // ============================================================

        private void ResolveReferences()
        {
            // --------------------------------------------------------
            // NAME
            // --------------------------------------------------------

            if (_nameText == null)
            {
                _nameText =
                    FindTextByNames(
                        "txtName",
                        "Name",
                        "PlayerName");
            }

            // --------------------------------------------------------
            // SCORE
            // --------------------------------------------------------

            if (_scoreText == null)
            {
                _scoreText =
                    FindTextByNames(
                        "txtScore",
                        "Score",
                        "PlayerScore");
            }

            // --------------------------------------------------------
            // AVATAR
            // --------------------------------------------------------

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

                    if (_avatarImage == null)
                    {
                        _avatarImage =
                            avatar.GetComponentInChildren<Image>(
                                true);
                    }
                }
            }

            // --------------------------------------------------------
            // TURN GLOW
            // --------------------------------------------------------

            if (_turnGlow == null)
            {
                Transform glow =
                    FindChildRecursive(
                        transform,
                        "TurnGlow");

                if (glow != null)
                    _turnGlow =
                        glow.gameObject;
            }

            // --------------------------------------------------------
            // BUTTON
            // --------------------------------------------------------

            _button =
                GetComponent<Button>();
        }

        private TMP_Text FindTextByNames(
            params string[] names)
        {
            for (int i = 0;
                 i < names.Length;
                 i++)
            {
                Transform target =
                    FindChildRecursive(
                        transform,
                        names[i]);

                if (target == null)
                    continue;

                TMP_Text text =
                    target.GetComponent<TMP_Text>();

                if (text != null)
                    return text;
            }

            // Fallback
            TMP_Text[] texts =
                GetComponentsInChildren<TMP_Text>(
                    true);

            if (texts.Length >= 1)
                return texts[0];

            return null;
        }

        // ============================================================
        // BIND
        // ============================================================

        public void Bind(
            PlayerViewData player,
            bool isCurrentTurn,
            Sprite avatarSprite = null)
        {
            if (player == null)
                return;

            // --------------------------------------------------------
            // NAME
            // --------------------------------------------------------

            if (_nameText != null)
            {
                _nameText.text =
                    player.PlayerName;
            }

            // --------------------------------------------------------
            // SCORE
            // --------------------------------------------------------

            if (_scoreText != null)
            {
                _scoreText.text =
                    player.Score.ToString();
            }

            // --------------------------------------------------------
            // AVATAR
            // --------------------------------------------------------

            if (_avatarImage != null)
            {
                if (avatarSprite != null)
                {
                    _avatarImage.sprite =
                        avatarSprite;
                }

                /*
                 * اگر Avatar از قبل در Scene Sprite داشته باشد،
                 * حتی وقتی avatarSprite از GameUi نیامده باشد
                 * نباید Image غیرفعال شود.
                 */

                _avatarImage.enabled =
                    _avatarImage.sprite != null;
            }

            // --------------------------------------------------------
            // TURN
            // --------------------------------------------------------

            SetTurn(
                isCurrentTurn);
        }

        public void SetTurn(
            bool isCurrentTurn)
        {
            if (_turnGlow != null)
            {
                _turnGlow.SetActive(
                    isCurrentTurn);
            }
        }

        // ============================================================
        // CLICK
        // ============================================================

        private void OnClicked()
        {
            Clicked?.Invoke();
        }

        // ============================================================
        // HELPERS
        // ============================================================

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

        // ============================================================
        // DESTROY
        // ============================================================

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