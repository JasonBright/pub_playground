using UnityEngine;

namespace SequenceStateMachine
{
    public class WaitSeconds : CustomYieldInstruction
    {
        private readonly float continueTime;
        public override bool keepWaiting => continueTime > Time.time; 

        public WaitSeconds(float seconds)
        {
            continueTime = Time.time + seconds;
        }
    }
}