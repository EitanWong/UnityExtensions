#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References.Entry
{
	using Core;
	using Settings;
	using System;
	using System.Collections.Generic;
	using Tools;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using Object = UnityEngine.Object;
	using UnityEditor.Experimental.SceneManagement;

	internal static class HierarchyEntryFinder
	{
#if !UNITY_2020_1_OR_NEWER
		private static int updateStep = -1;
#endif
		
		public static bool FillHierarchyReferenceEntries(Object[] objects, ProcessObjectReferenceHandler processReferenceCallback)
		{
			EntryFinder.currentProcessReferenceCallback = processReferenceCallback;
#if !UNITY_2020_1_OR_NEWER
			updateStep = -1;
#endif

			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null)
			{
				return ProcessPrefabForSceneScopeReferences(prefabStage);
			}

			var scenes = GetScenesFromObjects(objects);
			var rootObjects = GetRootGameObjectsFromScenes(scenes);
			return ProcessSceneForSceneScopeReferences(rootObjects);
		}

		private static bool ProcessPrefabForSceneScopeReferences(PrefabStage prefabStage)
		{
			EntryFinder.currentLocation = Location.PrefabAssetGameObject;
			return !CSTraverseTools.TraversePrefabGameObjects(prefabStage.prefabContentsRoot, false, true, EntryFinder.OnGameObjectTraverse);
		}
		
		private static bool ProcessSceneForSceneScopeReferences(GameObject[] rootObjects)
		{
			EntryFinder.currentLocation = Location.SceneGameObject;
			return !CSTraverseTools.TraverseRootGameObjects(rootObjects, false, true, EntryFinder.OnGameObjectTraverse, OnRootTraverse);
		}

		private static void OnRootTraverse(int index, int total, out bool canceled)
		{
#if !UNITY_2020_1_OR_NEWER
			if (updateStep == -1)
			{
				updateStep = Math.Max(total / ProjectSettings.UpdateProgressStep, 1);
			}
#endif
			
			canceled =
#if !UNITY_2020_1_OR_NEWER
				(index < 10 || index % updateStep == 0) &&
#endif
				EditorUtility.DisplayCancelableProgressBar(string.Format(ReferencesFinder.ProgressCaption, 2, ReferencesFinder.PhasesCount), 
				           string.Format(ReferencesFinder.ProgressText, "Filling reference details", index + 1, total),
				           (float)index / total);
		}
		
		private static Scene[] GetScenesFromObjects(Object[] objects)
		{
			var result = new List<Scene>(objects.Length);
			
			foreach (var o in objects)
			{
				if (o is Component)
				{
					var scene = (o as Component).gameObject.scene;
					if (!result.Contains(scene))
					{
						result.Add(scene);
					}
				}
				else if (o is GameObject)
				{
					var scene = (o as GameObject).scene;
					if (!result.Contains(scene))
					{
						result.Add(scene);
					}
				}
			}

			return result.ToArray();
		}

		private static GameObject[] GetRootGameObjectsFromScenes(Scene[] scenes)
		{
			var result = new List<GameObject>();
			
			foreach (var scene in scenes)
			{
				result.AddRange(scene.GetRootGameObjects());
			}

			return result.ToArray();
		}
	}
}