using System;
using UnityEngine;

namespace Code.Common.Tutor
{
	public class TutorialStep : ScriptableObject
	{
		//====================
		// PUBLIC
		//====================
		/// <summary>
		/// Set to true from <see cref="OnComplete()"/>
		/// </summary>
		public bool isComplete { get; protected set; }
		
		/// <summary>
		/// Set to true from after Execute() is called
		/// </summary>
		public bool isExecuting { get; protected set; }

		//====================
		// PROTECTED
		//====================
		/// <summary>
		/// Tutor reference for communication
		/// </summary>
		protected Tutor tutor;

		/// <summary>
		/// Timer used if the delay is > 0
		/// </summary>
		protected BSTimer timer;

		/// <summary>
		/// Potential message to display in the message box
		/// </summary>
		[Tooltip("Message to be displayed")]
		[SerializeField] protected string message;

		/// <summary>
		/// Arbitrary delay before execution
		/// </summary>
		[Tooltip("Delay before tutorial step can execute")]
		[SerializeField] protected float delay;

		/// <summary>
		/// Called as soon as <see cref="Tutor.Run"/> happens
		/// </summary>
		internal virtual void Init()
		{
			isExecuting = false;
			isComplete = false;
		}

		/// <summary>
		/// Called after the Tutor has initialized all the tutorial steps.
		/// </summary>
		internal virtual void PreExecute()
		{
			
		}

		/// <summary>
		/// Sets a tutor reference
		/// </summary>
		/// <param name="t"></param>
		internal void SetTutor(Tutor t)
		{
			tutor = t;
		}

		/// <summary>
		/// Execute whatever needs to happen for this tutorial step
		/// </summary>
		internal virtual void Execute()
		{
			if (isExecuting)
			{
				return;
			}
			
			isExecuting = true;

			if (delay > 0)
			{
				timer = new BSTimer(delay, false, DelayedExecution);
				timer.Start();
			}
			else
			{
				RenderMessage();
			}
		}

		/// <summary>
		/// Delayed execution runs if delay > 0. Subclasses wanting to implement a delay should leverage
		/// this method.
		/// </summary>
		protected virtual void DelayedExecution()
		{
			timer.Destroy();
			RenderMessage();
		}

		/// <summary>
		/// Method to call when execution has finished
		/// </summary>
		public virtual void OnComplete()
		{
			isComplete = true;
			
			if (tutor != null && tutor.GetTutorialPack() != null)
			{
				TutorialPack pack = tutor.GetTutorialPack();
				pack.PlayShroudOut();
				pack.ToggleMessage(false);
				pack.ToggleSwipe(false);
			}
			
			tutor.OnTutorialStepComplete(this);
		}

		/// <summary>
		/// Restore any changes to the system this step may have done
		/// </summary>
		internal virtual void Restore()
		{
			
		}

		/// <summary>
		/// Invoke any events the tutorial has taken control over
		/// </summary>
		internal virtual void InvokeEvents()
		{
			
		}

		/// <summary>
		/// Positions and updates the message if there is one
		/// </summary>
		internal virtual void RenderMessage()
		{
			if (tutor != null && tutor.GetTutorialPack() != null)
			{
				TutorialPack pack = tutor.GetTutorialPack();
				
				if (!string.IsNullOrEmpty(message))
				{
					pack.ToggleSwipe(false);
					pack.ToggleMessage(true);
					pack.SetMessage(message);
					pack.PlayShroudIn();
				}
				else
				{
					pack.PlayShroudOut();
					pack.ToggleMessage(false);
					pack.ToggleSwipe(false);
				}	
			}
		}

		/// <summary>
		/// Returns true if/when <see cref="Execute"/> can be called
		/// </summary>
		internal virtual bool CanExecute
		{
			get
			{
				return !isComplete && !isExecuting;
			}
		}
	}
}