#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues
{
	using System.Collections.Generic;
	using System.Text;
	using UnityEditorInternal;
	using UnityEditor;
	using Core;
	using Detectors;
	using Tools;

	internal static class SettingsChecker
	{
		public static List<IssueRecord> CheckSettingsAssetForMissingReferences(AssetInfo asset, AssetSettingsKind kind)
		{
			var result = new List<IssueRecord>();

			// include only supported settings files with object references

			if (kind != AssetSettingsKind.EditorSettings &&
			    kind != AssetSettingsKind.GraphicsSettings &&
			    kind != AssetSettingsKind.DynamicsManager &&
			    kind != AssetSettingsKind.Physics2DSettings &&
			    kind != AssetSettingsKind.PresetManager &&
			    kind != AssetSettingsKind.VFXManager)
			{
				return result;
			}

			var allAssets = AssetDatabase.LoadAllAssetsAtPath(asset.Path);
			if (allAssets == null || allAssets.Length <= 0) return result;

			foreach (var assetObject in allAssets)
			{
				if (assetObject == null)
				{
					return result;
				}

				var traverseInfo = new SerializedObjectTraverseInfo(assetObject);

				CSTraverseTools.TraverseObjectProperties(traverseInfo, (info, property) =>
				{
					if (MissingReferenceDetector.IsPropertyHasMissingReference(property))
					{
						var issue = SettingsIssueRecord.Create(kind, IssueKind.MissingReference, asset.Path,
							property.propertyPath);
						result.Add(issue);
					}
				});
			}

			return result;
		}

		public static List<IssueRecord> CheckSceneSettingsForMissingReferences(AssetInfo sceneAsset)
		{
			var result = new List<IssueRecord>();

			var sceneSettingsObject = CSSettingsTools.GetInSceneLightmapSettings();
			if (sceneSettingsObject != null)
			{
				var initialInfo = new SerializedObjectTraverseInfo(sceneSettingsObject);
				CSTraverseTools.TraverseObjectProperties(initialInfo, (info, property) =>
				{
					if (MissingReferenceDetector.IsPropertyHasMissingReference(property))
					{
						var record = SceneSettingsIssueRecord.Create(SceneSettingsKind.LightmapSettings, IssueKind.MissingReference, sceneAsset.Path, property.propertyPath);
						result.Add(record);
					}
				});
			}

			sceneSettingsObject = CSSettingsTools.GetInSceneRenderSettings();
			if (sceneSettingsObject != null)
			{
				var initialInfo = new SerializedObjectTraverseInfo(sceneSettingsObject);
				CSTraverseTools.TraverseObjectProperties(initialInfo, (info, property) =>
				{
					if (MissingReferenceDetector.IsPropertyHasMissingReference(property))
					{
						var record = SceneSettingsIssueRecord.Create(SceneSettingsKind.RenderSettings, IssueKind.MissingReference, sceneAsset.Path, property.propertyPath);
						result.Add(record);
					}
				});
			}

			return result;
		}

		public static SettingsIssueRecord CheckTagsAndLayers()
		{
			var issueBody = new StringBuilder();

			/* looking for duplicates in layers */

			var layers = new List<string>(InternalEditorUtility.layers);
			layers.RemoveAll(string.IsNullOrEmpty);
			var duplicateLayers = CSArrayTools.FindDuplicatesInArray(layers);

			if (duplicateLayers.Count > 0)
			{
				if (issueBody.Length > 0) issueBody.AppendLine();
				issueBody.Append("Duplicate <b>layer(s)</b>: ");

				foreach (var duplicate in duplicateLayers)
				{
					issueBody.Append('"').Append(duplicate).Append("\", ");
				}
				issueBody.Length -= 2;
			}

			/* looking for duplicates in sorting layers */

			var sortingLayers = new List<string>((string[])CSReflectionTools.GetSortingLayersPropertyInfo().GetValue(null, new object[0]));
			sortingLayers.RemoveAll(string.IsNullOrEmpty);
			var duplicateSortingLayers = CSArrayTools.FindDuplicatesInArray(sortingLayers);

			if (duplicateSortingLayers.Count > 0)
			{
				if (issueBody.Length > 0) issueBody.AppendLine();
				issueBody.Append("Duplicate <b>sorting layer(s)</b>: ");

				foreach (var duplicate in duplicateSortingLayers)
				{
					issueBody.Append('"').Append(duplicate).Append("\", ");
				}
				issueBody.Length -= 2;
			}

			if (issueBody.Length > 0)
			{
				return SettingsIssueRecord.Create(AssetSettingsKind.TagManager, IssueKind.DuplicateLayers, issueBody.ToString());
			}

			return null;
		}
	}
}