using System;
using System.Threading;
using Animancer;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharactersModule
{
	[RequireComponent( typeof(AnimancerComponent) ), ExecuteAlways]
	public class AnimationPreview : MonoBehaviour
	{
		[SerializeField] private ClipTransition idleClip;
		
		[SerializeField, HideIf( "sequence" )] private ClipTransition clip;
		[SerializeField] private bool sequence = false;
		[SerializeField, ShowIf( "sequence" ), TableList] private ActionSequenceEntry[] actionSequence;
		private CancellationTokenSource playingToken;

		private bool isPlaying;
		
		[Serializable]
		private class ActionSequenceEntry
		{
			[TableColumnWidth(200)]
			public ClipTransition Action;

			[TableColumnWidth(50, false), EnableIf("IsClipLooped"), Range(1, 10)]
			public int Repeats = 1;

			private bool IsClipLooped()
			{
				if (Action.Clip == null)
					return false;
				return Action.Clip.isLooping;
			}
		}

		private void Update()
		{
			if (isPlaying)
				return;
			if (idleClip.Clip == null)
				return;

			animancer().Play( idleClip );
		}

		[Button]
		public async void Play()
		{
			animancer().Graph.UnpauseGraph();
			
			isPlaying = true;
			if (playingToken != null)
			{
				playingToken.Cancel();
				playingToken.Dispose();
			}

			if (sequence == false)
			{
				PlaySingleClip();
				return;
			}

			playingToken = new CancellationTokenSource();
			foreach (var clipTransition in actionSequence)
			{
				var playingState = animancer().Play( clipTransition.Action );
				var isCanceled = await UniTask.WaitWhile( () =>
					{
						if (playingState.IsLooping == false)
							return playingState.IsPlaying;

						var loops = (int)playingState.NormalizedTime;
						return loops < clipTransition.Repeats;
					}, 
					cancellationToken: playingToken.Token ).SuppressCancellationThrow();
				if (isCanceled)
				{
					isPlaying = false;
					return;
				}
			}

			isPlaying = false;
		}

		private void PlaySingleClip()
		{
			if (clip.Clip == null)
				return;
			
			animancer().Play( clip );
		}

		[Button]
		public void PlayIdle()
		{
			animancer().Graph.UnpauseGraph();
			animancer().Play( idleClip );
		}

		[Button]
		public void StopAll()
		{
			if(playingToken != null)
				playingToken.Cancel();
			animancer().Stop();
			animancer().Graph.Destroy();
			animancer().Graph.PauseGraph();
		}

		// ReSharper disable once InconsistentNaming
		private AnimancerComponent animancer()
		{
			return GetComponent<AnimancerComponent>();
		}
	}
}