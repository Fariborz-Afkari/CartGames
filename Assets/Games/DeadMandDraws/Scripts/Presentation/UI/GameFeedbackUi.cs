using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardGames.DeadManDraws.Presentation.UI
{
    /// <summary>
    /// Shared feedback layer:
    /// notification, turn indicator, pause, confirmation,
    /// victory/defeat and simple fade transitions.
    /// </summary>
    public sealed class GameFeedbackUi : MonoBehaviour
    {
        [Header("Notification")]
        [SerializeField] private CanvasGroup _notificationGroup;
        [SerializeField] private TMP_Text _notificationText;
        [SerializeField] private float _notificationDuration = 1.8f;

        [Header("Turn")]
        [SerializeField] private GameObject _turnIndicator;
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private Animator _turnAnimator;

        [Header("Pause")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton;

        [Header("Confirmation")]
        [SerializeField] private GameObject _confirmationPanel;
        [SerializeField] private TMP_Text _confirmationText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        [Header("Result")]
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private GameObject _defeatPanel;
        [SerializeField] private TMP_Text _resultText;

        [Header("Screen Transition")]
        [SerializeField] private CanvasGroup _transitionGroup;
        [SerializeField] private float _transitionDuration = 0.25f;

        private Coroutine _notificationRoutine;
        private System.Action _confirmAction;

        private void Awake()
        {
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(Pause);

            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(Resume);

            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(Confirm);

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(CancelConfirmation);

            SetPause(false);
            HideResult();
            CancelConfirmation();
        }

        public void SetTurn(
            bool isPlayerTurn,
            string playerName = null)
        {
            if (_turnIndicator != null)
                _turnIndicator.SetActive(true);

            if (_turnText != null)
            {
                _turnText.text = isPlayerTurn
                    ? "YOUR TURN"
                    : string.IsNullOrWhiteSpace(playerName)
                        ? "OPPONENT TURN"
                        : $"{playerName} TURN";
            }

            if (_turnAnimator != null)
                _turnAnimator.SetTrigger("TurnChanged");
        }

        public void Notify(string message)
        {
            if (_notificationText != null)
                _notificationText.text = message;

            if (_notificationGroup == null)
                return;

            if (_notificationRoutine != null)
                StopCoroutine(_notificationRoutine);

            _notificationRoutine =
                StartCoroutine(ShowNotification());
        }

        public void ShowConfirmation(
            string message,
            System.Action confirmed)
        {
            _confirmAction = confirmed;

            if (_confirmationText != null)
                _confirmationText.text = message;

            if (_confirmationPanel != null)
                _confirmationPanel.SetActive(true);
        }

        public void ShowVictory(string message = "VICTORY")
        {
            if (_defeatPanel != null)
                _defeatPanel.SetActive(false);

            if (_resultText != null)
                _resultText.text = message;

            if (_victoryPanel != null)
                _victoryPanel.SetActive(true);
        }

        public void ShowDefeat(string message = "DEFEAT")
        {
            if (_victoryPanel != null)
                _victoryPanel.SetActive(false);

            if (_resultText != null)
                _resultText.text = message;

            if (_defeatPanel != null)
                _defeatPanel.SetActive(true);
        }

        public void HideResult()
        {
            if (_victoryPanel != null)
                _victoryPanel.SetActive(false);

            if (_defeatPanel != null)
                _defeatPanel.SetActive(false);
        }

        public void Pause()
        {
            SetPause(true);
        }

        public void Resume()
        {
            SetPause(false);
        }

        public void SetPause(bool paused)
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(paused);

            Time.timeScale = paused ? 0f : 1f;
        }

        public void PlayTransitionIn()
        {
            if (_transitionGroup != null)
                StartCoroutine(Fade(_transitionGroup, 0f, 1f));
        }

        public void PlayTransitionOut()
        {
            if (_transitionGroup != null)
                StartCoroutine(Fade(_transitionGroup, 1f, 0f));
        }

        private void Confirm()
        {
            System.Action action = _confirmAction;
            _confirmAction = null;

            if (_confirmationPanel != null)
                _confirmationPanel.SetActive(false);

            action?.Invoke();
        }

        private void CancelConfirmation()
        {
            _confirmAction = null;

            if (_confirmationPanel != null)
                _confirmationPanel.SetActive(false);
        }

        private IEnumerator ShowNotification()
        {
            _notificationGroup.alpha = 0f;
            _notificationGroup.gameObject.SetActive(true);

            yield return Fade(
                _notificationGroup,
                0f,
                1f,
                0.12f);

            yield return new WaitForSecondsRealtime(
                _notificationDuration);

            yield return Fade(
                _notificationGroup,
                1f,
                0f,
                0.18f);

            _notificationGroup.gameObject.SetActive(false);
            _notificationRoutine = null;
        }

        private IEnumerator Fade(
            CanvasGroup group,
            float from,
            float to,
            float duration = -1f)
        {
            if (group == null)
                yield break;

            if (duration < 0f)
                duration = _transitionDuration;

            group.alpha = from;

            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            group.alpha = to;
        }
    }
}
