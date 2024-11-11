using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Plugins.JBrightBehaviourTree.Tests
{
	public class CommonTests
	{
		[Test]
		public void can_enter_to_single_state()
		{
			var tree = new BehaviourTree();
			var idle = new Idle();
			tree.AddBranch( idle, 0 );
			tree.Update();
			Assert.IsTrue( tree.IsExecuting( idle ) );
		}
		
		[Test]
		public void make_work_and_exit_to_idle()
		{
			var size = 5;
			var tree = new BehaviourTree();
			var idle = new Idle();
			var work = new SomeWork(size);
			tree.AddBranch( work);
			tree.AddBranch( idle );
			for (int i = 0; i < size; i++)
			{
				tree.Update();
			}
			Assert.IsFalse( work.IsWorkDone );
			tree.Update();
			Assert.IsTrue( tree.IsExecuting( idle ) );
		}

		[Test]
		public void deep_branch_2states()
		{
			var size = 5;
			var tree = new BehaviourTree();
			var work = new SomeWork(size);
			var idle = new Idle();
			var gate = new Gate();

			tree.AddBranch( gate )
				.AddBranch( work );
			tree.AddBranch( idle );

			for (int i = 0; i <= size; i++)
			{
				tree.Update();
				Assert.IsTrue( gate.GateOpened );
			}
			tree.Update();
			Assert.IsTrue( gate.GateOpened == false );
		}
		
		[Test]
		public void deep_branch_4gates()
		{
			const int size = 4;
			var tree = new BehaviourTree();
			var idle = new Idle();
			var gates = new Gate[size];

			BranchBuilder b = null;
			for (int i = 0; i < size; i++)
			{
				gates[ i ] = new Gate();
				if (b == null)
				{
					b = tree.AddBranch( gates[ i ] );
				}
				else
				{
					b = b.AddBranch( gates[ i ] );
				}
			}
			
			tree.AddBranch( idle );

			tree.Update();
			Assert.IsTrue( gates[0].GateOpened == false );
		}

		[Test]
		public void deep_branch_4states()
		{
			var size = 5;
			var tree = new BehaviourTree();
			var gate = new Gate();
			var work = new SomeWork(size);

			var gate2 = new Gate();
			var work2 = new SomeWork( size );
			var idle = new Idle();

			tree.AddBranch( gate )
				.AddBranch( work )
				.AddBranch( gate2 )
				.AddBranch( work2 );
			tree.AddBranch( idle );

			for (int i = 0; i <= size; i++)
			{
				tree.Update();
				Assert.IsTrue( gate.GateOpened );
			}
			tree.Update();
			Debug.Log( $"{work.IsWorkDone}" );
			for (int i = 0; i < size; i++)
			{
				tree.Update();
				Assert.IsTrue( gate.GateOpened );
				Assert.IsTrue( gate2.GateOpened );
			}
			tree.Update();
			Debug.Log( $"{work2.IsWorkDone}" );
			Assert.IsTrue( gate.GateOpened == false );
			Assert.IsTrue( gate2.GateOpened == false );
		}
		
		[Test]
		public void deep_branch_with_sub_branch()
		{
			var size = 2;

			var mainGate = new Gate(2);
			
			var tree = new BehaviourTree();
			var gate = new Gate();
			var work = new SomeWork(size);

			var gate2 = new Gate();
			var work2 = new SomeWork( size );
			var idle = new Idle();

			var main = tree.AddBranch( mainGate );

			main.AddBranch( gate )
				.AddBranch( work );
			
			main.AddBranch( gate2 )
				.AddBranch( work2 );
			tree.AddBranch( idle );

			for (int i = 0; i <= size; i++)
			{
				tree.Update();
				Assert.IsTrue( mainGate.GateOpened );
				Assert.IsTrue( gate.GateOpened );
			}
			
			for (int i = 0; i < size; i++)
			{
				tree.Update();
				Assert.IsTrue( mainGate.GateOpened );
				Assert.IsTrue( gate.GateOpened == false );
				Assert.IsTrue( gate2.GateOpened );
			}
			tree.Update();
			tree.Update();
			Assert.IsTrue( mainGate.GateOpened == false );
			Assert.IsTrue( gate.GateOpened == false );
			Assert.IsTrue( gate2.GateOpened == false );
		}
		
		

		[Test]
		public void is_stopping_after_interrupt()
		{
			var tree = new BehaviourTree();

			var callInterrupt = new CallInterrupt( 1 );
			tree.AddBranch( callInterrupt );
			var someWork = new SomeWork( 5 );
			tree.AddBranch( someWork );
			tree.Update();
			Assert.IsTrue( tree.IsExecuting( someWork ) );
			tree.Update();
			Assert.IsTrue( tree.IsExecuting( callInterrupt ) );
			Assert.IsTrue( someWork.IsDone );
		}

		[Test]
		public void is_exit_state_correct_work()
		{
			var tree = new BehaviourTree();
			var callInterrupt = new CallInterrupt( 2 );
			tree.AddBranch( callInterrupt );

			var exitTrue = new MarkExitTrue( 0 );
			var exitTrue2 = new MarkExitTrue( 5 );
			tree.AddBranch( exitTrue )
				.AddBranch( exitTrue2 );

			for (int i = 0; i < 5; i++)
			{
				tree.Update();
			}
			Assert.IsTrue( exitTrue.IsExit );
			Assert.IsTrue( exitTrue2.IsExit );
		}
	}
}