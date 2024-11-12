using System;
using Animancer;

namespace CharactersModule.Tests
{
	public class TestIdleState : AnimatorState
	{
		public ClipTransition idle;	
		
		private void Update()
		{
			PlaySimple( idle );
		}
	}
}