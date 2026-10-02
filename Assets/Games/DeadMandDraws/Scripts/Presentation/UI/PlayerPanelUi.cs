using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardGames.DeadManDraws.Presentation.Game;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class PlayerPanelUi : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private TMP_Text _resourceText;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Button _button;

        [Header("Turn Feedback")]
        [SerializeField] private GameObject _turnGlow;
        [SerializeField] private GameObject _turnIndicator;
        [SerializeField] private Animator _animator;

        [Header("Optional State")]
        [SerializeField] private GameObject _defeatedOverlay;

        public event Action Clicked;

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

        public void Bind(PlayerViewData player, bool isCurrentTurn)
        {
            if (player == null)
                return;

            if (_nameText != null)
                _nameText.text = player.PlayerName;

            SetOptionalText(
                _scoreText,
                ReadMember(player, "Score", "Points"));

            SetOptionalText(
                _healthText,
                ReadMember(player, "Health", "HP"));

            SetOptionalText(
                _resourceText,
                ReadMember(player, "Resource", "Mana", "Energy"));

            object defeatedValue =
                ReadMember(player, "IsDefeated", "Defeated");

            bool defeated;

            if (defeatedValue != null)
            {
                defeated = ToBool(defeatedValue, false);
            }
            else
            {
                object aliveValue = ReadMember(player, "IsAlive");
                defeated = aliveValue != null && !ToBool(aliveValue, true);
            }

            if (_defeatedOverlay != null)
                _defeatedOverlay.SetActive(defeated);

            SetTurn(isCurrentTurn);
        }

        public void SetTurn(bool isCurrentTurn)
        {
            if (_turnGlow != null)
                _turnGlow.SetActive(isCurrentTurn);

            if (_turnIndicator != null)
                _turnIndicator.SetActive(isCurrentTurn);

            if (_animator != null)
            {
                _animator.SetBool("IsTurn", isCurrentTurn);
            }
        }

        private void OnClicked()
        {
            Clicked?.Invoke();
        }

        private static void SetOptionalText(TMP_Text target, object value)
        {
            if (target == null)
                return;

            target.text = value == null ? string.Empty : value.ToString();
        }

        private static object ReadMember(object source, params string[] names)
        {
            if (source == null)
                return null;

            Type type = source.GetType();

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

        private static bool ToBool(object value, bool fallback)
        {
            if (value == null)
                return fallback;

            if (value is bool boolValue)
                return !boolValue;

            try
            {
                return Convert.ToBoolean(value);
            }
            catch
            {
                return fallback;
            }
        }
    }
}
