#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References
{
	using Core;
	using Entry;
	using Settings;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using Tools;
	using UI;
	using UnityEditor;
	using UnityEngine;
	using Debug = UnityEngine.Debug;
	using Object = UnityEngine.Object;

	internal static class HierarchyScopeReferencesFinder
	{
		private class ExactReferenceData
		{
			public string assetPath;
			public ReferencingEntryData reference;
		}
		
		public static HierarchyReferenceItem[] FindComponentReferencesInHierarchy(Component component, bool showResults = true)
		{
			return FindObjectsReferencesInHierarchy(new Object[] {component}, showResults);
		}

		public static HierarchyReferenceItem[] FindObjectsReferencesInHierarchy(Object[] objects, bool checkGameObjectsComponents, bool showResults = true)
		{
			if (UserSettings.References.clearHierarchyResults)
			{
				SearchResultsStorage.HierarchyReferencesLastSearched = new int[0];
				SearchResultsStorage.HierarchyReferencesSearchResults = new HierarchyReferenceItem[0];
			}

			var lastSearched = SearchResultsStorage.HierarchyReferencesLastSearched;
			var allObjects = CSObjectTools.GetObjectsFromInstanceIds(lastSearched);

			var items = new List<Object>(objects);
			if (checkGameObjectsComponents)
			{
				for (var i = items.Count - 1; i >= 0; i--)
				{
					var item = items[i];
					var gameObject = item as GameObject;
					if (gameObject == null) continue;

					var components = gameObject.GetComponents<Component>();
					foreach (var component in components)
					{
						if (component == null) continue;
						if (CSObjectTools.IsHiddenInInspector(component)) continue;
						items.Insert(i, component);
					}
				}
			}

			var newItem = false;

			foreach (var o in items)
			{
				if (!ArrayUtility.Contains(allObjects, o))
				{
					newItem = true;
					ArrayUtility.Add(ref allObjects, o);
				}
			}

			if (items.Count == 1)
			{
				HierarchyReferencesTab.AutoSelectHierarchyReference = ObjectToReferencingEntry(items[0]).reference;
			}

			HierarchyReferenceItem[] result;
			
			if (newItem)
			{
				result = FindHierarchyObjectsReferences(allObjects, items.ToArray(), showResults);
			}
			else
			{
				MaintainerWindow.ShowObjectReferences();
				result = SearchResultsStorage.HierarchyReferencesSearchResults;
			}

			return result;
		}

		public static HierarchyReferenceItem[] FindHierarchyObjectsReferences(Object[] allObjects, Object[] newObjects, bool showResults = true)
		{
			var results = new List<HierarchyReferenceItem>();

			try
			{
				var sw = Stopwatch.StartNew();

				CSEditorTools.lastRevealSceneOpenResult = null;
				EntryGenerator.ResetCachedObjects();

				var searchCanceled = LookForObjectsReferencesInHierarchy(allObjects, results);
				sw.Stop();

				EditorUtility.ClearProgressBar();

				if (!searchCanceled)
				{
					var referencesFound = 0;
					foreach (var result in results)
					{
						if (result.depth == 1)
						{
							referencesFound++;
						}
					}

					if (referencesFound == 0)
					{
						HierarchyReferencesTab.AutoSelectHierarchyReference = null;
						MaintainerWindow.ShowNotification("Nothing found!");
					}
					else
					{
						if (newObjects != null && newObjects.Length > 0)
						{
							var totalFoundFromNew = GetFoundObjects(newObjects, results);
							if (totalFoundFromNew.Count == 0)
							{
								HierarchyReferencesTab.AutoSelectHierarchyReference = null;
								MaintainerWindow.ShowNotification("Nothing found!");
							}
							else
							{
								if (HierarchyReferencesTab.AutoSelectHierarchyReference == null)
								{
									if (totalFoundFromNew.Count == 1)
									{
										var firstFound = totalFoundFromNew[0];
										var entry = ObjectToReferencingEntry(firstFound);
										HierarchyReferencesTab.AutoSelectHierarchyReference = entry.reference;
									}
								}
								
								MaintainerWindow.ClearNotification();
							}
						}
						else
						{
							MaintainerWindow.ClearNotification();
						}
					}

					Debug.Log(Maintainer.LogPrefix + ReferencesFinder.ModuleName + " results: " + referencesFound +
					          " references found in " + sw.Elapsed.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture) +
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

			var foundObjects = GetFoundObjects(allObjects, results);
			SaveLastSearched(foundObjects);

			EntryGenerator.ResetCachedObjects();
			SearchResultsStorage.HierarchyReferencesSearchResults = results.ToArray();

			if (showResults)
			{
				MaintainerWindow.ShowObjectReferences();
			}

			return results.ToArray();
		}

		private static bool LookForObjectsReferencesInHierarchy(Object[] objects, List<HierarchyReferenceItem> results)
		{
			var foundReferences = new Dictionary<int, Dictionary<ExactReferenceData, List<ExactReferenceData>>>(objects.Length);
			var objectInstanceIds = CSObjectTools.GetObjectsInstanceIDs(objects);

			var canceled = HierarchyEntryFinder.FillHierarchyReferenceEntries(objects, (lookAt, lookForInstanceId, settings) =>
			{
				for (var i = 0; i < objectInstanceIds.Length; i++)
				{
					var instanceId = objectInstanceIds[i];
					if (instanceId == lookForInstanceId)
					{
						ExactReferenceData referenced;

						if (!foundReferences.ContainsKey(instanceId))
						{
							foundReferences.Add(instanceId, new Dictionary<ExactReferenceData, List<ExactReferenceData>>());
							referenced = ObjectToReferencingEntry(objects[i]);
							foundReferences[instanceId].Add(referenced, new List<ExactReferenceData>());
						}
						else
						{
							referenced = foundReferences[instanceId].Keys.First();
						}

						var assetPath = CSObjectTools.TryGetObjectAssetPath(lookAt);
						var reference = EntryGenerator.CreateNewReferenceEntry(EntryFinder.currentLocation, lookAt,
							lookAt as GameObject, settings);

						var referenceData = new ExactReferenceData
						{
							assetPath = assetPath,
							reference = reference
						};

						foundReferences[instanceId][referenced].Add(referenceData);
					}
				}
			});

			if (!canceled && foundReferences.Count > 0)
			{
				BuildTree(foundReferences, results);
			}

			if (canceled)
			{
				HierarchyReferencesTab.AutoSelectHierarchyReference = null;
			}

			return canceled;
		}

		private static ExactReferenceData ObjectToReferencingEntry(Object target)
		{
            var referencedObjectAsComponent = target as Component;
            var referencedObjectGameObject = target as GameObject;

            if (referencedObjectAsComponent != null)
            {
            	referencedObjectGameObject = referencedObjectAsComponent.gameObject;
            }

            if (referencedObjectGameObject == null)
            {
	            Debug.LogError(Maintainer.ConstructError("Couldn't find referenced Game Object from object " + target));
	            return new ExactReferenceData();
            }

            var referencedSettings = new EntryAddSettings
            {
            	componentName = referencedObjectAsComponent != null ? CSComponentTools.GetComponentName(referencedObjectAsComponent) : null,
            	componentIndex = referencedObjectAsComponent != null ? CSComponentTools.GetComponentIndex(referencedObjectAsComponent) : -1,
            	componentInstanceId = referencedObjectAsComponent != null ? referencedObjectAsComponent.GetInstanceID() : -1,
            };

            var reference = EntryGenerator.CreateNewReferenceEntry(EntryFinder.currentLocation,
	            referencedObjectGameObject,
	            referencedObjectGameObject, referencedSettings);

            var assetPath = CSObjectTools.TryGetObjectAssetPath(target);

            var result = new ExactReferenceData
			{
				reference = reference,
				assetPath = assetPath
			};

            return result;
		}

		private static void BuildTree(Dictionary<int, Dictionary<ExactReferenceData, List<ExactReferenceData>>> foundReferences, List<HierarchyReferenceItem> results)
		{
			var id = 0;

			var root = new HierarchyReferenceItem
			{
				id = id,
				name = "root",
				depth = -1
			};
			results.Add(root);

			foreach (var foundReference in foundReferences)
			{
				id ++;

				var keyValue = foundReference.Value.First();
				var referenceData = keyValue.Key;
				var referencedItem = new HierarchyReferenceItem
				{
					id = id,
					name = referenceData.reference.transformPath,
					depth = 0,
					reference = referenceData.reference
				};
				referencedItem.SetAssetPath(referenceData.assetPath);

				results.Add(referencedItem);

				foreach (var entryData in keyValue.Value)
				{
					id ++;

					var childReferenceData = entryData;
					var referencingItem = new HierarchyReferenceItem
					{
						id = id,
						name = childReferenceData.reference.transformPath,
						depth = 1,
						reference = childReferenceData.reference
					};
					referencingItem.SetAssetPath(childReferenceData.assetPath);

					results.Add(referencingItem);
				}
			}
		}

		private static IList<Object> GetFoundObjects(ICollection<Object> objects, List<HierarchyReferenceItem> results)
		{
			var result = new List<Object>(objects.Count);

			foreach (var o in objects)
			{
				var componentInstanceId = -1;
				var objectInstanceId = -1;
				if (o is Component)
				{
					objectInstanceId = (o as Component).gameObject.GetInstanceID();
					componentInstanceId = o.GetInstanceID();
				}
				else
				{
					objectInstanceId = o.GetInstanceID();
				}
				
				foreach (var element in results)
				{
					if (element.depth != 0) continue;

					var reference = element.reference;
					if (reference == null) continue;

					if (objectInstanceId == reference.objectInstanceId &&
					    componentInstanceId == reference.componentInstanceId)
					{
						result.Add(o);
						break;
					}
				}
			}

			return result;
		}

		private static void SaveLastSearched(IList<Object> objects)
		{
			var resultsCount = objects.Count;
			var showProgress = resultsCount > 500000;

			if (showProgress) EditorUtility.DisplayProgressBar(ReferencesFinder.ModuleName, "Parsing results...", 0);

			var rootItems = new List<int>(resultsCount);
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

				var result = objects[i];
				rootItems.Add(result.GetInstanceID());
			}

			SearchResultsStorage.HierarchyReferencesLastSearched = rootItems.ToArray();
		}
	}
}