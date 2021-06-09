#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues
{
	using System;
	using System.Linq;
	using Detectors;
	using Settings;
	using UnityEditor;
	using UnityEngine;
	using Tools;
	using Object = UnityEngine.Object;

	internal static class IssuesFixer
	{
		public static void FixRecords(IssueRecord[] results, bool showProgress = true)
		{
			var i = 0;
			var count = results.Length;

			var sortedRecords = results.OrderBy(RecordsSortings.issueRecordByPath).ToArray();
			
#if !UNITY_2020_1_OR_NEWER
			var updateStep = Math.Max(count / ProjectSettings.UpdateProgressStep, 1);
#endif

			for (var k = 0; k < count; k++)
			{
				var item = sortedRecords[k];

				if (showProgress)
				{
#if !UNITY_2020_1_OR_NEWER
					if (k % updateStep == 0)
#endif
					{
						if (IssuesFinder.ShowProgressBar(1, 1, i, count, "Resolving selected issues..."))
						{
							IssuesFinder.operationCanceled = true;
							break;
						}
					}
				}

				if (item.selected && item.IsFixable)
				{
					if (item.Location == RecordLocation.Scene)
					{
						var assetIssue = item as AssetIssueRecord;
						if (assetIssue != null)
						{
							var newOpenSceneResult = CSSceneTools.OpenScene(assetIssue.Path);
							if (!newOpenSceneResult.success)
							{
								continue;
							}

							if (newOpenSceneResult.sceneWasLoaded)
							{
								if (IssuesFinder.lastOpenSceneResult != null)
								{
									CSSceneTools.SaveScene(IssuesFinder.lastOpenSceneResult.scene);
									CSSceneTools.CloseOpenedSceneIfNeeded(IssuesFinder.lastOpenSceneResult);
								}
							}

							if (IssuesFinder.lastOpenSceneResult == null || IssuesFinder.lastOpenSceneResult.scene != newOpenSceneResult.scene)
							{
								IssuesFinder.lastOpenSceneResult = newOpenSceneResult;
							}
						}
					}

					item.Fix(true);
					i++;
				}
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public static FixResult FixObjectIssue(GameObjectIssueRecord issue, Object obj, Component component, IssueKind type)
		{
			FixResult result;
			if (type == IssueKind.MissingComponent)
			{
				var go = (GameObject)obj;
				var hasIssue = GameObjectHasMissingComponent(go);

				if (hasIssue)
				{
					if (PrefabUtility.IsPartOfPrefabAsset(go))
					{
						var allTransforms = go.transform.root.GetComponentsInChildren<Transform>(true);
						foreach (var child in allTransforms)
						{
							FixMissingComponents(issue, child.gameObject, false);
						}
					}
					else
					{
						FixMissingComponents(issue, go, false);
					}
					
					if (!GameObjectHasMissingComponent(go))
					{
						result = new FixResult(true);
					}
					else
					{
						result = FixResult.CreateError("Fix attempt failed!");
					}
				}
				else
				{
					result = new FixResult(true);
				}
			}
			else if (type == IssueKind.MissingReference)
			{
				result = FixMissingReference(component != null ? component : obj, issue.propertyPath, issue.Location);
			}
			else
			{
				result = FixResult.CreateError("IssueKind is not supported!");
			}

			return result;
		}

		#region missing component

		// -----------------------------------------------------------------------------
		// fix missing component
		// -----------------------------------------------------------------------------
		
		private static bool FixMissingComponents(GameObjectIssueRecord issue, GameObject go, bool alternative)
		{
			var touched = false;

			// TODO: re-check in Unity 2021
			// unfortunately RemoveMonoBehavioursWithMissingScript does not works correctly:
			// https://forum.unity.com/threads/remove-all-missing-components-in-prefabs.897761/
			// so it will be enabled back in later Unity versions
			/*var removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
			if (removedCount > 0)
			{
				touched = true;
			}*/

			if (!alternative)
			{
				CSObjectTools.SelectGameObject(go, issue.Location == RecordLocation.Scene);
			}

			var tracker = CSEditorTools.GetActiveEditorTrackerForSelectedObject();
			if (tracker == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't get active tracker."));
				return false;
			}
			tracker.RebuildIfNecessary();

			var activeEditors = tracker.activeEditors;
			for (var i = activeEditors.Length - 1; i >= 0; i--)
			{
				var editor = activeEditors[i];
				if (editor.serializedObject.targetObject == null)
				{
					Object.DestroyImmediate(editor.target, true);
					touched = true;
				}
			}

			if (alternative)
			{
				return touched;
			}

			if (!touched)
			{
				// missing script could be hidden with hide flags, so let's try select it directly and repeat
				var serializedObject = new SerializedObject(go);
				var componentsArray = serializedObject.FindProperty("m_Component");
				if (componentsArray != null)
				{
					for (var i = componentsArray.arraySize - 1; i >= 0; i--)
					{
						var componentPair = componentsArray.GetArrayElementAtIndex(i);
						var nestedComponent = componentPair.FindPropertyRelative("component");
						if (nestedComponent != null)
						{
							if (MissingReferenceDetector.IsPropertyHasMissingReference(nestedComponent))
							{
								var instanceId = nestedComponent.objectReferenceInstanceIDValue;
								if (instanceId == 0)
								{
									var fileId = nestedComponent.FindPropertyRelative("m_FileID");
									if (fileId != null)
									{
										instanceId = fileId.intValue;
									}
								}

								Selection.instanceIDs = new []{ instanceId };
								touched |= FixMissingComponents(issue, go, true);
							}
						}
						else
						{
							Debug.LogError(Maintainer.LogPrefix + "Couldn't find component in component pair!");
							break;
						}
					}

					if (touched)
					{
						CSObjectTools.SelectGameObject(go, issue.Location == RecordLocation.Scene);
					}
				}
				else
				{
					Debug.LogError(Maintainer.LogPrefix + "Couldn't find components array!");
				}
			}

			if (touched)
			{
				if (issue.Location == RecordLocation.Scene)
				{
					CSSceneTools.MarkSceneDirty();
				}
				else
				{
					EditorUtility.SetDirty(go);
				}
			}

			return touched;
		}

		private static bool GameObjectHasMissingComponent(GameObject go)
		{
			var hasMissingComponent = false;
			var components = go.GetComponents<Component>();
			foreach (var c in components)
			{
				if (c == null)
				{
					hasMissingComponent = true;
					break;
				}
			}
			return hasMissingComponent;
		}
		
		#endregion

		#region missing reference
		// -----------------------------------------------------------------------------
		// fix missing reference
		// -----------------------------------------------------------------------------

		public static FixResult FixMissingReference(Object unityObject, string propertyPath, RecordLocation location)
		{
			var so = new SerializedObject(unityObject);
			var sp = so.FindProperty(propertyPath);

			if (MissingReferenceDetector.IsPropertyHasMissingReference(sp))
			{
				sp.objectReferenceInstanceIDValue = 0;

				var fileId = sp.FindPropertyRelative("m_FileID");
				if (fileId != null)
				{
					fileId.intValue = 0;
				}

				// fixes dirty scene flag after batch issues fix
				// due to the additional undo action
				so.ApplyModifiedPropertiesWithoutUndo();

				if (location == RecordLocation.Scene)
				{
					CSSceneTools.MarkSceneDirty();
				}
				else
				{
					if (unityObject != null) EditorUtility.SetDirty(unityObject);
				}
			}

			return new FixResult(true);
		}
		#endregion
	}
}