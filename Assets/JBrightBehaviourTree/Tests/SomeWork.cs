using System.Collections;

namespace Plugins.JBrightBehaviourTree.Tests
{
	public class SomeWork : State
	{
		public int executeCallingCount;
		
		public SomeWork(int updatesCount)
		{
			this.updatesCount = updatesCount;
		}

		public bool IsWorkDone;
		private int updatesCount;

		public override bool CanExecute()
		{
			return true;
		}

		public override void Execute()
		{
			executeCallingCount++;
			StartCoroutineInState( DoWork() );
		}

		private IEnumerator DoWork()
		{
			var work = 0;
			while (updatesCount > work)
			{
				work++;
				yield return null;
			}
			IsWorkDone = true;
		}

		public override void OnExit()
		{
		}
	}
}