#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References
{
	using Core;
	using System;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Item describing object reference in hierarchy (in scene or prefab).
	/// </summary>
	[Serializable]
	public class HierarchyReferenceItem : TreeItem
	{
		/// <summary>
		/// AssetDatabase-compatible path to the asset.
		/// </summary>
		public string AssetPath
		{
			get
			{
				return AssetDatabase.GUIDToAssetPath(assetGUID);
			}
		}
		
		/// <summary>
		/// Exact reference description.
		/// </summary>
		public ReferencingEntryData Reference
		{
			get { return reference; }
		}
		
		[SerializeField]
		internal string assetGUID;

		[SerializeField]
		internal ReferencingEntryData reference;
		
		internal void SetAssetPath(string newPath)
		{
			assetGUID = AssetDatabase.AssetPathToGUID(newPath);
		}

		protected override bool Search(string searchString)
		{
			return reference.transformPath.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) != -1;
		}
	}
}