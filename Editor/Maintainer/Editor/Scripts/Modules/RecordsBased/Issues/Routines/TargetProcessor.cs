#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues.Routines
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Core;
	using Settings;
	using Tools;
	using UnityEditor;
	using UnityEditor.Experimental;
	using UnityEngine;

	internal static class TargetProcessor
	{
		private enum Phase:byte
		{
			Scenes = 1,
			Prefabs = 2,
			Rest = 3
		}

		private const int TotalPhases = 3;
#if !UNITY_2020_1_OR_NEWER
		private const int ObjectTraverseUpdateStep = 100;
#endif

		private static int currentObjectIndex;
		private static int itemIndex;
		private static int totalItems;

		private static string currentAssetName;
		private static List<IssueRecord> currentIssuesList;

		private delegate void ProcessAssetCallback(AssetInfo asset, string assetName, int itemIndex, int totalItems);

		internal static void SetIssuesList(List<IssueRecord> issues)
		{
			currentIssuesList = issues;
		}

		internal static void ProcessTargetAssets(AssetInfo[] targetAssets)
		{
			var sceneTargets = new List<AssetInfo>();
			var prefabTargets = new List<AssetInfo>();
			var restTargets = new List<AssetInfo>();

			foreach (var targetAsset in targetAssets)
			{
				var type = targetAsset.Type;

				if (type == CSReflectionTools.sceneAssetType)
				{
					sceneTargets.Add(targetAsset);
				}
				else if (type == CSReflectionTools.gameObjectType)
				{
					prefabTargets.Add(targetAsset);
				}
				else
				{
					restTargets.Add(targetAsset);
				}
			}

			IssuesDetector.Init(currentIssuesList);
			ProcessAssetTargets(sceneTargets, ProcessScene, Phase.Scenes, "Opening scene");
			if (IssuesFinder.operationCanceled) return;
			ProcessAssetTargets(prefabTargets, ProcessPrefab, Phase.Prefabs, "Prefab");
			if (IssuesFinder.operationCanceled) return;
			ProcessAssetTargets(restTargets, ProcessAsset, Phase.Rest, "Asset");
		}

		private static void ProcessAssetTargets(List<AssetInfo> targetAssets, ProcessAssetCallback callback,
			Phase phase, string progressAssetLabel)
		{
			var targetsCount = targetAssets.Count;
			
#if !UNITY_2020_1_OR_NEWER
			var updateStep = Math.Max(targetsCount / ProjectSettings.UpdateProgressStep, 1);
#endif
			for (var i = 0; i < targetsCount; i++)
			{
				var targetAsset = targetAssets[i];
				var path = targetAsset.Path;
				var assetName = Path.GetFileNameWithoutExtension(path);

				if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;

				if (
#if !UNITY_2020_1_OR_NEWER
					i % updateStep == 0 || 
#endif
					phase == Phase.Scenes)
				{
					if (IssuesFinder.ShowProgressBar((int)phase, TotalPhases, i, targetsCount, progressAssetLabel + ": " + assetName))
					{
						IssuesFinder.operationCanceled = true;
						break;
					}
				}

				callback.Invoke(targetAsset, assetName, i, targetsCount);
				if (IssuesFinder.operationCanceled) break;
			}
		}

		private static void ProcessScene(AssetInfo asset, string assetName, int sceneIndex, int totalScenes)
		{
			currentObjectIndex = 0;
			itemIndex = sceneIndex;
			totalItems = totalScenes;

			currentAssetName = assetName;

			var openSceneResult = CSSceneTools.OpenScene(asset.Path);
			if (!openSceneResult.success)
			{
				Debug.LogWarning(Maintainer.ConstructWarning("Can't open scene " + asset.Path));
				return;
			}

			var skipCleanPrefabInstances = ProjectSettings.Issues.scanGameObjects && ProjectSettings.Issues.lookInAssets;

			IssuesDetector.SceneStart(asset);
			CSTraverseTools.TraverseSceneGameObjects(openSceneResult.scene, skipCleanPrefabInstances, false, OnGameObjectTraverse);
			IssuesDetector.SceneEnd(asset);

			CSSceneTools.CloseOpenedSceneIfNeeded(openSceneResult);
		}

		private static void ProcessPrefab(AssetInfo asset, string assetName, int prefabIndex, int totalPrefabs)
		{
			currentObjectIndex = 0;

			itemIndex = prefabIndex;
			totalItems = totalPrefabs;

			currentAssetName = assetName;

			var prefabRootGameObject = CSPrefabTools.GetPrefabAssetRoot(asset.Path);
			if (prefabRootGameObject == null) return;
			IssuesDetector.StartPrefabAsset(asset);
			CSTraverseTools.TraversePrefabGameObjects(prefabRootGameObject, true, false, OnPrefabGameObjectTraverse);
			IssuesDetector.EndPrefabAsset(asset);
		}

		private static void ProcessAsset(AssetInfo asset, string assetName, int assetIndex, int totalAssets)
		{
			currentObjectIndex = 0;

			itemIndex = assetIndex;
			totalItems = totalAssets;

			var assetType = asset.Type;

			if (asset.Kind == AssetKind.Settings)
			{
				IssuesDetector.ProcessSettingsAsset(asset);
			}
			else
			{
				if (assetType == null)
				{
					if (CSAssetTools.IsAssetScriptableObjectWithMissingScript(asset.Path))
					{
						IssuesDetector.ProcessScriptableObject(asset, null);
					}
				}
				else if (assetType.BaseType == CSReflectionTools.scriptableObjectType)
				{
					var scriptableObject = AssetDatabase.LoadMainAssetAtPath(asset.Path);
					IssuesDetector.ProcessScriptableObject(asset, scriptableObject);
				}
#if UNITY_2019_1_OR_NEWER
				else if (assetType == CSReflectionTools.shaderType)
				{
					var shader = AssetDatabase.LoadAssetAtPath<Shader>(asset.Path);
					IssuesDetector.ProcessShader(asset, shader);
				}
#endif
			}
		}

		private static bool OnGameObjectTraverse(ObjectTraverseInfo objectInfo)
		{
#if !UNITY_2020_1_OR_NEWER
			if (currentObjectIndex % ObjectTraverseUpdateStep == 0)
#endif
			{
				if (IssuesFinder.ShowProgressBar(1, 3, itemIndex, totalItems,
					string.Format("Processing scene: {0} root {1}/{2}", currentAssetName, objectInfo.rootIndex + 1, objectInfo.TotalRoots)))
				{
					return false;
				}
			}

			currentObjectIndex++;

			bool skipTree;
			if (IssuesDetector.StartGameObject(objectInfo.current, objectInfo.inPrefabInstance, out skipTree))
			{
				CSTraverseTools.TraverseGameObjectComponents(objectInfo, OnComponentTraverse);
				IssuesDetector.EndGameObject(objectInfo.current);
			}
			objectInfo.skipCurrentTree = skipTree;

			return true;
		}

		private static bool OnPrefabGameObjectTraverse(ObjectTraverseInfo objectInfo)
		{
#if !UNITY_2020_1_OR_NEWER
			if (currentObjectIndex % ObjectTraverseUpdateStep == 0)
#endif			
			{

				if (IssuesFinder.ShowProgressBar(2, 3, itemIndex, totalItems,
					string.Format("Processing prefab: {0}", currentAssetName)))
				{
					return false;
				}
			}

			currentObjectIndex++;

			bool skipTree;
			if (IssuesDetector.StartGameObject(objectInfo.current, objectInfo.inPrefabInstance, out skipTree))
			{
				CSTraverseTools.TraverseGameObjectComponents(objectInfo, OnComponentTraverse);
				IssuesDetector.EndGameObject(objectInfo.current);
			}
			objectInfo.skipCurrentTree = skipTree;

			return true;
		}

		private static void OnComponentTraverse(ObjectTraverseInfo objectInfo, Component component, int orderIndex)
		{
			IssuesDetector.ProcessComponent(component, orderIndex);
		}
	}
}