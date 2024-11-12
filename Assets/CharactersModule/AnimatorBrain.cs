using CharactersModule;

namespace GameLogic.Characters
{
    public class AnimatorBrain
    {
        public PriorityLayer CurrentLayer => currentLayer;
        private PriorityLayer currentLayer;
        private AnimatorState currentAnimator;

        public bool PlayRequest(AnimatorState animator)
        {
            if (currentAnimator != null && currentAnimator.IsPlaying == false)
            {
                currentLayer = 0;
                currentAnimator = null;
            }
            
            if (animator.Priority >= currentLayer)
            {
                currentAnimator = animator;
                currentLayer = animator.Priority;
                return true;
            }
            return false;
        }
    }
}