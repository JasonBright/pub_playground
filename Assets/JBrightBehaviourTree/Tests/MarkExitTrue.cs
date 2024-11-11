using System.Collections;

namespace Plugins.JBrightBehaviourTree.Tests
{
	public class MarkExitTrue : State
	{
		public bool IsExit;
		private readonly int count;

		public MarkExitTrue(int targetCount)
		{
			count = targetCount;
		}

		public override bool CanExecute()
		{
			return true;
		}

		public override void Execute()
		{
			if(count > 0)
				StartCoroutineInState( DoWork() );
		}

		private IEnumerator DoWork()
		{
			for (int i = 0; i < count; i++)
			{
				yield return null;
			}
		}

		public override void OnExit()
		{
			IsExit = true;
		}
	}
}