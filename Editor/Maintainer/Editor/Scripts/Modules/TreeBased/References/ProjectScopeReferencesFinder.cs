#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References
{
	using Core;
	using Entry;
	using Routines;
	using Settings;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using Tools;
	using UI;
	using UnityEditor;
	using UnityEngine;
	using Debug = UnityEngine.Debug;
	using Object = UnityEngine.Object;

	internal static class ProjectScopeReferencesFinder
	{
		private static readonly List<AssetConjunctions> ConjunctionInfoList = new List<AssetConjunctions>();

		public static AssetConjunctions currentAssetConjunctions;

		public static ProjectReferenceItem[] FindAssetReferencesFromResults(string asset)
		{
			return FindAssetsReferences(new []{FilterItem.Create(asset, FilterKind.Path)}, true, true);
		}

		public static ProjectReferenceItem[] FindAssetsReferences(FilterItem[] assets, bool ignoreClearOption, bool showResults)
		{
			if (UserSettings.References.selectedFindClearsProjectResults && !ignoreClearOption)
			{
				SearchResultsStorage.ProjectReferencesLastSearched = new FilterItem[0];
				SearchResultsStorage.ProjectReferencesSearchResults = new ProjectReferenceItem[0];
			}

			var lastSearched = SearchResultsStorage.ProjectReferencesLastSearched;
			var newItem = false;

			foreach (var asset in assets)
			{
				newItem |= CSFilterTools.TryAddNewItemToFilters(ref lastSearched, asset);
			}

			if (assets.Length == 1)
			{
				ProjectReferencesTab.AutoSelectPath = assets[0].value;
			}

			if (newItem)
			{
				return FindAssetsReferences(lastSearched, assets, showResults);
			}

			//ReferencesTab.AutoShowExistsNotification = true;
			MaintainerWindow.ShowAssetReferences();

			return SearchResultsStorage.ProjectReferencesSearchResults;
		}

		public static ProjectReferenceItem[] FindAssetsReferences(FilterItem[] allTargetAssets, FilterItem[] newTargetAssets, bool showResults = true)
		{
			var results = new List<ProjectReferenceItem>();

			ConjunctionInfoList.Clear();

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

			try
			{
				var sw = Stopwatch.StartNew();

				CSEditorTools.lastRevealSceneOpenResult = null;
				EntryGenerator.ResetCachedObjects();

				var searchCanceled = LookForAssetsReferences(allTargetAssets, results);
				sw.Stop();

				EditorUtility.ClearProgressBar();

				if (!searchCanceled)
				{
					var resultsCount = results.Count;
					if (resultsCount <= 1)
					{
						ProjectReferencesTab.AutoSelectPath = null;
						MaintainerWindow.ShowNotification("Nothing found!");
					}
					else
					{
						if (newTargetAssets != null && newTargetAssets.Length > 0)
						{
							var found = false;
							foreach (var result in results)
							{
								if (result.depth == 0 && CSFilterTools.IsValueMatchesAnyFilter(result.assetPath, newTargetAssets))
								{
									found = true;
									break;
								}
							}

							if (!found)
							{
								ProjectReferencesTab.AutoSelectPath = null;
								MaintainerWindow.ShowNotification("Nothing found!");
							}
							else
							{
								MaintainerWindow.ClearNotification();
							}
						}
						else
						{
							MaintainerWindow.ClearNotification();
						}
					}

					Debug.Log(Maintainer.LogPrefix + ReferencesFinder.ModuleName + " results: " + (resultsCount - 1) +
					          " items found in " + sw.Elapsed.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture) +
					          " seconds.");
				}
				else
				{
					Debug.Log(Maintainer.LogPrefix + ReferencesFinder.ModuleName + "Search canceled by user!");
				}
			}
			catch (Exception e)
			{
				Debug.LogError(Maintainer.LogPrefix + ReferencesFinder.ModuleName + ": " + e);
				EditorUtility.ClearProgressBar();
			}

			SaveLastSearched(results);

			EntryGenerator.ResetCachedObjects();
			SearchResultsStorage.ProjectReferencesSearchResults = results.ToArray();

			if (showResults)
			{
				MaintainerWindow.ShowAssetReferences();
			}

			return results.ToArray();
		}

		public static string[] GetSelectedAssets()
		{
			var selectedIDs = Selection.instanceIDs;
			return GetAssetsFromInstances(selectedIDs);
		}

		public static string[] GetAssetsFromInstances(int[] instanceIDs)
		{
			var paths = new List<string>(instanceIDs.Length);

			foreach (var id in instanceIDs)
			{
				if (AssetDatabase.IsSubAsset(id)) continue;
				var path = AssetDatabase.GetAssetPath(id);
				path = CSPathTools.EnforceSlashes(path);
				if (!File.Exists(path) && !Directory.Exists(path)) continue;
				paths.Add(path);
			}

			return paths.ToArray();
		}

		private static bool LookForAssetsReferences(FilterItem[] selectedAssets, List<ProjectReferenceItem> results)
		{
			var canceled = !CSSceneTools.SaveCurrentModifiedScenes(false);

			if (!canceled)
			{
				var map = AssetsMap.GetUpdated();
				if (map == null) return true;

				var count = map.assets.Count;
				
#if !UNITY_2020_1_OR_NEWER
				var updateStep = Math.Max(count / ProjectSettings.UpdateProgressStep, 1);
#endif
				var root = new ProjectReferenceItem
				{
					id = results.Count,
					name = "root",
					depth = -1
				};
				results.Add(root);

				for (var i = 0; i < count; i++)
				{
					if (
#if !UNITY_2020_1_OR_NEWER
						i % updateStep == 0 && 
#endif
					    EditorUtility.DisplayCancelableProgressBar(
						    string.Format(ReferencesFinder.ProgressCaption, 1, ReferencesFinder.PhasesCount),
						    string.Format(ReferencesFinder.ProgressText, "Building references tree", i + 1, count),
						    (float) i / count))
					{
						canceled = true;
						break;
					}

					var assetInfo = map.assets[i];

					// excludes settings assets from the list depth 0 items
					if (assetInfo.Kind == AssetKind.Settings) continue;

					// excludes all assets except selected ones from the list depth 0 items, if any was selected
					if (selectedAssets != null)
					{
						if (!CSFilterTools.IsValueMatchesAnyFilter(assetInfo.Path, selectedAssets)) continue;
					}

					if (ProjectSettings.References.pathIncludesFilters != null &&
					    ProjectSettings.References.pathIncludesFilters.Length > 0)
					{
						// excludes all root assets except included ones
						if (!CSFilterTools.IsValueMatchesAnyFilter(assetInfo.Path, ProjectSettings.References.pathIncludesFilters)) continue;
					}

					// excludes ignored root asset
					if (CSFilterTools.IsValueMatchesAnyFilter(assetInfo.Path, ProjectSettings.References.pathIgnoresFilters)) continue;

					var branchElements = new List<ProjectReferenceItem>();
					ProjectScopeReferencesTreeBuilder.BuildTreeBranch(assetInfo, 0, results.Count, ConjunctionInfoList, branchElements);
					results.AddRange(branchElements);
				}
			}

			if (!canceled)
			{
				canceled = ProjectEntryFinder.FillProjectScopeReferenceEntries(ConjunctionInfoList, TryAddEntryToMatchedConjunctions);
			}

			// TODO: remove this work-around when this bug will be fixed:
			// https://issuetracker.unity3d.com/issues/assets-used-in-components-of-a-nested-prefab-are-counted-as-direct-dependencies-of-all-higher-level-nested-prefabs
			for (var i = results.Count - 1; i >= 0; i--)
			{
				var result = results[i];
				var resultEntries = result.referencingEntries;
				if (resultEntries == null || resultEntries.Length == 0)
				{
					continue;
				}

				foreach (var referencingEntry in resultEntries)
				{
					if (referencingEntry.Location == Location.NotFound && result.assetTypeName == "GameObject")
					{
						results.Remove(result);
						break;
					}
				}
			}

			if (!canceled)
			{
				AssetsMap.Save();
			}

			if (canceled)
			{
				ProjectReferencesTab.AutoSelectPath = null;
			}

			return canceled;
		}

		private static void TryAddEntryToMatchedConjunctions(Object lookAt, int lookForInstanceId, EntryAddSettings settings)
		{
			var lookAtGameObject = lookAt as GameObject;

			for (var i = 0; i < currentAssetConjunctions.conjunctions.Count; i++)
			{
				var conjunction = currentAssetConjunctions.conjunctions[i];
				var referencedAssetObjects = conjunction.referencedAsset.GetAllAssetObjects();

				var match = false;
				for (var j = 0; j < referencedAssetObjects.Length; j++)
				{
					if (referencedAssetObjects[j] != lookForInstanceId) continue;

					match = true;
					break;
				}

				if (!match) continue;

				var newEntry = EntryGenerator.CreateNewReferenceEntry(EntryFinder.currentLocation, lookAt, lookAtGameObject, settings);

				conjunction.referencedAtInfo.AddNewEntry(newEntry);
			}
		}

		private static void SaveLastSearched(List<ProjectReferenceItem> results)
		{
			var resultsCount = results.Count;
			var showProgress = resultsCount > 500000;

			if (showProgress) EditorUtility.DisplayProgressBar(ReferencesFinder.ModuleName, "Parsing results...", 0);

			var rootItems = new List<FilterItem>(resultsCount);
#if !UNITY_2020_1_OR_NEWER
			var updateStep = Math.Max(resultsCount / ProjectSettings.UpdateProgressStep, 1);
#endif
			for (var i = 0; i < resultsCount; i++)
			{
				if (showProgress
#if !UNITY_2020_1_OR_NEWER
				    && i % updateStep == 0
#endif
				    ) EditorUtility.DisplayProgressBar(ReferencesFinder.ModuleName, "Parsing results...", (float)i / resultsCount);

				var result = results[i];
				if (result.depth != 0) continue;
				rootItems.Add(FilterItem.Create(result.assetPath, FilterKind.Path));
			}

			SearchResultsStorage.ProjectReferencesLastSearched = rootItems.ToArray();
		}
	}
}