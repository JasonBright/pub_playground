using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Plugins.JBrightBehaviourTree
{
	public class BehaviourTree
	{
		private LinkedList<Node> rootStates = new();
		private float lastExecutionTime = float.MinValue;

		public int Frequency { get; set; }

		public void Run(DiContainer diContainer, float frequency = 0)
		{
			Inject( diContainer );
			Frequency = 0;
		}

		public BranchBuilder AddBranch(State state)
		{
			return AddBranch( state, rootStates );
		}

		public BranchBuilder AddBranch(State state, int order = 0)
		{
			return AddBranch( state, rootStates, order );
		}

		public BranchBuilder Add(State state, BranchBuilder parent, int order = 0)
		{
			return AddBranch( state, parent.Node.SubNodes, order );
		}

		public void Update()
		{
			if (activeState != null)
			{
				activeState.MoveNext();
			}

			if (lastExecutionTime + Frequency > Time.time)
				return;

			Execute();
			lastExecutionTime = Time.time;
		}

		private void Inject(DiContainer diContainer)
		{
			if (diContainer == null)
				return;

			var nodes = GetAllNodes();
			foreach (Node node in nodes)
			{
				diContainer.Inject( node.State );
			}
		}

		private List<Node> GetAllNodes()
		{
			List<Node> nodes = new List<Node>();
			foreach (Node rootState in rootStates)
			{
				RecursiveAddNodes( rootState, nodes );
			}

			return nodes;
		}

		private void RecursiveAddNodes(Node currentNode, List<Node> nodes)
		{
			nodes.Add( currentNode );
			foreach (Node subNode in currentNode.SubNodes)
			{
				RecursiveAddNodes( subNode, nodes );
			}
		}

		// ReSharper disable Unity.PerformanceAnalysis
		private void Execute()
		{
			for (int i = 0; i < 10; i++)
			{
				var shouldRunAgain = false;
				currentPath.Clear();
				var prevHead = previousPath.First;
				var isExecuting = ExecuteRecursive( rootStates, prevHead );
				if (isExecuting == false)
				{
					ExitingCurrentStates();
					shouldRunAgain = true;
				}
				else
				{
					previousPath.Clear();
					foreach (Node node in currentPath)
					{
						previousPath.AddLast( node );
					}
				}


				if (shouldRunAgain == false)
				{
					return;
				}
			}

			Debug.LogError( "Emergency exit!" );
		}

		public bool IsExecuting(State state)
		{
			if (activeState == state)
				return true;

			return currentPath.Any( node => node.State == activeState );
		}

		private bool ExecuteRecursive(LinkedList<Node> nodes, LinkedListNode<Node> prevHead)
		{
			foreach (Node node in nodes)
			{
				if (node.State.CanExecute() == false)
					continue;

				currentPath.AddLast( node );

				var isPrevHeadNull = prevHead == null;
				var isCurrentNodeDifferentOrNull = isPrevHeadNull || prevHead.Value != node;

				if (isCurrentNodeDifferentOrNull)
				{
					if (isPrevHeadNull == false)
						ExitingPreviousStatesWithCheckCurrent();

					node.State.Execute();
					activeState = node.State;
					if (node.State.IsDone)
					{
						//ныряю дальше
						var isExecuting = ExecuteRecursive( node.SubNodes, null );
						return isExecuting;
					}

					//стейт работает - выполнение закончено - буду ждать следующего цикла обновления дерева
					return true;
				}
				else //стейт не поменялся
				{
					if (node.State.IsDone)
					{
						prevHead = prevHead.Next;
						var isExecuting = ExecuteRecursive( node.SubNodes, prevHead );
						return isExecuting;
					}
					else
					{
						return true;
					}
				}
			}

			return false;
		}

		private void ExitingPreviousStatesWithCheckCurrent()
		{
			//var lastCurrent = currentPath.Last.Previous;
			while (previousPath.Count > 0)
			{
				var last = previousPath.Last;
				if (currentPath.Contains( last.Value ))
				{
					break;
				}

				last.Value.State.OnExit();
				last.Value.State.StopExecuting();
				previousPath.RemoveLast();

				// if(lastCurrent != null)
				// 	lastCurrent = lastCurrent.Previous;
			}
		}

		private void ExitingCurrentStates()
		{
			while (currentPath.Count > 0)
			{
				var last = currentPath.Last;
				last.Value.State.OnExit();
				last.Value.State.StopExecuting();
				currentPath.RemoveLast();
			}
			previousPath.Clear();
		}

		private bool IsTheSameNodes(LinkedListNode<Node> currentHead, LinkedListNode<Node> previousHead)
		{
			if (currentHead == null)
				return false;
			return currentHead.Value == previousHead.Value;
		}

		private LinkedList<Node> previousPath = new();
		private LinkedList<Node> currentPath = new();
		public State activeState;

		private BranchBuilder AddBranch(State state, LinkedList<Node> subNodes, int order = 0)
		{
			var node = new Node( state, order );
			AddNodeWithOrderSorting( subNodes, node );
			var builder = new BranchBuilder( this, node );
			return builder;
		}

		private void AddNodeWithOrderSorting(LinkedList<Node> nodes, Node newNode)
		{
			LinkedListNode<Node> currentNode = nodes.First;

			while (currentNode != null && currentNode.Value.Order < newNode.Order)
			{
				currentNode = currentNode.Next;
			}

			if (currentNode == null || currentNode.Value.Order == newNode.Order)
			{
				nodes.AddLast( newNode );
			}
			else
			{
				nodes.AddBefore( currentNode, newNode );
			}
		}

		public void Reset()
		{
			ExitingCurrentStates();
			previousPath.Clear();
			currentPath.Clear();
			lastExecutionTime = float.MinValue;
			activeState = null;
		}
	}

	public class BranchBuilder
	{
		private BranchBuilder sequenceHead;
		internal BehaviourTree Tree { get; }
		internal Node Node { get; }

		public BranchBuilder(BehaviourTree tree, Node node)
		{
			Tree = tree;
			Node = node;
			sequenceHead = this;
		}

		public BranchBuilder AddBranch(State state, int order = 0)
		{
			return Tree.Add( state, this, order );
		}

		//выглядит как мутная темка
		public BranchBuilder AddSequenced(State state, int order = 0)
		{
			sequenceHead = Tree.Add( state, sequenceHead, order );
			return sequenceHead;
		}
	}

	public class Node
	{
		public State State { get; private set; }
		public LinkedList<Node> SubNodes { get; private set; }
		public int Order { get; }

		public Node(State state, int order = 0)
		{
			State = state;
			SubNodes = new LinkedList<Node>();
			Order = order;
		}
	}
}