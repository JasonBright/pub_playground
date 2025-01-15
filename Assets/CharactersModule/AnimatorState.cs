using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using GameLogic.Characters;
using UnityEngine;
using Zenject;

namespace CharactersModule
{
	public class AnimatorState : MonoBehaviour
	{
		private AnimancerComponent animancer;
		private AnimatorBrain brain;

		private PlayingAnimation playingAnimation;
		protected AnimancerState playingSimple;
		
		[field: SerializeField] public PriorityLayer Priority { get; private set; }
		public AnimancerComponent Animancer => animancer;

		public bool IsPlaying
		{
			get
			{
				if (playingAnimation == null)
					return false;

				if (playingAnimation.IsPlaying == false)
					return false;

				return playingAnimation.IsPlaying;
			}
		}

		public AnimancerState PlaySimple(ClipTransition clip)
		{
			if (CanPlayNow() == false)
				return null;
			playingSimple = animancer.Play( clip );
			return playingSimple;
		}
		
		public void Stop()
		{
			if (playingAnimation == null)
				return;
			playingAnimation.Stop();
			playingAnimation = null;
		}

		[Inject]
		public void Init(AnimancerComponent animancer, AnimatorBrain brain)
		{
			this.brain = brain;
			this.animancer = animancer;
		}

		protected PlayingAnimation Create(ClipTransition transition)
		{
			var playing = new PlayingAnimation( transition, animancer, this );
			playingAnimation = playing;
			return playing;
		}

		public virtual bool CanPlayNow()
		{
			return brain.PlayRequest( this );
		}
	}

	public enum AnimationReactionType
	{
		Hit,
		End,
	}

	public class PlayingAnimation
	{
		private AnimancerState state;
		private AnimancerComponent animancer;
		private Func<bool> canPlayCondition;
		private bool isWaiting;
		private ClipTransition transition;

		public bool IsLost { get; private set; } = false;

		public bool IsPlaying => state != null && IsEnded() == false;

		public PlayingAnimation(ClipTransition transition, AnimancerComponent animancer, AnimatorState animatorState)
		{
			this.animatorState = animatorState;
			this.transition = transition;
			this.animancer = animancer;
		}

		private List<Tuple<AnimationReactionType, Action>> reactions = new();
		private AnimatorState animatorState;
		private bool isOneLoopForced;

		public PlayingAnimation AddReaction(AnimationReactionType reactionType, Action callback)
		{
			reactions.Add( new Tuple<AnimationReactionType, Action>( reactionType, callback ) );
			return this;
		}

		public PlayingAnimation OneLoopForced()
		{
			isOneLoopForced = true;
			return this;
		}

		public PlayingAnimation Play(float speed = 1)
		{
			if (animatorState.CanPlayNow())
			{
				if (state != null && state.Clip == transition.Clip && IsEnded())
				{
					state.Time = 0;
				}

				state = animancer.Play( transition );
				state.Speed = speed;
				
				if (state.Events( null, out var sequence ))
				{
					foreach (var ev in transition.Clip.events)
					{
						sequence.Add( ev.time / transition.Length, OnHit );
					}
					sequence.OnEnd = OnEndCallback;
				}
				else
				{
					for (var index = 0; index < transition.Clip.events.Length; index++)
					{
						state.SharedEvents.SetCallback( index, OnHit );
					}
				}
			}
			return this;
		}

		public void SetSpeed(float speed)
		{
			state.Speed = speed;
		}

		private void OnEndCallback()
		{
			if (isOneLoopForced && transition.IsLooping) 
			{
				Stop();
			}
		}

		private void OnHit()
		{
			foreach (var reaction in reactions)
			{
				if (reaction.Item1 == AnimationReactionType.Hit)
				{
					reaction.Item2.Invoke();
				}
			}
		}

		private bool IsEnded()
		{
			if (state.IsPlaying == false)
				return true;

			if (IsLost)
				return true;

			if (isOneLoopForced && (int)state.NormalizedTime > 0)
				return true;
			
			return false;
		}

		public IEnumerator WaitForEnd()
		{
			while (IsEnded() == false)
			{
				yield return null;
			}
		}

		public void Stop()
		{
			IsLost = true;
		}
	}

	public enum AnimationResult
	{
		Reset,
		Waiting,
		Playing,
		Hit,
		End,
	}

	public enum PriorityLayer
	{
		Base = 0,
		OverrideBase = 10,
		Impacting = 100,
		Special = 200,
		Die = 300,
	}
}