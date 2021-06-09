#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References.Routines
{
	using System.Collections.Generic;
	using System.Linq;
	using Core;
	using Settings;
	using Tools;

	internal static class ProjectScopeReferencesTreeBuilder
	{
		public static ProjectReferenceItem BuildTreeBranch(AssetInfo referencedAsset, int depth, int id, List<AssetConjunctions> conjunctionInfoList, List<ProjectReferenceItem> results)
		{
			// excludes assets without references if necessary and excludes assets which were ignored
			if (AssetIsFilteredOut(referencedAsset, depth)) return null;

			var assetPath = referencedAsset.Path;
			var assetType = referencedAsset.Type;
			string assetTypeName;

			if (referencedAsset.SettingsKind == AssetSettingsKind.NotSettings)
			{
				assetTypeName = assetType != null && !string.IsNullOrEmpty(assetType.Name) ? assetType.Name : "Unknown type";
			}
			else
			{
				assetTypeName = "Settings Asset";
			}

			var element = new ProjectReferenceItem
			{
				id = id + results.Count,
				name = CSPathTools.NicifyAssetPath(referencedAsset.Path, referencedAsset.Kind),
				assetPath = assetPath,
				assetTypeName = assetTypeName,
				assetSize = referencedAsset.Size,
				assetSizeFormatted = CSEditorTools.FormatBytes(referencedAsset.Size),
				assetIsTexture = assetType != null && (assetType.BaseType == CSReflectionTools.textureType),
				assetSettingsKind = referencedAsset.SettingsKind,
				isReferenced = referencedAsset.referencedAtInfoList != null && referencedAsset.referencedAtInfoList.Length > 0,
				depth = depth
			};
			results.Add(element);

			var recursionId = CheckParentsForRecursion(element, results);

			if (recursionId > -1)
			{
				element.name += " [RECURSION]";
				element.recursionId = recursionId;
				return element;
			}

			if (referencedAsset.referencedAtInfoList != null && referencedAsset.referencedAtInfoList.Length > 0)
			{
				foreach (var referencedAtInfo in referencedAsset.referencedAtInfoList)
				{
					//if (CSFilterTools.IsValueMatchesAnyFilter(referencedAtInfo.assetInfo.Path, MaintainerSettings.References.pathIgnoresFilters)) continue;
					if (referencedAtInfo.assetInfo.Kind == AssetKind.FromPackage) continue;

					var newDepth = depth + 1;

					if (newDepth > 1) continue;

					var childElement = BuildTreeBranch(referencedAtInfo.assetInfo, newDepth, id, conjunctionInfoList, results);
					if (childElement == null) continue;

					var referencedAtType = referencedAtInfo.assetInfo.Type;

					if (referencedAtType == CSReflectionTools.gameObjectType ||
					    referencedAtType == CSReflectionTools.sceneAssetType ||
					    referencedAtType == CSReflectionTools.monoScriptType ||
					    referencedAtType == CSReflectionTools.monoBehaviourType ||
					    referencedAtType != null && referencedAtType.BaseType == CSReflectionTools.scriptableObjectType)
					{
						if (referencedAtInfo.entries != null)
						{
							childElement.referencingEntries = referencedAtInfo.entries;
						}
						else
						{
							var collectedData = conjunctionInfoList.FirstOrDefault(d => d.asset == referencedAtInfo.assetInfo);

							if (collectedData == null)
							{
								collectedData = new AssetConjunctions();
								conjunctionInfoList.Add(collectedData);
								collectedData.asset = referencedAtInfo.assetInfo;
							}

							var tc = collectedData.conjunctions.FirstOrDefault(c => c.referencedAsset == referencedAsset);

							if (tc == null)
							{
								tc = new TreeConjunction
								{
									referencedAsset = referencedAsset,
									referencedAtInfo = referencedAtInfo
								};

								collectedData.conjunctions.Add(tc);
							}
							tc.treeElements.Add(childElement);
						}
					}
				}
			}

			return element;
		}

		private static bool AssetIsFilteredOut(AssetInfo referencedAsset, int depth)
		{
			if (UserSettings.References.showAssetsWithoutReferences || depth != 0) return false;
			if (referencedAsset.referencedAtInfoList.Length == 0) return true;

			var allIgnored = true;
			foreach (var referencedAtInfo in referencedAsset.referencedAtInfoList)
			{
				if (CSFilterTools.IsValueMatchesAnyFilter(referencedAtInfo.assetInfo.Path, ProjectSettings.References.pathIgnoresFilters)) continue;
				if (referencedAtInfo.assetInfo.Kind == AssetKind.FromPackage) continue;

				allIgnored = false;
				break;
			}

			return allIgnored;
		}

		private static int CheckParentsForRecursion(ProjectReferenceItem item, List<ProjectReferenceItem> items)
		{
			var result = -1;

			var lastDepth = item.depth;
			for (var i = items.Count - 1; i >= 0; i--)
			{
				var previousItem = items[i];
				if (previousItem.depth >= lastDepth) continue;

				lastDepth = previousItem.depth;
				if (item.assetPath != previousItem.assetPath) continue;

				result = previousItem.id;
				break;
			}

			return result;
		}
	}
}