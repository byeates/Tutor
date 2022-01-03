using System;
using System.Collections.Generic;

namespace Code.Common.Tutor
{
	/// <summary>
	/// Management system for Tutor instances, allows for a centralized location
	/// for running, finding, viewing tutor instances. Avoids creating references in random places
	/// throughout the codebase. This is a FIFO based tutorial system, whenever someone calls
	/// <see cref="Tutor.Run"/>, that has to be executed in the order it happened
	/// </summary>
	public class TutorDirector : IDestructible
	{
		//====================
		// PRIVATE
		//====================
		private static List<Tutor> tutors = new List<Tutor>();

		/// <summary>
		/// List of tutors that Run() was called on, but we were already executing a tutor
		/// </summary>
		private static Queue<Tutor> queue = new Queue<Tutor>();

		/// <summary>
		/// Add a tutor instance (done automatically from <see cref="Tutor.Awake()"/>
		/// </summary>
		/// <param name="tutor"></param>
		public static void AddTutor(Tutor tutor)
		{
			if (!tutors.Contains(tutor))
			{
				tutors.Add(tutor);
			}
		}

		/// <summary>
		/// Removes a tutor instance (done automatically from <see cref="Tutor.OnDestroy()"/>.
		/// It will then attempt to run any more tutors in the queue
		/// </summary>
		/// <param name="tutor"></param>
		/// <param name="runNext">true by default, will run the next Tutor in the queue</param>
		public static void RemoveTutor(Tutor tutor, bool runNext = true)
		{
			if (tutors.Contains(tutor))
			{
				tutors.Remove(tutor);
			}

			if (runNext)
			{
				Run();	
			}
		}

		/// <summary>
		/// Add a tutor instance to be executed
		/// </summary>
		/// <param name="tutor"></param>
		/// <param name="runIfAvailable">if set to false, won't run a queued tutor until Run() is called</param>
		internal static void QueueTutor(Tutor tutor, bool runIfAvailable = true)
		{
			if (!queue.Contains(tutor))
			{
				queue.Enqueue(tutor);
			}

			if (!isBusy && runIfAvailable)
			{
				Run();
			}
		}

		/// <summary>
		/// Runs the first tutor instance in the list if desired, you can also just call <see cref="Tutor.Run"/>
		/// </summary>
		public static void Run()
		{
			if (!isBusy)
			{
				// check if the queue has anything to run
				if (queue.Count > 0)
				{
					Tutor t = queue.Dequeue();
					
					while (t.isExecuting)
					{
						if (queue.Count > 0)
						{
							t = queue.Dequeue();
						}
						else
						{
							break;
						}
					}
					
					t.Run();
				}
			}
		}

		/// <summary>
		/// IDestructible implementation, clears the tutor references
		/// </summary>
		public static void Destroy()
		{
			tutors.Clear();
		}
		
		/*================================================================================
		ANCILLARY		
		=================================================================================*/
		/// <summary>
		/// Searches a partial match name (contains check) for a tutor instance
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Tutor GetTutorByObjectName(string name)
		{
			for (int i = 0; i < tutors.Count; ++i)
			{
				if (tutors[i].name.Contains(name))
				{
					return tutors[i];
				}
			}

			return null;
		}
		
		/// <summary>
		/// Searches a partial match name (contains check) for a tutor instance, returns true if it exists
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool HasTutorByObjectName(string name)
		{
			return GetTutorByObjectName(name) != null;
		}

		/// <summary>
		/// Returns true if any tutor is in the process of executing a tutorial
		/// </summary>
		public static bool isBusy
		{
			get
			{
				for (int i = 0; i < tutors.Count; ++i)
				{
					if (tutors[i].isExecuting)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Returns true if a tutor instance has a tutorial step in execution of the given Type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static bool IsExecutingStepOfType<T>() where T : TutorialStep
		{
			if (isBusy)
			{
				for (int i = 0; i < tutors.Count; ++i)
				{
					if (tutors[i].isExecuting)
					{
						TutorialStep step = tutors[i].currentTutorial;

						if (step is T)
						{
							return true;
						}
					}
				}
			}

			return false;
		}
		
		/// <summary>
		/// Returns true if a tutor instance has a tutorial step in execution of the given Type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static TutorialStep GetStepOfType<T>() where T : TutorialStep
		{
			if (isBusy)
			{
				for (int i = 0; i < tutors.Count; ++i)
				{
					if (tutors[i].isExecuting)
					{
						TutorialStep step = tutors[i].currentTutorial;

						if (step is T)
						{
							return step;
						}
					}
				}
			}

			return null;
		}
	}
}