using UnityEngine;

namespace DeadManDraws
{
    public class UiAudio : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private AudioSource _source;

        [Header("Clips")]
        public AudioClip Click;
        public AudioClip DrawCard;
        public AudioClip Burn;
        public AudioClip Collect;
        public AudioClip Win;
        public AudioClip Ability;   // cannon/sword/hook/map/oracle
        public AudioClip Trait;

        [Header("Volume")]
        [Range(0f, 1f)] public float MasterVolume = 1f;

        private void Awake()
        {
            if (_source == null) _source = GetComponent<AudioSource>();
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
        }

        private void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _source == null) return;
            _source.PlayOneShot(clip, volume * MasterVolume);
        }

        public void PlayClick() => Play(Click);
        public void PlayDraw() => Play(DrawCard);
        public void PlayBurn() => Play(Burn);
        public void PlayCollect() => Play(Collect);
        public void PlayWin() => Play(Win);
        public void PlayAbility() => Play(Ability);
        public void PlayTrait() => Play(Trait);
    }
}