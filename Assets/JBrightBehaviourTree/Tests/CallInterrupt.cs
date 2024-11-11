using System.Collections;

namespace Plugins.JBrightBehaviourTree.Tests
{
	public class CallInterrupt : State
	{
		private int askBeforeSayYes;
		private int askCount = 0;

		public CallInterrupt(int askBeforeSayYes)
		{
			this.askBeforeSayYes = askBeforeSayYes;
		}

		public override bool CanExecute()
		{
			if (askCount >= askBeforeSayYes)
				return true;
			
			askCount++;
			return false;
		}

		public override void Execute()
		{
			StartCoroutineInState( DoSmth() );
		}

		private IEnumerator DoSmth()
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