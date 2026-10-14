using UnityEngine;

namespace DeadManDraws
{
    public class UiAudio : MonoBehaviour
    {
        [Header("SFX Source")]
        [SerializeField] private AudioSource _source;

        [Header("Music Source")]
        [SerializeField] private AudioSource _musicSource;

        [Header("Clips")]
        public AudioClip Click;
        public AudioClip DrawCard;
        public AudioClip Burn;
        public AudioClip Collect;
        public AudioClip Win;
        public AudioClip Ability;
        public AudioClip Trait;
        public AudioClip GunFire;

        [Header("Background Music")]
        public AudioClip BackgroundMusic;
        [Range(0f, 1f)] public float MusicVolume = 0.4f;
        public bool LoopMusic = true;
        public bool PlayMusicOnStart = true;

        [Header("Volume")]
        [Range(0f, 1f)] public float MasterVolume = 1f;

        private void Awake()
        {
            // SFX
            if (_source == null) _source = GetComponent<AudioSource>();
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;

            // Music: یک AudioSource جداگانه بساز
            if (_musicSource == null)
            {
                var musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform, false);
                _musicSource = musicGO.AddComponent<AudioSource>();
            }
            _musicSource.playOnAwake = false;
            _musicSource.loop = LoopMusic;
            _musicSource.volume = MusicVolume * MasterVolume;
        }

        private void Start()
        {
            if (PlayMusicOnStart) PlayMusic();
        }

        // =========================================================
        // SFX
        // =========================================================
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
        public void PlayGun() => Play(GunFire);

        // =========================================================
        // Music
        // =========================================================
        public void PlayMusic()
        {
            if (_musicSource == null || BackgroundMusic == null) return;

            if (_musicSource.clip == BackgroundMusic && _musicSource.isPlaying) return;

            _musicSource.clip = BackgroundMusic;
            _musicSource.loop = LoopMusic;
            _musicSource.volume = MusicVolume * MasterVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null) _musicSource.Stop();
        }

        public void PauseMusic()
        {
            if (_musicSource != null) _musicSource.Pause();
        }

        public void ResumeMusic()
        {
            if (_musicSource != null) _musicSource.UnPause();
        }

        public void SetMusicVolume(float v)
        {
            MusicVolume = Mathf.Clamp01(v);
            if (_musicSource != null) _musicSource.volume = MusicVolume * MasterVolume;
        }
    }
}