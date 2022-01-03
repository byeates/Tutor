using UnityEngine;

namespace Code.Common.Tutor
{
	/// <summary>
	/// Tutorial step that will wait for another tutor instance to finish. 
	/// </summary>
	[CreateAssetMenu(fileName = "TutorialTutorStep", menuName = "ScriptableObjects/Tutor/TutorialTutorStep", order = 1)]
	public class TutorialTutorStep : TutorialStep
	{
		/// <summary>
		/// Searches for a tutor instance by name
		/// </summary>
		[Tooltip("Tutor name lookup")]
		[SerializeField] protected string tutorName;

		/// <summary>
		/// Tutor reference to attach to the oncomplete
		/// </summary>
		protected Tutor attachTutor;
		
		/// <summary>
		/// Will call <see cref="Tutor.Run()"/> when this step executes
		/// </summary>
		[Tooltip("When set to true it will call Tutor.Run() when this step executes")] 
		[SerializeField] protected bool runTutor;

		internal override void Execute()
		{
			base.Execute();

			attachTutor = TutorDirector.GetTutorByObjectName(tutorName);
			
			if (attachTutor != null)
			{
				if (!attachTutor.isFinished)
				{
					attachTutor.OnTutorComplete += OnComplete;

					if (runTutor)
					{
						attachTutor.Run();
					}
				}
				else
				{
					OnComplete();
				}
			}
		}
	}
}