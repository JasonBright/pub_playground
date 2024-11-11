namespace Plugins.JBrightBehaviourTree.Tests
{
	public class PlaceholderState : State
	{
		private bool _canExecute;

		public PlaceholderState(bool canExecute)
		{
			_canExecute = canExecute;
		}

		public override bool CanExecute()
		{
			return _canExecute;
		}

		public override void Execute()
		{
		}

		public override void OnExit()
		{
		}
	}
}