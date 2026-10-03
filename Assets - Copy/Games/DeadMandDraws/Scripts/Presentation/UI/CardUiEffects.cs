using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardGames.DeadManDraws.Presentation.UI
{
    /// <summary>
    /// Add this component to the root of the CardWidget prefab.
    /// Provides hover, selected, playable-glow and click feedback
    /// without coupling the visual effect to CardViewData.
    /// </summary>
    public sealed class CardUiEffects : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        [Header("Scale")]
        [SerializeField, Min(1f)] private float _hoverScale = 1.06f;
        [SerializeField, Min(0.01f)] private float _scaleDuration = 0.10f;

        [Header("Glow")]
        [SerializeField] private GameObject _playableGlow;
        [SerializeField] private GameObject _selectedGlow;

        [Header("Optional Click")]
        [SerializeField] private Button _button;

        private Vector3 _baseScale;
        private Coroutine _scaleRoutine;
        private bool _hovered;
        private bool _selected;

        public bool IsPlayable { get; private set; }

        private void Awake()
        {
            _baseScale = transform.localScale;

            if (_button != null)
                _button.onClick.AddListener(EmitButtonClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(EmitButtonClick);
        }

        public void SetPlayable(bool value)
        {
            IsPlayable = value;

            if (_playableGlow != null)
                _playableGlow.SetActive(value);
        }

        public void SetSelected(bool value)
        {
            _selected = value;

            if (_selectedGlow != null)
                _selectedGlow.SetActive(value);

            RefreshScale();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            RefreshScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            RefreshScale();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Reserved for future card click FX.
        }

        private void EmitButtonClick()
        {
            // The existing CardWidget owns the actual card callback.
            // This component is visual-only.
        }

        private void RefreshScale()
        {
            float target =
                _hovered || _selected
                    ? _hoverScale
                    : 1f;

            if (_scaleRoutine != null)
                StopCoroutine(_scaleRoutine);

            _scaleRoutine = StartCoroutine(
                AnimateScale(_baseScale * target));
        }

        private IEnumerator AnimateScale(Vector3 target)
        {
            Vector3 start = transform.localScale;
            float time = 0f;

            while (time < _scaleDuration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / _scaleDuration);
                transform.localScale =
                    Vector3.Lerp(start, target, t);

                yield return null;
            }

            transform.localScale = target;
            _scaleRoutine = null;
        }
    }
}
