using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Common.Tutor
{
	/// <summary>
	/// Message box, pointers, swipes prefab/package
	/// </summary>
	public class TutorialPack : MonoBehaviour
	{
		//====================
		// PROTECTED
		//====================
		[Header("Messaging")]
		[SerializeField] protected GameObject messageParent;
		[SerializeField] protected TextMeshProUGUI message;
		
		[Header("Buttons")]
		[SerializeField] protected Button button;
		
		[Header("Swiping")]
		[SerializeField] protected GameObject swipe;
		[SerializeField] protected Animator swipeAnimator;
		
		[Header("Shroud")]
		[SerializeField] protected GameObject shroud;
		[SerializeField] protected Animator shroudAnimator;
		[SerializeField] protected StretchTransformWidthHeight shroudStretch;

		[Header("Element parenting")]
		[Tooltip("Used for adding objects to re-parent for tutorial purposes (like clicking buttons)")]
		[SerializeField] protected RectTransform dynamicParent;
		
		//====================
		// CONST
		//====================
		private const string SWIPE_ANIMATION_UP = "Tutor - Swipe up";
		private const string SWIPE_ANIMATION_DOWN = "Tutor - Swipe down";
		private const string SHROUD_ANIMATION_IN = "Tutor - Shroud in";
		private const string SHROUD_ANIMATION_OUT = "Tutor - Shroud out";
		
		/// <summary>
		/// Dispatched on click
		/// </summary>
		protected Action CallbackHandler;

		/*================================================================================
		EVENT HANDLING		
		=================================================================================*/
		/// <summary>
		/// Add a method to be called when the message box button is clicked
		/// </summary>
		/// <param name="callback"></param>
		public void AddHandler(Action callback)
		{
			CallbackHandler += callback;
		}

		/// <summary>
		/// Remove a callback handler from the button dispatch
		/// </summary>
		/// <param name="callback"></param>
		public void RemoveHandler(Action callback)
		{
			CallbackHandler -= callback;
		}

		/// <summary>
		/// Button callback
		/// </summary>
		public virtual void OnClickNext()
		{
			CallbackHandler?.Invoke();
		}
		
		/*================================================================================
		STATE/VIEW TOGGLES		
		=================================================================================*/
		/// <summary>
		/// Toggle the message box on/off
		/// </summary>
		/// <param name="active"></param>
		public void ToggleMessage(bool active)
		{
			SafeSet.SetActive(messageParent, active);
		}

		/// <summary>
		/// Toggle the swipe object on/off
		/// </summary>
		/// <param name="active"></param>
		public void ToggleSwipe(bool active)
		{
			SafeSet.SetActive(swipe, active);
		}

		/// <summary>
		/// Toggle the "ok" button on/off
		/// </summary>
		/// <param name="active"></param>
		public void ToggleButton(bool active)
		{
			SafeSet.SetActive(button, active);
		}

		/// <summary>
		/// Sets the message text
		/// </summary>
		/// <param name="text"></param>
		public virtual void SetMessage(string text)
		{
			message.text = text;
		}

		/*================================================================================
		ANIMATIONS		
		=================================================================================*/
		public virtual void PlaySwipeUp()
		{
			swipeAnimator.Play(SWIPE_ANIMATION_UP);
		}

		public virtual void PlaySwipeDown()
		{
			swipeAnimator.Play(SWIPE_ANIMATION_DOWN);
		}

		public virtual void PlayShroudIn()
		{
			if (!isShroudShowing)
			{
				shroudAnimator.Play(SHROUD_ANIMATION_IN);
			}
		}

		public virtual void PlayShroudOut()
		{
			if (isShroudShowing)
			{
				shroudAnimator.Play(SHROUD_ANIMATION_OUT);
			}
		}
		
		/*================================================================================
		ANCILLARY		
		=================================================================================*/
		protected bool isShroudShowing
		{
			get
			{
				return shroud.activeInHierarchy;
			}
		}
	}
}