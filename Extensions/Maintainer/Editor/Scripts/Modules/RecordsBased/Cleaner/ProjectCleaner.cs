#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Cleaner
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using UnityEngine;
	using Core;
	using Settings;
	using Tools;
	using UI;

	/// <summary>
	/// Allows to find and clean garbage in your Unity project. See readme for details.
	/// </summary>
	public static class ProjectCleaner
	{
		internal const string ModuleName = "Project Cleaner";

		private const string ProgressCaption = ModuleName + ": phase {0} of {1}, item {2} of {3}";

		private static int phasesCount;
		private static int currentPhase;

		private static int folderIndex;
		private static int foldersCount;

		private static int itemsToClean;

		private static long cleanedBytes;

		/// <summary>
		/// Starts garbage search and generates report.
		/// </summary>
		/// <returns>Project Cleaner report, similar to the exported report from the %Maintainer window.</returns>
		/// %Maintainer window is not shown.
		/// <br/>Useful when you wish to integrate %Maintainer in your build pipeline.
		public static string SearchAndReport()
		{
			var foundGarbage = StartSearch(false);

			// ReSharper disable once CoVariantArrayConversion
			return ReportsBuilder.GenerateReport(ModuleName, foundGarbage);
		}

		/// <summary>
		/// Starts garbage search, cleans what was found with optional confirmation and
		/// generates report to let you know what were cleaned up.
		/// </summary>
		/// <param name="showConfirmation">Enables or disables confirmation dialog about cleaning up found stuff.</param>
		/// <returns>Project Cleaner report about removed items.</returns>
		/// %Maintainer window is not shown.
		/// <br/>Useful when you wish to integrate %Maintainer in your build pipeline.
		public static string SearchAndCleanAndReport(bool showConfirmation = true)
		{
			var foundGarbage = StartSearch(false);
			var cleanedGarbage = StartClean(foundGarbage, false, showConfirmation);

			var header = "Total cleaned bytes: " + CSEditorTools.FormatBytes(cleanedBytes);
			header += "\nFollowing items were cleaned up:";

			// ReSharper disable once CoVariantArrayConversion
			return ReportsBuilder.GenerateReport(ModuleName, cleanedGarbage, header);
		}

		/// <summary>
		/// Starts garbage search with current settings.
		/// </summary>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of CleanerRecords in case you wish to manually iterate over them and make custom report.</returns>
		public static CleanerRecord[] StartSearch(bool showResults)
		{
			var results = new List<CleanerRecord>();

			phasesCount = 0;
			currentPhase = 0;

			if (ProjectSettings.Cleaner.findEmptyFolders) phasesCount++;
			if (ProjectSettings.Cleaner.findUnreferencedAssets) phasesCount++;

			var searchCanceled = !CSSceneTools.SaveCurrentModifiedScenes(true);

			if (searchCanceled)
			{
				Debug.Log(Maintainer.LogPrefix + "Search canceled by user!");
				return null;
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

			try
			{
				var sw = System.Diagnostics.Stopwatch.StartNew();

				CSEditorTools.lastRevealSceneOpenResult = null;

				if (ProjectSettings.Cleaner.findEmptyFolders)
				{
					searchCanceled = ScanFolders(results);
				}

				if (!searchCanceled && ProjectSettings.Cleaner.findUnreferencedAssets)
				{
					searchCanceled = ScanProjectFiles(results);
				}

				sw.Stop();

				EditorUtility.ClearProgressBar();

				if (!searchCanceled)
				{
					Debug.Log(Maintainer.LogPrefix + ModuleName + " results: " + results.Count +
					          " items found in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
					          " seconds.");
				}
				else
				{
					Debug.Log(Maintainer.LogPrefix + "Search canceled by user!");
				}

			}
			catch (Exception e)
			{
				Debug.Log(Maintainer.LogPrefix + e);
				EditorUtility.ClearProgressBar();
			}

			SearchResultsStorage.CleanerSearchResults = results.ToArray();
			if (showResults) MaintainerWindow.ShowCleaner();

			return results.ToArray();
		}

		/// <summary>
		/// Starts clean of the garbage found with StartSearch() method.
		/// </summary>
		/// <param name="recordsToClean">Pass records you wish to clean here or leave null to let it load last search results.</param>
		/// <param name="showResults">Shows results in the %Maintainer window if true.</param>
		/// <param name="showConfirmation">Shows confirmation dialog before performing cleanup if true.</param>
		/// <returns>Array of CleanRecords which were cleaned up.</returns>
		public static CleanerRecord[] StartClean(CleanerRecord[] recordsToClean = null, bool showResults = true, bool showConfirmation = true)
		{
			var records = recordsToClean;
			if (records == null)
			{
				records = SearchResultsStorage.CleanerSearchResults;
			}

			if (records.Length == 0)
			{
				return null;
			}

			cleanedBytes = 0;
			itemsToClean = 0;

			foreach (var record in records)
			{
				if (record.selected) itemsToClean++;
			}

			if (itemsToClean == 0)
			{
				EditorUtility.DisplayDialog(ModuleName, "Please select items to clean up!", "Ok");
				return null;
			}

			if (!showConfirmation || itemsToClean == 1 || EditorUtility.DisplayDialog("Confirmation", "Do you really wish to delete " + itemsToClean + " items?\n" + Maintainer.DataLossWarning, "Go for it!", "Cancel"))
			{
				var sw = System.Diagnostics.Stopwatch.StartNew();

				var cleanCanceled = CleanRecords(records);

				var cleanedRecords = new List<CleanerRecord>(records.Length);
				var notCleanedRecords = new List<CleanerRecord>(records.Length);

				foreach (var record in records)
				{
					if (record.cleaned)
					{
						cleanedRecords.Add(record);
					}
					else
					{
						notCleanedRecords.Add(record);
					}
				}

				records = notCleanedRecords.ToArray();

				sw.Stop();

				EditorUtility.ClearProgressBar();

				if (!cleanCanceled)
				{
					Debug.Log(Maintainer.LogPrefix + ModuleName + " results: " + cleanedRecords.Count +
						" items (" + CSEditorTools.FormatBytes(cleanedBytes) + " in size) cleaned in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
						" seconds.");
				}
				else
				{
					Debug.Log(Maintainer.LogPrefix + "Deletion was canceled by user!");
				}

				SearchResultsStorage.CleanerSearchResults = records;
				if (showResults) MaintainerWindow.ShowCleaner();

				return cleanedRecords.ToArray();
			}

			return null;
		}

		[DidReloadScripts]
		private static void AutoCleanFolders()
		{
			if (!ProjectSettings.Cleaner.findEmptyFolders || !ProjectSettings.Cleaner.findEmptyFoldersAutomatically) return;

			var results = new List<CleanerRecord>();
			ScanFolders(results, false);

			if (results.Count > 0)
			{
				var result = EditorUtility.DisplayDialogComplex("Maintainer", ModuleName + " found " + results.Count + " empty folders. Do you wish to remove them?\n" + Maintainer.DataLossWarning, "Yes", "No", "Show in Maintainer");
				if (result == 0)
				{
					var records = results.ToArray();
					CleanRecords(records, false);
					Debug.Log(Maintainer.LogPrefix + results.Count + " empty folders cleaned.");
				}
				else if (result == 2)
				{
					SearchResultsStorage.CleanerSearchResults = results.ToArray();
					MaintainerWindow.ShowCleaner();
				}
			}
		}

		private static bool ScanFolders(ICollection<CleanerRecord> results, bool showProgress = true)
		{
			bool canceled;
			currentPhase++;

			folderIndex = 0;

			if (showProgress)
			{
				EditorUtility.DisplayProgressBar(
				    string.Format(ProgressCaption, currentPhase, phasesCount, folderIndex, foldersCount), "Getting all folders...", (float)folderIndex / foldersCount);
			}

			var emptyFolders = new List<string>();
			var root = CSPathTools.EnforceSlashes(Application.dataPath);

			foldersCount = Directory.GetDirectories(root, "*", SearchOption.AllDirectories).Length;
			FindEmptyFoldersRecursive(emptyFolders, root, showProgress, out canceled);

			ExcludeSubFoldersOfEmptyFolders(ref emptyFolders);

			foreach (var folder in emptyFolders)
			{
				var newRecord = AssetRecord.CreateEmptyFolderRecord(folder);
				if (newRecord != null) results.Add(newRecord);
			}

			return canceled;
		}

		private static bool ScanProjectFiles(ICollection<CleanerRecord> results, bool showProgress = true)
		{
			currentPhase++;

			var ignoredScenes = new List<string>();

			if (ProjectSettings.Cleaner.ignoreScenesInBuild)
			{
				ignoredScenes.AddRange(CSSceneTools.GetScenesInBuild(!ProjectSettings.Cleaner.ignoreOnlyEnabledScenesInBuild));
			}

			foreach (var scene in ProjectSettings.Cleaner.sceneIgnoresFilters)
			{
				if (ignoredScenes.IndexOf(scene.value) == -1)
				{
					ignoredScenes.Add(scene.value);
				}
			}

			CheckScenesForExistence(results, ignoredScenes);

			if (ignoredScenes.Count == 0)
			{
				if (!UserSettings.Cleaner.muteNoIgnoredScenesWarning)
				{
					var dialogResult = EditorUtility.DisplayDialogComplex(
						"No ignored scenes!",
						"No scenes were added to the build settings or to the Filters > Scenes Ignores.\n" +
						"All not ignored scenes are treated as unused if not referenced somewhere in other ignored assets.", 
						"Proceed anyway",
						"Proceed and never show again", "Cancel");
					
					if (dialogResult == 1)
					{
						UserSettings.Cleaner.muteNoIgnoredScenesWarning = true;
						UserSettings.Save();
					}
					else if (dialogResult == 2)
					{
						return false;
					}
				}
				
				results.Add(CleanerWarningRecord.Create("No scenes added to the build settings or Filters > Scenes Ignores tab.\n" +
				                                        "Search may include all assets used in your game scenes!"));
			}

			var map = AssetsMap.GetUpdated();
			if (map == null)
			{
				results.Add(CleanerErrorRecord.Create("Can't get assets map!"));
				return false;
			}
			
			EditorUtility.DisplayCancelableProgressBar(string.Format(ProgressCaption, currentPhase, phasesCount, 0, 0), "Analyzing Assets Map for references...", 0);

#if UNITY_2019_3_OR_NEWER
			BuildReportAnalyzer.Init();
#endif
			
			var allAssetsInProject = map.assets;
			var count = allAssetsInProject.Count;
			
#if !UNITY_2020_1_OR_NEWER
			var updateStep = Math.Max(count / ProjectSettings.UpdateProgressStep, 1);
#endif
			var referencedAssets = new HashSet<AssetInfo>();

			for (var i = 0; i < count; i++)
			{
				if (showProgress
#if !UNITY_2020_1_OR_NEWER
				    && i % updateStep == 0
#endif
				    && i != 0 && EditorUtility.DisplayCancelableProgressBar(
					    string.Format(ProgressCaption, currentPhase, phasesCount, i + 1, count), "Analyzing Assets Map for references...",
					    (float)(i + 1) / count))
				{
					return true;
				}

				var asset = allAssetsInProject[i];

				/*if (asset.Path.EndsWith("1.pdf"))
				{
					Debug.Log(asset.Type);
					if (asset.Type == CSReflectionTools.defaultAssetType)
					{
						var importer = AssetImporter.GetAtPath(asset.Path);
						Debug.Log(importer);
					}
				}*/

				if (asset.Kind != AssetKind.Regular) continue;

				if (AssetInIgnores(asset, ignoredScenes))
				{
					referencedAssets.Add(asset);
					var references = asset.GetReferencesRecursive();
					foreach (var reference in references)
					{
						referencedAssets.Add(reference);
					}
				}

#if UNITY_2019_3_OR_NEWER
				if (BuildReportAnalyzer.IsFileInBuildReport(asset.GUID))
				{
					referencedAssets.Add(asset);
					var references = asset.GetReferencesRecursive();
					foreach (var reference in references)
					{
						if (!referencedAssets.Contains(reference))
						{
							referencedAssets.Add(reference);
						}
					}
				}
#endif

				if (AssetInIgnoresSecondPass(asset, referencedAssets))
				{
					referencedAssets.Add(asset);
					var references = asset.GetReferencesRecursive();
					foreach (var reference in references)
					{
						if (!referencedAssets.Contains(reference))
						{
							referencedAssets.Add(reference);
						}
					}
				}
			}

			var unreferencedAssets = new List<AssetInfo>(count);
			for (var i = 0; i < count; i++)
			{
				if (showProgress
#if !UNITY_2020_1_OR_NEWER
				    && i % updateStep == 0 
#endif
				    && i != 0 && EditorUtility.DisplayCancelableProgressBar(
					    string.Format(ProgressCaption, currentPhase, phasesCount, i + 1, count), "Filtering out unreferenced assets...",
					    (float)(i + 1) / count))
				{
					return true;
				}

				var asset = allAssetsInProject[i];
				if (asset.Kind != AssetKind.Regular) continue;

				if (!referencedAssets.Contains(asset))
				{
					if (unreferencedAssets.IndexOf(asset) == -1)
					{
						unreferencedAssets.Add(asset);
					}
				}
			}

			count = unreferencedAssets.Count;
			
#if !UNITY_2020_1_OR_NEWER
			updateStep = Math.Max(count / ProjectSettings.UpdateProgressStep, 1);
#endif

			for (var i = count - 1; i > -1; i--)
			{
				if (showProgress
#if !UNITY_2020_1_OR_NEWER
				    && i % updateStep == 0 
#endif
				    && i != 0)
				{
					var index = count - i;
					if (EditorUtility.DisplayCancelableProgressBar(
						string.Format(ProgressCaption, currentPhase, phasesCount, index, count), "Populating results...",
						(float)index / count))
					{
						return true;
					}
				}

				var unreferencedAsset = unreferencedAssets[i];
				results.Add(AssetRecord.Create(RecordType.UnreferencedAsset, unreferencedAsset));
			}

			return false;
		}

		private static bool AssetInIgnores(AssetInfo assetInfo, List<string> ignoredScenes)
		{
			if (assetInfo.Type == CSReflectionTools.monoScriptType/* && !MaintainerSettings.Cleaner.findUnreferencedScripts*/)
			{
				return true;
			}

			if (assetInfo.Type == CSReflectionTools.textAssetType)
			{
				return true;
			}

			if (assetInfo.Type == CSReflectionTools.spriteAtlasType)
			{
				var atlas = AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>(assetInfo.Path);
				if (atlas != null)
				{
					var so = new SerializedObject(atlas);

					// source: SpriteAtlasInspector
					var bindAsDefaultProperty = so.FindProperty("m_EditorData.bindAsDefault");
					if (bindAsDefaultProperty != null)
					{
						if (bindAsDefaultProperty.boolValue)
						{
							return true;
						}
					}
					else
					{
						Debug.LogError(Maintainer.LogPrefix + "Can't parse UnityEngine.U2D.SpriteAtlas, please report to " + Maintainer.SupportEmail);
					}
				}
				else
				{
					Debug.LogWarning(Maintainer.LogPrefix + "Couldn't load SpriteAtlas: " + assetInfo.Path);
				}
			}

			if (assetInfo.Type == CSReflectionTools.assemblyDefinitionAssetType)
			{
				return true;
			}

			if (assetInfo.Type == CSReflectionTools.defaultAssetType)
			{
				var importer = AssetImporter.GetAtPath(assetInfo.Path);
				if (importer is PluginImporter) return true;
				if (importer.ToString() == " (UnityEngine.DefaultImporter)") return true;
			}

			if (CSFilterTools.IsValueMatchesAnyFilter(assetInfo.Path, ProjectSettings.Cleaner.MandatoryFilters))
			{
				return true;
			}

			if (CSFilterTools.IsValueMatchesAnyFilter(assetInfo.Path, ProjectSettings.Cleaner.pathIgnoresFilters))
			{
				return true;
			}

			if (assetInfo.Type == CSReflectionTools.sceneAssetType && ignoredScenes.IndexOf(assetInfo.Path) != -1) return true;

			foreach (var referencedAtInfo in assetInfo.referencedAtInfoList)
			{
				if (referencedAtInfo.assetInfo.SettingsKind != AssetSettingsKind.NotSettings && referencedAtInfo.assetInfo.SettingsKind != AssetSettingsKind.EditorBuildSettings)
				{
					return true;
				}

				if (referencedAtInfo.assetInfo.Kind == AssetKind.FromPackage)
				{
					return true;
				}
			}

			var assetBundleName = AssetDatabase.GetImplicitAssetBundleName(assetInfo.Path);
			if (!string.IsNullOrEmpty(assetBundleName))
			{
				return true;
			}

			if (assetInfo.Type != null)
			{
				var nameSpace = assetInfo.Type.Namespace;
				if (!string.IsNullOrEmpty(nameSpace))
				{
					if (nameSpace.StartsWith("UnityEditor.AddressableAssets"))
					{
						return true;
					}
				}
			}
			
			return false;
		}

		private static bool AssetInIgnoresSecondPass(AssetInfo asset, HashSet<AssetInfo> referencedAssets)
		{
			if (asset.Type != CSReflectionTools.spriteAtlasType || asset.assetReferencesInfo.Length <= 0) return false;

			var spriteAtlasHasReferencedItems = false;
			foreach (var reference in asset.assetReferencesInfo)
			{
				if (!referencedAssets.Contains(reference.assetInfo)) continue;

				spriteAtlasHasReferencedItems = true;
				break;
			}

			if (spriteAtlasHasReferencedItems)
			{
				return true;
			}

			return false;
		}

		private static void CheckScenesForExistence(ICollection<CleanerRecord> results, List<string> ignoredScenes)
		{
			for (var i = ignoredScenes.Count - 1; i >= 0; i--)
			{
				var scenePath = ignoredScenes[i];
				if (!File.Exists(scenePath))
				{
					results.Add(CleanerErrorRecord.Create("Scene " + Path.GetFileName(scenePath) + " from Ignores or Build Settings not found!"));
					ignoredScenes.RemoveAt(i);
				}
			}
		}

		private static void ExcludeSubFoldersOfEmptyFolders(ref List<string> emptyFolders)
		{
			var emptyFoldersFiltered = new List<string>(emptyFolders.Count);
			for (var i = emptyFolders.Count-1; i >= 0; i--)
			{
				var folder = emptyFolders[i];
				if (!CSArrayTools.IsItemContainsAnyStringFromArray(folder, emptyFoldersFiltered))
				{
					emptyFoldersFiltered.Add(folder);
				}
			}
			emptyFolders = emptyFoldersFiltered;
		}

		private static bool FindEmptyFoldersRecursive(List<string> foundEmptyFolders, string root, bool showProgress, out bool canceledByUser)
		{
			var rootSubFolders = Directory.GetDirectories(root);

			var canceled = false;
			var emptySubFolders = true;

			var count = rootSubFolders.Length;
			
#if !UNITY_2020_1_OR_NEWER
			var updateStep = Math.Max(count / ProjectSettings.UpdateProgressStep, 1);
#endif

			for (var i = 0; i < count; i++)
			{
				var folder = CSPathTools.EnforceSlashes(rootSubFolders[i]);
				folderIndex++;

				if (showProgress
#if !UNITY_2020_1_OR_NEWER
				    && i % updateStep == 0
#endif
				    && EditorUtility.DisplayCancelableProgressBar(
					    string.Format(ProgressCaption, currentPhase, phasesCount, folderIndex, foldersCount), "Scanning folders...",
					    (float) folderIndex / foldersCount))
				{
					canceled = true;
					break;
				}

				if (CSFilterTools.IsValueMatchesAnyFilter(folder.Replace('\\', '/'), ProjectSettings.Cleaner.pathIgnoresFilters))
				{
					emptySubFolders = false;
					continue;
				}

				emptySubFolders &= FindEmptyFoldersRecursive(foundEmptyFolders, folder, showProgress, out canceled);
				if (canceled) break;
			}

			if (canceled)
			{
				canceledByUser = true;
				return false;
			}

			var rootFolderHasFiles = true;
			var filesInRootFolder = Directory.GetFiles(root);

			foreach (var file in filesInRootFolder)
			{
				if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;

				rootFolderHasFiles = false;
				break;
			}

			var rootFolderEmpty = emptySubFolders && rootFolderHasFiles;
			if (rootFolderEmpty)
			{
				foundEmptyFolders.Add(root);
			}

			canceledByUser = false;
			return rootFolderEmpty;
		}

		private static bool CleanRecords(IEnumerable<CleanerRecord> results, bool showProgress = true)
		{
			var canceled = false;
			var i = 0;

			AssetDatabase.StartAssetEditing();

			foreach (var item in results)
			{
				if (showProgress && EditorUtility.DisplayCancelableProgressBar(string.Format(ProgressCaption, 1, 1, i + 1, itemsToClean), "Cleaning selected items...", (float)i/itemsToClean))
				{
					canceled = true;
					break;
				}

				if (item.selected)
				{
					i++;
					if (item.Clean() && item is AssetRecord)
					{
						cleanedBytes += (item as AssetRecord).size;
					}
				}
			}

			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh();

			return canceled;
		}
	}
}