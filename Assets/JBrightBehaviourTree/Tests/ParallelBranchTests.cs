using NUnit.Framework;

namespace Plugins.JBrightBehaviourTree.Tests
{
	public class ParallelBranchTests
	{
		[Test]
		public void SkipAllStateInFirstBranch()
		{
			var tree = new BehaviourTree();
			var idle = new Idle();

			var falseState1 = new PlaceholderState( false );
			var falseState2 = new PlaceholderState( false );
			var rootOfFalses = new PlaceholderState( true );

			var branch1 = tree.AddBranch( rootOfFalses );
			{
				branch1.AddBranch( falseState1 );
				branch1.AddBranch( falseState2 );
			}

			tree.AddBranch( idle );
			
			tree.Update();
			Assert.IsTrue( tree.IsExecuting( idle ) );
		}
	}
}