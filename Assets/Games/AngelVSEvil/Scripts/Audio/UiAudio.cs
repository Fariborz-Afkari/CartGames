using UnityEngine;

namespace AngelVSZombie
{
    public class UiAudio : MonoBehaviour
    {
        public AudioSource AudioSource;
        public AudioClip CardDrawSound;
        public AudioClip CardPlaySound;
        public AudioClip ZombieSound;
        
        public void PlayCardDraw() => AudioSource.PlayOneShot(CardDrawSound);
        public void PlayCardPlay() => AudioSource.PlayOneShot(CardPlaySound);
        public void PlayZombieSound() => AudioSource.PlayOneShot(ZombieSound);
    }
}