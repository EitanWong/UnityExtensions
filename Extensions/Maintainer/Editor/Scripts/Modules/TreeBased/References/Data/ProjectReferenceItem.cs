#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References
{
	using System;
	using Core;
	using UnityEngine;

	/// <summary>
	/// Item describing asset reference in project.
	/// </summary>
	[Serializable]
	public class ProjectReferenceItem : TreeItem
	{
		/// <summary>
		/// AssetDatabase-compatible path to the asset.
		/// </summary>
		public string AssetPath
		{
			get { return assetPath; }
		}

		[SerializeField] internal string assetPath;
		[SerializeField] internal string assetTypeName;
		[SerializeField] internal long assetSize;
		[SerializeField] internal string assetSizeFormatted;
		[SerializeField] internal bool assetIsTexture;
		[SerializeField] internal bool isReferenced;
		[SerializeField] internal AssetSettingsKind assetSettingsKind;
		[SerializeField] internal ReferencingEntryData[] referencingEntries;

		protected override bool Search(string searchString)
		{
			return assetPath.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) != -1;
		}
	}
}