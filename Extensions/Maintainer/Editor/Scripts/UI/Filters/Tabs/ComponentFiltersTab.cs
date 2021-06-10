#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI.Filters
{
	using System;
	using UnityEditor;
	using UnityEngine;
	using Core;
	using Tools;

	internal class ComponentFiltersTab : StringFiltersTab
	{
		internal ComponentFiltersTab(FilterType filterType, FilterItem[] filtersList, SaveFiltersCallback saveCallback) : base(filterType, filtersList, saveCallback)
		{
			caption = new GUIContent("Component <color=" +
										(filterType == FilterType.Includes ? "#02C85F" : "#FF4040FF") + ">" + filterType + "</color>", CSEditorIcons.ScriptIcon);
		}

		protected override void DrawTabHeader()
		{
			EditorGUILayout.LabelField("Here you may specify names of Components you'd like to <color=" +
										(filterType == FilterType.Includes ? "#02C85F" : "#FF4040FF") + "><b>" +
										(filterType == FilterType.Ignores ? "ignore" : "include") + "</b></color> during the Issues Search.\n"+
									   "You may drag & drop components to this window from the Inspector.", UIHelpers.richWordWrapLabel);
			EditorGUILayout.LabelField("Examples:\n" +
									   "MeshFilter\n" +
									   "DOTweenAnimation",
									   EditorStyles.wordWrappedMiniLabel);
		}

		protected override bool CheckNewItem(ref string newItem)
		{
			var found = false;

			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var t in a.GetTypes())
				{
					if (t.Name == newItem)
					{
						found = true;
						break;
					}
				}

				if (found) break;
			}

			if (!found)
			{
				EditorUtility.DisplayDialog("Can't find specified Component", "Specified component " + newItem + " wasn't found in usual places. Make sure you've entered valid name.", "Cool, thanks!");
			}

			return found;
		}

		internal override void ProcessDrags()
		{
			if (currentEventType != EventType.DragUpdated && currentEventType != EventType.DragPerform) return;

			var objects = DragAndDrop.objectReferences;

			if (objects != null && objects.Length > 0)
			{
				var canDrop = false;

				for (var i = 0; i < objects.Length; i++)
				{
					if (objects[i] is Component)
					{
						canDrop = true;
						break;
					}

					var monoScript = objects[i] as MonoScript;
					if (monoScript != null)
					{
						var type = monoScript.GetClass();
						if (type.IsSubclassOf(CSReflectionTools.componentType))
						{
							canDrop = true;
							break;
						}
					}
				}

				if (canDrop)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

					if (currentEventType == EventType.DragPerform)
					{
						var needToSave = false;
						var needToShowWarning = false;
						var noComponent = false;
						var invalidComponent = false;

						for (var i = 0; i < objects.Length; i++)
						{
							var component = objects[i] as Component;
							var monoScript = objects[i] as MonoScript;
							string componentName = null;

							if (component != null)
							{
								componentName = component.GetType().Name;
							}
							else if (monoScript != null)
							{
								var type = monoScript.GetClass();
								if (type.IsSubclassOf(CSReflectionTools.componentType))
								{
									componentName = type.Name;
								}
								else
								{
									noComponent = true;
								}
							}
							else
							{
								noComponent = true;
							}

							if (noComponent) continue;

							if (!string.IsNullOrEmpty(componentName) && componentName != "Object" && componentName != "Component" && componentName != "Behaviour")
							{
								var added = CSFilterTools.TryAddNewItemToFilters(ref filters, FilterItem.Create(componentName, FilterKind.Type));
								needToSave |= added;
								needToShowWarning |= !added;
							}
							else
							{
								invalidComponent = true;
							}
						}

						if (needToSave)
						{
							SaveChanges();
						}

						var warningText = "";

						if (needToShowWarning)
						{
							warningText = "One or more of the dragged items already present in the list!";
						}

						if (noComponent)
						{
							if (!string.IsNullOrEmpty(warningText))
							{
								warningText += "\n";
							}
							warningText += "One or more of the dragged items are not the Components!";
						}

						if (invalidComponent)
						{
							if (!string.IsNullOrEmpty(warningText))
							{
								warningText += "\n";
							}
							warningText += "Can't detect valid name for one or more of the dragged items!";
						}

						if (!string.IsNullOrEmpty(warningText)) window.ShowNotification(new GUIContent(warningText));

						DragAndDrop.AcceptDrag();
					}
				}
			}
			Event.current.Use();
		}
	}
}