#region copyright
//---------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
//---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues
{
	/// <summary>
	/// Kinds of Issues Finder results items.
	/// </summary>
	public enum IssueKind
	{
		/* object issues */

		MissingComponent = 0,
		DuplicateComponent = 50,
		MissingReference = 100,
		MissingPrefab = 300,
		UnnamedLayer = 800,
		HugePosition = 900,
		InconsistentTerrainData = 1100,

#if UNITY_2019_1_OR_NEWER
		ShaderError = 1200,
#endif

		/* project settings issues */

		DuplicateLayers = 3010,
		Error = 5000,
		Other = 100000
	}
}