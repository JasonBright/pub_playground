using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Plugins.JBrightBehaviourTree
{
	//todo: сильно не нравится, что использует то же имя, что и сишарповый экшон
	public abstract class State
	{
		public bool IsDone => routines.Count == 0;
		protected List<IEnumerator> routines = new( 4 );
		
		private Dictionary<RoutineHandler, IEnumerator> routinesMap = new(4);
		public event Action ExceptionCatched;
		
		public abstract bool CanExecute();
		//todo: меня смущает название Execute. Будто бы этот метод будет вызываться при каждом апдейте во время выполнения
		//а по факту он выполняется лишь единожды, когда в ноду зашли впервые.
		public abstract void Execute();
		public abstract void OnExit();

		internal void StopExecuting()
		{
			routines.Clear();
			routinesMap.Clear();
		}

		protected RoutineHandler StartCoroutineInState(IEnumerator enumerator)
		{
			routines.Add( enumerator );
			var enumeratorKey = new RoutineHandler();
			routinesMap[ enumeratorKey ] = enumerator;
			return enumeratorKey;
		}

		protected void StopCoroutineInState(RoutineHandler enumeratorKey)
		{
			if (routinesMap.TryGetValue( enumeratorKey, out var routine ))
			{
				routinesMap.Remove( enumeratorKey );
				routines.Remove( routine );
			}
		}
		
		public void MoveNext()
		{
			for(int i = routines.Count - 1; i >= 0; i--)
			{
				var isRunning = false;
				try
				{
					isRunning = EnumeratorHandler.ConsumeEnumerator( routines[ i ] );
				}
				catch(Exception e)
				{
					LogInStateException( e );
				}
				if (isRunning == false && routines.Count > 0)
				{
					routines.RemoveAt( i );
				}
			}

			if (routines.Count == 0)
			{

			}
		}
		
		private void LogInStateException(Exception e)
		{
#if UNITY_EDITOR
			Debug.LogError( $"Editor Only: Suspend Exception at {this.GetType().Name} - {e}" );
#endif
			ExceptionCatched?.Invoke();
		}
	}

	public class RoutineHandler
	{
	}
}