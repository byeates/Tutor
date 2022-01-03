using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Code.Common.Tutor
{
	/// <summary>
	/// Run time only generated callbacks
	/// </summary>
	[CreateAssetMenu(fileName = "TutorialCallbackStep", menuName = "ScriptableObjects/Tutor/TutorialCallbackStep", order = 1)]
	public class TutorialCallbackStep : TutorialStep
	{
		/// <summary>
		/// Fully qualified path to method. E.g. CommonUtils.Method
		/// </summary>
		[Tooltip("Fully qualified path to method. E.g. CommonUtils.Method")]
		[SerializeField] protected string qualifiedMethod;

		/// <summary>
		/// List of flags to add to the reflection
		/// </summary>
		[Tooltip("Binding flags for reflection")]
		[SerializeField] protected List<BindingFlags> flags;
		
		/// <summary>
		/// Assembly to look in
		/// </summary>
		[Tooltip("Assembly to search, if none is specified, searches all assemblies")]
		[SerializeField] protected string assembly;

		/// <summary>
		/// Assembly to look in
		/// </summary>
		[Tooltip("Optional parameters to pass into the method call")] 
		[SerializeField] protected object[] parameters;
		
		public override void OnComplete()
		{
			base.OnComplete();

			if (string.IsNullOrEmpty(assembly) || string.IsNullOrEmpty(qualifiedMethod))
			{
				return;
			}

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			
			foreach (Assembly asm in assemblies)
			{
				if (asm.FullName.Contains(assembly))
				{
					string[] callInfo = qualifiedMethod.Split('.');

					// partial qualified validation
					if (callInfo.Length <= 1)
					{
						return;
					}
					
					// class name
					string top = callInfo[0];
					
					// method name
					string methodName = callInfo[callInfo.Length - 1];

					// grab types from asssembly
					var types = asm.GetTypes().Where(t => t.FullName == top);
					
					// compress bindingflags
					BindingFlags flag = 0;
					for (int i = 0; i < flags.Count; ++i)
					{
						flag |= flags[i];
					}

					// setup for method call
					MethodInfo methodInfo;
					
					// target we are invoking on
					Type target;
					
					// search for type, first found terminates the run
					foreach (Type type in types)
					{
						target = type;
						
						// check for nested properties
						if (callInfo.Length > 2)
						{
							MemberInfo info = null;
							for (int i = 1; i < callInfo.Length - 1; ++i)
							{
								info = type.GetField(callInfo[i]) as MemberInfo ?? type.GetProperty(callInfo[i]);
							}

							if (info != null)
							{
								target = info.GetType();
							}
						}
						
						methodInfo = target.GetMethod(methodName, flag);

						if (methodInfo != null)
						{
							methodInfo.Invoke(null, parameters);
						}

						break;
					}
				}
			}
		}
	}
}