#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues
{
	using System.Collections.Generic;
	using Core;
	using Detectors;
	using Settings;
	using Tools;
	using UnityEditor;
	using UnityEngine;
	using Object = UnityEngine.Object;

	internal static class IssuesDetector
	{
		private static MissingReferenceDetector missingReferenceDetector;
		private static MissingComponentDetector missingComponentDetector;
		internal static DuplicateComponentDetector duplicateComponentDetector;
		private static MissingPrefabDetector missingPrefabDetector;
		private static InconsistentTerrainDataDetector inconsistentTerrainDataDetector;
		private static HugePositionDetector hugePositionDetector;
		private static EmptyLayerNameDetector emptyLayerNameDetector;
		private static DuplicateLayersDetector duplicateLayersDetector;

#if UNITY_2019_1_OR_NEWER
		private static ShaderErrorDetector shaderErrorDetector;
#endif

		private static AssetInfo currentAsset;
		private static RecordLocation currentLocation;
		private static GameObject currentGameObject;

		public static void Init(List<IssueRecord> issues)
		{
			missingReferenceDetector = new MissingReferenceDetector(issues);

			missingComponentDetector = new MissingComponentDetector(issues);
			duplicateComponentDetector = new DuplicateComponentDetector(issues);
			inconsistentTerrainDataDetector = new InconsistentTerrainDataDetector(issues);
			missingPrefabDetector = new MissingPrefabDetector(issues);

			hugePositionDetector = new HugePositionDetector(issues);
			emptyLayerNameDetector = new EmptyLayerNameDetector(issues);

			duplicateLayersDetector = new DuplicateLayersDetector(issues);
#if UNITY_2019_1_OR_NEWER
			shaderErrorDetector = new ShaderErrorDetector(issues);
#endif
		}

		/////////////////////////////////////////////////////////////////////////
		// Scenes Processing
		/////////////////////////////////////////////////////////////////////////

		public static void SceneStart(AssetInfo asset)
		{
			currentLocation = RecordLocation.Scene;
			currentAsset = asset;

			missingReferenceDetector.TryDetectIssuesInSceneSettings(currentAsset);
		}

		public static void SceneEnd(AssetInfo asset)
		{
			currentLocation = RecordLocation.Unknown;
			currentAsset = null;
			currentGameObject = null;
		}

		/////////////////////////////////////////////////////////////////////////
		// Prefab Assets Processing
		/////////////////////////////////////////////////////////////////////////

		public static void StartPrefabAsset(AssetInfo asset)
		{
			currentLocation = RecordLocation.Prefab;
			currentAsset = asset;
		}

		public static void EndPrefabAsset(AssetInfo asset)
		{
			currentLocation = RecordLocation.Unknown;
			currentAsset = null;
			currentGameObject = null;
		}

		/////////////////////////////////////////////////////////////////////////
		// Game Objects Processing (both from Scenes and Prefab Assets)
		/////////////////////////////////////////////////////////////////////////

		public static bool StartGameObject(GameObject target, bool inPrefabInstance, out bool skipTree)
		{
			skipTree = false;

			if (!ProjectSettings.Issues.touchInactiveGameObjects)
			{
				if (currentLocation == RecordLocation.Scene)
				{
					if (!target.activeInHierarchy) return false;
				}
				else
				{
					if (!target.activeSelf) return false;
				}
			}

			if (inPrefabInstance)
			{
				if (missingPrefabDetector.TryDetectIssue(currentLocation, currentAsset.Path, target))
				{
					skipTree = true;
					return false;
				}
			}

			currentGameObject = target;
			missingComponentDetector.StartGameObject();
			duplicateComponentDetector.StartGameObject();

			hugePositionDetector.TryDetectIssue(currentLocation, currentAsset.Path, target);
			emptyLayerNameDetector.TryDetectIssue(currentLocation, currentAsset.Path, target);

			return true;
		}

		public static void EndGameObject(GameObject target)
		{
			missingComponentDetector.TryDetectIssue(currentLocation, currentAsset.Path, target);
			inconsistentTerrainDataDetector.TryDetectIssue(currentLocation, currentAsset.Path, target);
		}

		/////////////////////////////////////////////////////////////////////////
		// Game Object's Components Processing
		/////////////////////////////////////////////////////////////////////////

		public static void ProcessComponent(Component component, int orderIndex)
		{
#if !UNITY_2019_1_OR_NEWER
			if (missingComponentDetector.CheckAndRecordNullComponent(component))
			{
				return;
			}
#else
			if (component == null) return;
#endif

			if ((component.hideFlags & HideFlags.HideInInspector) != 0)
			{
				return;
			}

			if (!ProjectSettings.Issues.touchDisabledComponents)
			{
				if (EditorUtility.GetObjectEnabled(component) == 0) return;
			}

			var componentType = component.GetType();
			var componentName = componentType.Name;

			if (ProjectSettings.Issues.componentIgnoresFilters != null &&
				ProjectSettings.Issues.componentIgnoresFilters.Length > 0)
			{
				if (CSFilterTools.IsValueMatchesAnyFilterOfKind(componentName, ProjectSettings.Issues.componentIgnoresFilters, FilterKind.Type)) return;
			}

			duplicateComponentDetector.ProcessComponent(component, componentType);

			var shouldCheckPropertiesForDuplicate = duplicateComponentDetector.IsPropertiesHashCalculationRequired();
			if (shouldCheckPropertiesForDuplicate)
			{
				// skipping duplicate search for non-standard components with invisible properties
				var baseType = componentType.BaseType;
				if (baseType != null)
				{
					if (baseType.Name == "MegaModifier")
					{
						shouldCheckPropertiesForDuplicate = false;
						duplicateComponentDetector.SkipComponent();
					}
				}
			}

			var shouldTraverseProperties = missingReferenceDetector.Enabled || shouldCheckPropertiesForDuplicate;
			if (shouldTraverseProperties)
			{
				var initialInfo = new SerializedObjectTraverseInfo(component);

				CSTraverseTools.TraverseObjectProperties(initialInfo, (info, property) =>
				{
					if (property.type == "UnityEvent")
					{
						missingReferenceDetector.TryDetectUnityEventIssues(currentLocation, currentAsset.Path,
							currentGameObject, componentType, componentName, orderIndex, property);

						info.skipCurrentTree = true;
						return;
					}

					missingReferenceDetector.TryDetectIssue(currentLocation, currentAsset.Path, currentGameObject, componentType, componentName, orderIndex, property);

					if (shouldCheckPropertiesForDuplicate) duplicateComponentDetector.ProcessProperty(property);
				});
			}

			if (shouldCheckPropertiesForDuplicate)
			{
				duplicateComponentDetector.TryDetectIssue(currentLocation, currentAsset.Path, currentGameObject, componentType, componentName, orderIndex);
			}

			if (component is Terrain)
			{
				inconsistentTerrainDataDetector.ProcessTerrainComponent((Terrain)component, componentType, componentName, orderIndex);
			}
			else if (component is TerrainCollider)
			{
				inconsistentTerrainDataDetector.ProcessTerrainColliderComponent((TerrainCollider)component);
			}
			//Debug.Log("ProcessComponent: " + target.name + ":" + component);
		}

		/////////////////////////////////////////////////////////////////////////
		// Scriptable Objects Processing
		/////////////////////////////////////////////////////////////////////////

		public static void ProcessScriptableObject(AssetInfo asset, Object scriptableObject)
		{
			currentLocation = RecordLocation.Asset;

			if (missingComponentDetector.TryDetectScriptableObjectIssue(asset.Path,
				scriptableObject))
			{
				return;
			}

			var shouldTraverseProperties = missingReferenceDetector.Enabled;
			if (shouldTraverseProperties)
			{
				var initialInfo = new SerializedObjectTraverseInfo(scriptableObject);
				CSTraverseTools.TraverseObjectProperties(initialInfo, (info, property) =>
				{
					if (property.type == "UnityEvent")
					{
						missingReferenceDetector.TryDetectScriptableObjectUnityEventIssue(asset.Path,
							info.TraverseTarget.GetType().Name, property);

						info.skipCurrentTree = true;
						return;
					}

					missingReferenceDetector.TryDetectScriptableObjectIssue(asset.Path,
						info.TraverseTarget.GetType().Name, property);
				});
			}

			currentLocation = RecordLocation.Unknown;
		}

		/////////////////////////////////////////////////////////////////////////
		// Settings Assets Processing
		/////////////////////////////////////////////////////////////////////////

		public static void ProcessSettingsAsset(AssetInfo asset)
		{
			currentLocation = RecordLocation.Asset;

			var kind = asset.SettingsKind;

			missingReferenceDetector.TryDetectIssuesInSettingsAsset(asset, kind);

			if (kind == AssetSettingsKind.TagManager)
			{
				duplicateLayersDetector.TryDetectIssue();
			}

			currentLocation = RecordLocation.Unknown;
		}

		/////////////////////////////////////////////////////////////////////////
		// Shaders Assets Processing
		/////////////////////////////////////////////////////////////////////////

#if UNITY_2019_1_OR_NEWER
		public static void ProcessShader(AssetInfo asset, Shader shader)
		{
			shaderErrorDetector.TryDetectIssue(asset, shader);
		}
#endif
	}
}
