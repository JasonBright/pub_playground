using System;
using System.Collections;
using Animancer;
using GameLogic.Characters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharactersModule.Tests
{
	public class TestAnimatorBrain : MonoBehaviour
	{
		public TestAnimatorState fighterAnimator;
		public TestAnimatorState gatheringAnimator;
		public TestIdleState testIdleState;
		
		public void Hit(){ }

		private void Awake()
		{
			var animatorBrain = new AnimatorBrain();
			var animancer = GetComponent<AnimancerComponent>();
			fighterAnimator.Init( animancer, animatorBrain );
			gatheringAnimator.Init( animancer, animatorBrain );
			testIdleState.Init( animancer, animatorBrain );
		}

		[Button]
		public void Play()
		{
			StartCoroutine( DoPlay() );
		}
		
		[Button]
		public void StopFighter()
		{
			fighterAnimator.Stop();
		}

		[Button]
		public void PlayGathering()
		{
			StartCoroutine( DoPlayGathering() );
		}

		private int playcounter;

		IEnumerator DoPlay()
		{
			var playIndex = playcounter;
			playcounter++;
			var playingAnimation = fighterAnimator.PlayAttack().
							AddReaction( AnimationReactionType.Hit, () => { Debug.Log( $"Hit from fighter {playIndex}" ); } ).
							Play();
			yield return playingAnimation.WaitForEnd();
			Debug.Log( $"fighter end anim" );
		}
		
		IEnumerator DoPlayGathering()
		{
			var playingAnimation = gatheringAnimator.PlayAttack().
				AddReaction( AnimationReactionType.Hit, () => { Debug.Log( $"Gathering!" ); } ).
				OneLoopForced().
				Play();
			yield return playingAnimation.WaitForEnd();
			Debug.Log( $"gathering end anim" );
		}
	}
}