#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References
{
	using System.Collections.Generic;
	using Core;

	internal class TreeConjunction
	{
		public readonly List<ProjectReferenceItem> treeElements = new List<ProjectReferenceItem>();
		public AssetInfo referencedAsset;
		public ReferencedAtInfo referencedAtInfo;
	}
}