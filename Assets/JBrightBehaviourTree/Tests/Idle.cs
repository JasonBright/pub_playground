using System.Collections;

namespace Plugins.JBrightBehaviourTree.Tests
{
	public class Idle : State
	{
		public override bool CanExecute()
		{
			return true;
		}

		public override void Execute()
		{
			StartCoroutineInState( JustIdle() );
		}

		private IEnumerator JustIdle()
		{
			while (true)
			{
				yield return null;
			}
		}

		public override void OnExit()
		{
		}
	}
}