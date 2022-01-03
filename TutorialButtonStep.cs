using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Code.Common.Tutor
{
	/// <summary>
	/// Tutorial step that waits for a button interaction before proceeding
	/// </summary>
	[CreateAssetMenu(fileName = "TutorialButtonStep", menuName = "ScriptableObjects/Tutor/TutorialButtonStep", order = 1)]
	public class TutorialButtonStep : TutorialStep
	{
		//====================
		// PROTECTED
		//====================
		[SerializeField] protected string buttonName;
		
		[Tooltip("Set the target to transform/reparent. If null, uses the button object instead")]
		[SerializeField] protected string buttonTransformTarget;

		/// <summary>
		/// Button parent
		/// </summary>
		protected Transform buttonParent;

		/// <summary>
		/// Cache reference to a button
		/// </summary>
		private Selectable buttonRef;

		/// <summary>
		/// Stores the current button onClick unity event, before setting it to a new one
		/// </summary>
		private UnityEvent originalOnClick;

		/// <summary>
		/// Button step handles looking at a button reference, attaching to the callbacks
		/// and making sure the user clicks that before completing
		/// </summary>
		internal override void Execute()
		{
			base.Execute();
			
			if (button == null)
			{
				OnComplete();
			}

			buttonParent = button.transform.parent;

			AddHandler(OnComplete);

			if (tutor.GetTutorialPack() != null)
			{
				tutor.GetTutorialPack().ToggleButton(false);
			}

			GameObject target = !string.IsNullOrEmpty(buttonTransformTarget) ? GameObject.Find(buttonTransformTarget) : button.gameObject; 
			tutor.ReparentObject(target);
		}

		protected void AddHandler(UnityAction action)
		{
			if (button is ImageStateButton stateButton)
			{
				originalOnClick = stateButton.onClick;
				stateButton.onClick = new UnityEvent();
				stateButton.AddHandler(action);
			}
			else
			{
				originalOnClick = ((Button)button).onClick;
				((Button)button).onClick = new Button.ButtonClickedEvent();
				((Button)button).onClick.AddListener(action);
			}
		}

		protected void RemoveHandler(UnityAction action)
		{
			if (button is ImageStateButton)
			{
				((ImageStateButton)button).RemoveHandler(action);
			}
			else
			{
				((Button)button).onClick.AddListener(action);
			}
		}

		/// <summary>
		/// Restore the buttons on click events, and dispatches them
		/// </summary>
		internal override void Restore()
		{
			if (button is ImageStateButton stateButton)
			{
				stateButton.onClick = originalOnClick;
			}
			else
			{
				((Button)button).onClick = originalOnClick as Button.ButtonClickedEvent;
			}
			
			GameObject target = !string.IsNullOrEmpty(buttonTransformTarget) ? GameObject.Find(buttonTransformTarget) : button.gameObject;
			target.transform.SetParent(buttonParent);
		}
		
		/// <summary>
		/// Restore the buttons on click events, and dispatches them
		/// </summary>
		internal override void InvokeEvents()
		{
			if (button is ImageStateButton stateButton)
			{
				stateButton.onClick?.Invoke();
			}
			else
			{
				((Button)button).onClick?.Invoke();
			}
		}

		/// <inheritdoc/>
		public override void OnComplete()
		{
			RemoveHandler(OnComplete);
			Restore();
			InvokeEvents();
			base.OnComplete();
		}
		
		/*================================================================================
		ANCILLARY		
		=================================================================================*/
		public Selectable button
		{
			get
			{
				if (buttonRef == null)
				{
					GameObject obj = GameObject.Find(buttonName);

					if (obj != null)
					{
						return obj.GetComponent<Selectable>();
					}
				}

				return buttonRef;
			}
		}
	}
}