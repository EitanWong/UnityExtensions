#region copyright
// -------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// -------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References
{
	using System.Collections.Generic;
	using Core;

	internal class AssetConjunctions
	{
		public AssetInfo asset;
		public readonly List<TreeConjunction> conjunctions = new List<TreeConjunction>();
	}
}