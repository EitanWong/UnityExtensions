#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	using UnityEditor;
	using UnityEngine;

	using Tools;

	[Serializable]
	public enum AssetKind
	{
		Regular = 0,
		Settings = 10,
		FromPackage = 20,
		Unsupported = 100
	}

	[Serializable]
	public enum AssetSettingsKind
	{
		NotSettings = 0,
		AudioManager = 100,
		ClusterInputManager = 200,
		DynamicsManager = 300,
		EditorBuildSettings = 400,
		EditorSettings = 500,
		GraphicsSettings = 600,
		InputManager = 700,
		NavMeshAreas = 800,
		NavMeshLayers = 900,
		NavMeshProjectSettings = 1000,
		NetworkManager = 1100,
		Physics2DSettings = 1200,
		ProjectSettings = 1300,
		PresetManager = 1400,
		QualitySettings = 1500,
		TagManager = 1600,
		TimeManager = 1700,
		UnityAdsSettings = 1800,
		UnityConnectSettings = 1900,
		VFXManager = 2000,
		Unknown = 100000
	}

	internal class RawAssetInfo
	{
		public string path;
		public string guid;
		public AssetKind kind;
	}

	[Serializable]
	internal class AssetInfo
	{
		public string GUID { get; private set; }
		public string Path { get; private set; }
		public AssetKind Kind { get; private set; }
		public AssetSettingsKind SettingsKind { get; private set; }
		public Type Type { get; private set; }
		public long Size { get; private set; }

		public string[] dependenciesGUIDs = new string[0];
		public AssetReferenceInfo[] assetReferencesInfo = new AssetReferenceInfo[0];
		public ReferencedAtAssetInfo[] referencedAtInfoList = new ReferencedAtAssetInfo[0];

		public bool needToRebuildReferences = true;

		private ulong lastHash;
		private FileInfo fileInfo;
		private FileInfo metaFileInfo;

		[NonSerialized]
		private int[] allAssetObjects;

		public static AssetInfo Create(RawAssetInfo rawAssetInfo, Type type, AssetSettingsKind settingsKind)
		{
			if (string.IsNullOrEmpty(rawAssetInfo.guid))
			{
				Debug.LogError("Can't create AssetInfo since guid is invalid!");
				return null;
			}

			var newAsset = new AssetInfo
			{
				GUID = rawAssetInfo.guid,
				Path = rawAssetInfo.path,
				Kind = rawAssetInfo.kind,
				Type = type,
				SettingsKind = settingsKind,
				fileInfo = new FileInfo(rawAssetInfo.path),
				metaFileInfo = new FileInfo(rawAssetInfo.path + ".meta")
			};

			newAsset.UpdateIfNeeded();

			return newAsset;
		}

		private AssetInfo() { }

		public bool Exists()
		{
			ActualizePath();
			fileInfo.Refresh();
			return fileInfo.Exists;
		}

		public void UpdateIfNeeded()
		{
			if (string.IsNullOrEmpty(Path))
			{
				Debug.LogWarning(Maintainer.LogPrefix + "Can't update Asset since path is not set!");
				return;
			}

			/*if (Path.Contains("qwerty.unity"))
			{
				Debug.Log(Path);
			}*/

			fileInfo.Refresh();

			if (!fileInfo.Exists)
			{
				Debug.LogWarning(Maintainer.LogPrefix + "Can't update asset since file at path is not found:\n" + fileInfo.FullName + "\nAsset Path: " + Path);
				return;
			}

			ulong currentHash = 0;

			if (metaFileInfo == null)
			{
				metaFileInfo = new FileInfo(fileInfo.FullName + ".meta");
			}

			metaFileInfo.Refresh();
			if (metaFileInfo.Exists)
			{
				currentHash += (ulong)metaFileInfo.LastWriteTimeUtc.Ticks;
				currentHash += (ulong)metaFileInfo.Length;
			}

			currentHash += (ulong)fileInfo.LastWriteTimeUtc.Ticks;
			currentHash += (ulong)fileInfo.Length;

			if (lastHash == currentHash)
			{
				for (var i = dependenciesGUIDs.Length - 1; i > -1; i--)
				{
					var guid = dependenciesGUIDs[i];
					var path = AssetDatabase.GUIDToAssetPath(guid);
					path = CSPathTools.EnforceSlashes(path);
					if (!string.IsNullOrEmpty(path) && File.Exists(path)) continue;

					ArrayUtility.RemoveAt(ref dependenciesGUIDs, i);
					foreach (var referenceInfo in assetReferencesInfo)
					{
						if (referenceInfo.assetInfo.GUID != guid) continue;

						ArrayUtility.Remove(ref assetReferencesInfo, referenceInfo);
						break;
					}
				}

				if (!needToRebuildReferences) return;
			}

			foreach (var referenceInfo in assetReferencesInfo)
			{
				foreach (var info in referenceInfo.assetInfo.referencedAtInfoList)
				{
					if (info.assetInfo != this) continue;

					ArrayUtility.Remove(ref referenceInfo.assetInfo.referencedAtInfoList, info);
					break;
				}
			}

			lastHash = currentHash;

			needToRebuildReferences = true;
			Size = fileInfo.Length;

			assetReferencesInfo = new AssetReferenceInfo[0];
			dependenciesGUIDs = AssetDependenciesSearcher.FindDependencies(SettingsKind, Type, Kind, Path);
		}

		public List<AssetInfo> GetReferencesRecursive()
		{
			var result = new List<AssetInfo>();

			WalkReferencesRecursive(result, assetReferencesInfo);

			return result;
		}

		public List<AssetInfo> GetReferencedAtRecursive()
		{
			var result = new List<AssetInfo>();

			WalkReferencedAtRecursive(result, referencedAtInfoList);

			return result;
		}

		public void Clean()
		{
			foreach (var referenceInfo in assetReferencesInfo)
			{
				foreach (var info in referenceInfo.assetInfo.referencedAtInfoList)
				{
					if (info.assetInfo != this) continue;
					ArrayUtility.Remove(ref referenceInfo.assetInfo.referencedAtInfoList, info);
					break;
				}
			}

			foreach (var referencedAtInfo in referencedAtInfoList)
			{
				foreach (var info in referencedAtInfo.assetInfo.assetReferencesInfo)
				{
					if (info.assetInfo != this) continue;
					ArrayUtility.Remove(ref referencedAtInfo.assetInfo.assetReferencesInfo, info);
					referencedAtInfo.assetInfo.needToRebuildReferences = true;
					break;
				}
			}
		}

		public int[] GetAllAssetObjects()
		{
			if (allAssetObjects != null) return allAssetObjects;

			var assetType = Type;
			var assetTypeName = assetType != null ? assetType.Name : null;

			if ((assetType == CSReflectionTools.fontType ||
				assetType == CSReflectionTools.texture2DType ||
				assetType == CSReflectionTools.gameObjectType ||
				assetType == CSReflectionTools.defaultAssetType && Path.EndsWith(".dll") ||
				assetTypeName == "AudioMixerController" ||
				Path.EndsWith("LightingData.asset")) &&
				assetType != CSReflectionTools.lightingDataAsset
#if UNITY_2020_1_OR_NEWER
			    && assetType != CSReflectionTools.lightingSettings
#endif
				)
			{
				var loadedObjects = AssetDatabase.LoadAllAssetsAtPath(Path);
				var referencedObjectsCandidatesList = new List<int>(loadedObjects.Length);
				for (var i = 0; i < loadedObjects.Length; i++)
				{
					var loadedObject = loadedObjects[i];
					if (loadedObject == null) continue;
					var instance = loadedObject.GetInstanceID();
					if (assetType == CSReflectionTools.gameObjectType)
					{
						if (!AssetDatabase.IsSubAsset(instance) && !AssetDatabase.IsMainAsset(instance)) continue;
					}

					referencedObjectsCandidatesList.Add(instance);
				}

				allAssetObjects = referencedObjectsCandidatesList.ToArray();
			}
			else
			{
				var mainAsset = AssetDatabase.LoadMainAssetAtPath(Path);
				if (mainAsset != null)
				{
					allAssetObjects = new[] { AssetDatabase.LoadMainAssetAtPath(Path).GetInstanceID() };
				}
				else
				{
					allAssetObjects = new int[0];
				}
			}

			return allAssetObjects;
		}

		private void WalkReferencesRecursive(List<AssetInfo> result, AssetReferenceInfo[] assetReferenceInfos)
		{
			foreach (var referenceInfo in assetReferenceInfos)
			{
				if (result.IndexOf(referenceInfo.assetInfo) == -1)
				{
					result.Add(referenceInfo.assetInfo);
					WalkReferencesRecursive(result, referenceInfo.assetInfo.assetReferencesInfo);
				}
			}
		}

		private void WalkReferencedAtRecursive(List<AssetInfo> result, ReferencedAtAssetInfo[] referencedAtInfos)
		{
			foreach (var referencedAtInfo in referencedAtInfos)
			{
				if (result.IndexOf(referencedAtInfo.assetInfo) == -1)
				{
					result.Add(referencedAtInfo.assetInfo);
					WalkReferencedAtRecursive(result, referencedAtInfo.assetInfo.referencedAtInfoList);
				}
			}
		}

		private void ActualizePath()
		{
			if (Kind == AssetKind.FromPackage) return;

			var actualPath = CSPathTools.EnforceSlashes(AssetDatabase.GUIDToAssetPath(GUID));
			if (!string.IsNullOrEmpty(actualPath) && actualPath != Path)
			{
				fileInfo = new FileInfo(actualPath);
				metaFileInfo = new FileInfo(actualPath + ".meta");
				Path = actualPath;
			}
		}
	}
}