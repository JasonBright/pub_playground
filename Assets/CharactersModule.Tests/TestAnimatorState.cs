using Animancer;
using UnityEngine;

namespace CharactersModule.Tests
{
	public class TestAnimatorState : AnimatorState
	{
		[SerializeField] private ClipTransition clip;
		
		public PlayingAnimation PlayAttack()
		{
			return this.Create( clip );
		}
	}
}