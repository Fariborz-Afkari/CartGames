using UnityEngine;

namespace CardGames.DeadManDraws.Presentation.UI
{
    public sealed class UiAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;

        [Header("UI")]
        [SerializeField] private AudioClip _buttonClip;
        [SerializeField] private AudioClip _deckClickClip;

        [Header("Cards")]
        [SerializeField] private AudioClip _drawClip;
        [SerializeField] private AudioClip _burnClip;
        [SerializeField] private AudioClip _collectClip;
        [SerializeField] private AudioClip _playClip;

        [Header("Turn / Feedback")]
        [SerializeField] private AudioClip _turnStartClip;
        [SerializeField] private AudioClip _notificationClip;

        public void PlayButton() => Play(_buttonClip);
        public void PlayDeckClick() => Play(_deckClickClip);
        public void PlayDraw() => Play(_drawClip);
        public void PlayBurn() => Play(_burnClip);
        public void PlayCollect() => Play(_collectClip);
        public void PlayPlayCard() => Play(_playClip);
        public void PlayTurnStart() => Play(_turnStartClip);
        public void PlayNotification() => Play(_notificationClip);

        private void Play(AudioClip clip)
        {
            if (_source != null && clip != null)
                _source.PlayOneShot(clip);
        }
    }
}
