namespace Plugins.JBrightBehaviourTree.Tests
{
	public class Gate : State
	{
		public bool GateOpened;
		private bool canEnter = true;
		private int availableEntrances;
		private int entranced = 0;
		
		public Gate(int availableEntrances = 1)
		{
			this.availableEntrances = availableEntrances;
		}

		public override bool CanExecute()
		{
			if (canEnter)
			{
				return true;
			}
			return GateOpened;
		}

		public override void Execute()
		{
			GateOpened = true;
			entranced++;
			if(entranced >= availableEntrances)
			{
				canEnter = false;
			}
		}

		public override void OnExit()
		{
			GateOpened = false;
		}
	}
}