#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;

	[Serializable]
	internal class ReferencedAtAssetInfo : ReferencedAtInfo
	{
		public AssetInfo assetInfo;
	}
}