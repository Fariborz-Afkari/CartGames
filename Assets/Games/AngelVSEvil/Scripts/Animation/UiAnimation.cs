using UnityEngine;

namespace AngelVSZombie
{
    

    public class UiAnimation : MonoBehaviour
    {
        public Animator CardAnimator;
        
        public void PlayDrawAnimation()
        {
            if (CardAnimator != null) CardAnimator.SetTrigger("Draw");
        }
        
        public void PlayPlayAnimation()
        {
            if (CardAnimator != null) CardAnimator.SetTrigger("Play");
        }
    }
}