using UnityEngine;

namespace Code.Common.Tutor
{
	/// <summary>
	/// Tutorial step that displays a message to the user
	/// </summary>
	[CreateAssetMenu(fileName = "TutorialMessageStep", menuName = "ScriptableObjects/Tutor/TutorialMessageStep", order = 1)]
	public class TutorialMessageStep : TutorialStep
	{
		/// <inheritdoc/>
		internal override void RenderMessage()
		{
			base.RenderMessage();
			
			if (!string.IsNullOrEmpty(message) && tutor != null && tutor.GetTutorialPack() != null)
			{
				TutorialPack messageBox = tutor.GetTutorialPack();

				messageBox.AddHandler(OnButtonClick);
				
				messageBox.ToggleButton(true);
			}
		}

		/// <summary>
		/// On button click removes the handler, and completes this step
		/// </summary>
		private void OnButtonClick()
		{
			TutorialPack messageBox = tutor.GetTutorialPack();

			if (messageBox != null)
			{
				messageBox.RemoveHandler(OnButtonClick);
			}
			
			OnComplete();
		}
	}
}