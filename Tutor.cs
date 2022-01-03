using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Common.Tutor
{
	public class Tutor : MonoBehaviour
	{
		//====================
		// PUBLIC
		//====================
		/// <summary>
		/// Callback for when a tutorial step has been completed
		/// </summary>
		public Action<TutorialStep> OnStepComplete;

		/// <summary>
		/// Callback for when the tutor has finished
		/// </summary>
		public Action OnTutorComplete;

		//====================
		// PROTECTED
		//====================
		/// <summary>
		/// Current step in the tutorial
		/// </summary>
		protected int currentStep;

		/// <summary>
		/// Instantiated message box prefab
		/// </summary>
		protected TutorialPack messageBox;

		/// <summary>
		/// Timer to match the delay from <see cref="startDelay"/> before running the tutor steps
		/// </summary>
		protected BSTimer startTimer;

		/// <summary>
		/// Updates to the current state of this tutor instance
		/// </summary>
		protected StateMachine stateMachine;

		//====================
		// PRIVATE
		//====================
		/// <summary>
		/// Tutorial steps list
		/// </summary>
		[SerializeField] private List<TutorialStep> steps;
		
		/// <summary>
		/// Prefab for displaying clickable messages, swipe animations, etc.
		/// </summary>
		[SerializeField] private GameObject messageBoxPrefab;

		/// <summary>
		/// Set to true, will start running the tutor immediately when Awake() happens
		/// </summary>
		[Header("Parameters and Rule sets")]
		[Tooltip("Whether or not to run on Awake")]
		[SerializeField] private bool runOnAwake;

		/// <summary>
		/// With delay > 0, the Run() method won't execute until the time has expired
		/// </summary>
		[Tooltip("Delay before running, tied to Run on awake boolean")]
		[SerializeField] private float startDelay;
		
		/// <summary>
		/// When set to true, will automatically run the next tutorial step when the previous one finishes
		/// </summary>
		[Tooltip("Set to true, will run the next tutorial step right after the previous one finishes")]
		[SerializeField] private bool autoRun;
		
		/// <summary>
		/// Set to a value for the global lookup in <see cref="TutorDirector"/>, this is optional,
		/// but needed if you want a fast lookup for a tutorial step that may rely on another tutor
		/// instance to complete first
		/// </summary>
		[Tooltip("Global name used for lookups, can make tutorial steps that wait for a tutor instance easier to find")]
		[SerializeField] private string tutorName;

		/*================================================================================
		STARTUP/SHUTDOWN		
		=================================================================================*/
		private void Awake()
		{
			Init();

			if (!runOnAwake) { return; }
			
			if (startDelay > 0f)
			{
				startTimer = new BSTimer(startDelay, false, OnDelayComplete);
				startTimer.Start();
			}
			else
			{
				Run();
			}
		}

		/// <summary>
		/// Initialize any variables as needed, this is called from Awake as well as Run, so variables
		/// should certain to only be called once
		/// </summary>
		protected virtual void Init()
		{
			// already done this
			if (stateMachine != null)
			{
				return;
			}
			
			stateMachine = new StateMachine("tutor");
			stateMachine.AddState(GameState.GameStates.RUNNING);
			stateMachine.AddState(GameState.GameStates.READY);
			stateMachine.AddState(GameState.GameStates.DONE);
			stateMachine.UpdateState(GameState.GameStates.READY);
			
			TutorDirector.AddTutor(this);
		}

		private void OnDestroy()
		{
			TutorDirector.RemoveTutor(this);
		}

		/// <summary>
		/// Callback from the BSTimer delay
		/// </summary>
		private void OnDelayComplete()
		{
			startTimer?.Destroy();
			Run();
		}

		/// <summary>
		/// Queues the tutor instance, and it will run as soon as it can
		/// </summary>
		public void Queue()
		{
			Init();
			TutorDirector.QueueTutor(this);
		}

		/// <summary>
		/// Run the next tutorial step
		/// </summary>
		public void Run()
		{
			Init();

			if (stateMachine.isState(GameState.GameStates.READY))
			{
				stateMachine.UpdateState(GameState.GameStates.RUNNING);

				// initialize tutorial steps so they have appropriate values ready to go
				for (int i = currentStep; i < steps.Count; ++i)
				{
					steps[i].Init();
				}
				
				// preexecute calls so the tutorial steps can take control of elements in preparation
				// to be executed
				for (int i = currentStep; i < steps.Count; ++i)
				{
					steps[i].PreExecute();
				}
			}
			
			if (steps != null && steps.Count > 0 && currentStep < steps.Count)
			{
				TutorialStep step = steps[currentStep];

				if (step.CanExecute)
				{
					step.SetTutor(this);
					step.Execute();
				}
			}
		}

		/// <summary>
		/// End the tutor
		/// </summary>
		/// <param name="invokeLastStep">Set to true if you want to invoke the last step.
		/// This can be useful if the intention was to chain Tutors together using callbacks or 
		/// <see cref="TutorialTutorStep"/></param>
		public void Terminate(bool invokeLastStep = false)
		{
			Init();
			
			// move to the last step
			currentStep = steps.Count - 1;

			if (invokeLastStep)
			{
				// invocation may concern a transfer of control as is typical with tutorials
				// so we allow a user to terminate the tutor, and still potentially handle callbacks
				steps[currentStep].InvokeEvents();
			}

			// if the tutor was already running, call Restore() on the tutorial steps
			if (!stateMachine.isState(GameState.GameStates.READY))
			{
				// restore all the steps to their original settings
				for (int i = 0; i < steps.Count; ++i)
				{
					steps[i].Restore();
				}	
			}

			stateMachine.UpdateState(GameState.GameStates.DONE);
		}

		/*================================================================================
		TUTORIAL STEP MANAGEMENT		
		=================================================================================*/
		/// <summary>
		/// Callback from the <see cref="TutorialStep.OnComplete"/>, signifies the end of a step
		/// </summary>
		/// <param name="step"></param>
		internal void OnTutorialStepComplete(TutorialStep step)
		{
			currentStep++;
			
			OnStepComplete?.Invoke(step);

			if (currentStep >= steps.Count)
			{
				stateMachine.UpdateState(GameState.GameStates.DONE);
				OnTutorComplete?.Invoke();
				TutorDirector.RemoveTutor(this);
			}
			else if (autoRun)
			{
				Run();
			}
		}

		/// <summary>
		/// Skip the current step
		/// </summary>
		public void Skip()
		{
			OnTutorialStepComplete(null);
		}

		/// <summary>
		/// Moves the current step index
		/// </summary>
		/// <param name="step"></param>
		public void SkipTo(int step)
		{
			currentStep = step;
		}

		/// <summary>
		/// Moves the current step to the last tutorial
		/// </summary>
		public void SkipToEnd()
		{
			currentStep = steps.Count - 1;
		}

		/*================================================================================
		OBJECT MANAGEMENT		
		=================================================================================*/
		/// <summary>
		/// Re-parent the game object to the tutor parent
		/// </summary>
		/// <param name="obj"></param>
		internal void ReparentObject(GameObject obj)
		{
			obj.transform.SetParent(transform);
		}

		/// <summary>
		/// Re-parent the game object to the tutor parent, with a new position
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="position"></param>
		internal void ReparentObject(GameObject obj, Vector3 position)
		{
			obj.transform.SetParent(transform);
			obj.transform.localPosition = position;
		}

		/// <summary>
		/// Returns the message box prefab if there is one
		/// </summary>
		/// <returns></returns>
		internal TutorialPack GetTutorialPack()
		{
			if (messageBoxPrefab != null && messageBox == null)
			{
				messageBox = CommonUtils.CreateGameObject(messageBoxPrefab, transform, true).GetComponent<TutorialPack>();
				messageBox.transform.SetParent(transform);
				
			}
			return messageBox;
		}
		
		/*================================================================================
		ANCILLARY		
		=================================================================================*/
		/// <summary>
		/// Returns the current tutorial step index
		/// </summary>
		public int currentTutorialStepIndex
		{
			get
			{
				return currentStep;
			}
		}

		/// <summary>
		/// Returns the total number of tutorial steps
		/// </summary>
		public int totalSteps
		{
			get
			{
				return steps.Count;
			}
		}
		
		/// <summary>
		/// Returns the current TutorialStep instance if there is one
		/// </summary>
		public TutorialStep currentTutorial
		{
			get
			{
				if (steps != null && steps.Count > 0 && currentStep < steps.Count)
				{
					return steps[currentStep];
				}

				return null;
			}
		}

		/// <summary>
		/// Returns true if we are on the last tutorial
		/// </summary>
		public bool isLastTutorial
		{
			get
			{
				return currentStep == steps.Count - 1;
			}
		}

		/// <summary>
		/// Returns true if execution of a tutorial step is taking place
		/// </summary>
		public bool isExecuting
		{
			get
			{
				if (steps != null && steps.Count > 0 && currentStep < steps.Count)
				{
					return steps[currentStep].isExecuting && !steps[currentStep].isComplete;
				}

				return false;
			}
		}

		/// <summary>
		/// Returns true when the tutor is finished
		/// </summary>
		public bool isFinished
		{
			get
			{
				return stateMachine.isState(GameState.GameStates.DONE);
			}
		}

		/// <summary>
		/// Returns the name set in <see cref="tutorName"/>
		/// </summary>
		public new string name
		{
			get
			{
				return tutorName;
			}
		}
	}
}